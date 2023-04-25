using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class GunnerSeat : CrewSeat
{
    public bool mainGun = false;
    public Turret turret;
    public Transform gunPovTarget;
    public HydraulicSystem progressiveHydraulic;
    private float perlinRandomizer;

    private Vector3 currentLead;
    private float difficulty;

    const float leadMatchNoob = 12f;
    const float leadMatchExpert = 25f;

    const float maxRange = 700f;
    const float maxRangeAA = 3500f;
    const float sprayTargetCycle = 4f;
    const float burstPerlinNoob = 0.42f;
    const float burstPerlinExpert = 0.52f;
    public override SeatInterface SeatUI() { return SeatInterface.Gunner; }

    public override Vector3 CrosshairDirection()
    {
        if (turret) return turret.FiringDirection() * 500f;
        return base.CrosshairDirection();
    }
    public override int Priority()
    {
        if (mainGun && aircraft && aircraft.crew[0] == PlayerManager.player.crew) return -1;
        if (target) return 2;
        return 0;
    }

    private void Update()
    {
        if (turret && turret.transform.root != transform.root)
        {
            turret = null;
            NewGrips(null, null);
        }
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        InvokeRepeating("GetTarget", Random.Range(0f, 2f), 1.5f);
        difficulty = aircraft ? aircraft.difficulty : 1f;
        perlinRandomizer = Random.Range(0f, 1000f);
        currentLead = Vector3.zero;
    }
    public override void ResetSeat()
    {
        base.ResetSeat();
    }

    public override Vector3 HeadPosition(bool player)
    {
        bool useGunPov = gunPovTarget && gunPovTarget.root == transform.root && !(player && CameraFov.zoomed);
        if (useGunPov)  return (gunPovTarget.position + defaultPOV.position) * 0.5f;
        else return base.HeadPosition(player);
    }
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);

        if (progressiveHydraulic)
        {
            float input = PlayerActions.Gunner().Hydraulics.ReadValue<float>();
            progressiveHydraulic.SetDirection(Mathf.RoundToInt(input));
        }
        if (!turret) return;
        if (!handsBusy) NewGrips(defaultRightHand, defaultLeftHand);

        Vector2 basic = PlayerActions.Gunner().BasicAxis.ReadValue<Vector2>();
        float special = PlayerActions.Gunner().SpecialAxis.ReadValue<float>();
        turret.SetDirectionSemi(PlayerCamera.directionInput,special);
        bool firing = PlayerActions.Gunner().Fire.ReadValue<float>() > 0.5f;
        turret.Operate(firing,false);
        if (turret.Firing()) VibrationsManager.SendVibrations(0.2f, 0.1f, aircraft);
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);

        if (handsBusy || !turret) return;
        if (mainGun && PlayerManager.player.crew == aircraft.crew[0]) return;

        bool firing = false;

        if (target && sofObject && !sofObject.destroyed) firing = AiTargeting();

        if (target) headLookDirection = targetTr.position - transform.position;
        else headLookDirection = turret.FiringDirection();

        turret.Operate(firing,true);
    }
    public bool AiTargeting()
    {
        AI.GunnerTargetingData targetData = new AI.GunnerTargetingData(turret, target.data.rb); //Compute Ballistic data
        float t = Ballistics.InterceptionTime(turret.MuzzleVelocity() * 0.95f, targetData.dir, targetData.relativeVel);
        Vector3 gravityLead = -Physics.gravity * t * t * 0.5f;
        Vector3 speedLead = t * target.data.rb.velocity;
        Vector3 targetLead = speedLead + gravityLead - rb.velocity * t * 1.2f;
        targetLead *= Mathf.Lerp(-0.7f + difficulty * 1.1f, 3f - 1.35f * difficulty, Mathf.PerlinNoise(perlinRandomizer, Time.time / 3f));

        if (turret.Firing()) //Spray when firing
        {
            float sprayTarget = Mathf.PingPong(Time.time * 2f / sprayTargetCycle, 2f) - 1f;
            targetLead += targetTr.right * sprayTarget * target.wingSpan / 3f;
        }

        currentLead = Vector3.MoveTowards(currentLead, targetLead, Mathf.Lerp(leadMatchNoob, leadMatchExpert, difficulty) * Time.deltaTime);
        currentLead += Random.insideUnitSphere * targetData.distance * 0.01f * Mathf.Lerp(0.8f, 0.15f, difficulty);

        Vector3 targetDir = targetTr.position - transform.position + currentLead;
        turret.SetDirectionAuto(targetDir); //Aim target

        float gunsAngle = Vector3.Angle(targetDir, turret.FiringDirection());
        float minAngle = Mathf.Max(target.wingSpan / targetData.distance * Mathf.Rad2Deg,1f);

        //Solution temporaire pour 40 mm
        if (!aircraft) turret.SetFuze(targetData.distance);

        if (gunsAngle > minAngle || targetData.distance > (aircraft ? maxRange : maxRangeAA)) return false;
        return Mathf.PerlinNoise(perlinRandomizer, Time.time) < Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty);
    }
    public void GetTarget()
    {
        spotted = visibility.Spot();
        target = TargetPicker.PickTargetGunner(turret, spotted, target);
        targetTr = target ? target.transform : null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GunnerSeat))]
public class GunnerSeatEditor : CrewSeatEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GunnerSeat seat = (GunnerSeat)target;

        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Gunner Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;

        seat.turret = EditorGUILayout.ObjectField("Turret", seat.turret, typeof(Turret), true) as Turret;
        seat.gunPovTarget = EditorGUILayout.ObjectField("Gun Pov Target", seat.gunPovTarget, typeof(Transform), true) as Transform;
        seat.progressiveHydraulic = EditorGUILayout.ObjectField("Progressive hydraulics", seat.progressiveHydraulic, typeof(HydraulicSystem), true) as HydraulicSystem;
        seat.mainGun = EditorGUILayout.Toggle("Can Use As Main Gun", seat.mainGun);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(seat);
            EditorSceneManager.MarkSceneDirty(seat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif