using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CrewMember : Part
{
    //State
    public float audioCockpitRatio = 1f;
    public int currentSeat = 0;

    public CrewSeat[] seats;
    public Parachute parachute;

    private SkinnedMeshRenderer meshRend;
    public Mesh headLessModel;
    private Mesh defaultModel;

    public HumanBody body;

    private float bailingCount;
    private bool bailingOut = false;

    const float bailOutTime = 3f;
    const float minBailOutAltitude = 30f;
    const float minCrashTime = 2f;

    public override float Mass() { return HumanBody.Weight(); }
    public override float EmptyMass() { return 0f; }
    public CrewSeat Seat() { return seats[currentSeat]; }
    public SeatInterface Interface() { return Seat().SeatUI(); }
    public string Action() { return seats[currentSeat].Action(); }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            meshRend = GetComponentInChildren<SkinnedMeshRenderer>();
            defaultModel = meshRend.sharedMesh;
            body = new HumanBody(this);
        }
        SwitchSeat(0);
    }

    private void Update()
    {
        //LOD
        bool firstPerson = GameManager.player.crew == this && (PlayerCamera.customCam.pos == CamPosition.FirstPerson || GameManager.gm.vr);
        bool isFirstPerson = meshRend.sharedMesh == headLessModel;
        if (firstPerson != isFirstPerson) meshRend.sharedMesh = firstPerson ? headLessModel : defaultModel;
        //Audio
        audioCockpitRatio = seats[currentSeat].CockpitAudio();

        //Conscious or dead conditions
        if (ripped || Time.timeScale == 0f || !sofObject) return;
        body.ApplyForces(data.gForce, Time.deltaTime);
        if ((GameManager.player.crew == this && GameManager.gm.vr) || body.Gloc()) return;

        //Bailout sequence
        bool bailing = bailingOut;
        bailing &= data.relativeAltitude + data.VerticalSpeed * minCrashTime > minBailOutAltitude;
        if (Seat().canopy) bailing &= Seat().canopy.state > 0.5f || Seat().canopy.Destroyed();
        if (bailing)
        {
            bailingCount -= Time.deltaTime;
            if (bailingCount < 0f) Bailout();
            if (GameManager.player.crew == this) Log.Print("Bailout in " + bailingCount.ToString("0.0") + " s", "bailout");
        }

        //Actions in delta time
        if (GameManager.player.crew == this) seats[currentSeat].PlayerUpdate(this);
        else
        {
            //Automatic bail out
            if (aircraft && (aircraft.burning || aircraft.destroyed) && GameManager.player.aircraft != aircraft) StartBailout(Random.Range(1f,3f));
            //Pick the seat with highest priority
            currentSeat = 0;
            for (int i = 1; i < seats.Length; i++) if (seats[i].Priority() > seats[currentSeat].Priority()) currentSeat = i;
            seats[currentSeat].AiUpdate(this);
        }
    }
    private void FixedUpdate()
    {
        //Actions in fixed delta time
        if ((GameManager.player.crew == this && GameManager.gm.vr) || body.Gloc() || ripped || Time.timeScale == 0f || !sofObject) return;
        if (GameManager.player.crew == this) seats[currentSeat].PlayerFixed(this); else seats[currentSeat].AiFixed(this);
    }
    public void SwitchSeat(int seat)
    {
        if (seat == currentSeat) return;
        seats[currentSeat].ResetSeat();
        currentSeat = seat;
        seats[seat].ResetSeat();
        if (GameManager.player.crew == this) PlayerCamera.instance.ResetView(false);
    }
    public void SwitchSeat(){ SwitchSeat((currentSeat +1)%seats.Length);}
    public override void Rip()
    {
        structureDamage = 0f;
        if (aircraft && this == aircraft.crew[0]) { aircraft.hasPilot = false; aircraft.destroyed = true; }
        base.Rip();
    }
    public void StartBailout(float delay)
    {
        if (!aircraft || ripped || bailingOut) return;
        bailingOut = true;
        if (Seat().canopy) Seat().canopy.Set(1f);
        bailingCount = bailOutTime + delay;
    }
    public void CancelBailout()
    {
        if (!aircraft || ripped) return;
        bailingOut = false;
        if (Seat().canopy) Seat().canopy.Set(0f);
    }
    public void Bailout()
    {
        if (!aircraft || ripped) return;
        bailingOut = false;
        if (this == aircraft.crew[0]) { aircraft.hasPilot = false; aircraft.destroyed = true; }
        Parachute para = Instantiate(parachute, transform.position, transform.rotation);
        para.tag = sofObject.tag;
        para.data.rb.velocity = rb.velocity + transform.up * 5f;
        transform.parent = para.transform;
        Seat().ResetSeat();
        seats = new CrewSeat[1] { para.GetComponentInChildren<CrewSeat>() };
        currentSeat = 0;
        seats[0].ResetSeat();
        para.data.Initialize(false);
        meshRend.enabled = true;
        if (GameManager.player.crew == this)
            PlayerCamera.instance.ResetView(false);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CrewMember))]
public class CrewMemberEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        CrewMember crew = (CrewMember)target;

        crew.material = EditorGUILayout.ObjectField("Part Material", crew.material, typeof(PartMaterial), true) as PartMaterial;
        SerializedProperty seats = serializedObject.FindProperty("seats");
        EditorGUILayout.PropertyField(seats, true);
        crew.headLessModel = EditorGUILayout.ObjectField("Head Less Model", crew.headLessModel, typeof(Mesh), false) as Mesh;
        crew.parachute = EditorGUILayout.ObjectField("Parachute", crew.parachute, typeof(Parachute), false) as Parachute;
        GUILayout.Space(15f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Animation References", MessageType.None);
        GUI.color = GUI.backgroundColor;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(crew);
            EditorSceneManager.MarkSceneDirty(crew.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
