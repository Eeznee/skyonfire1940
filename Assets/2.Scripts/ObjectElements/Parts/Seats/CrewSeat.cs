using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public enum SeatInterface { Empty, Pilot, Gunner, Bombardier }

[System.Serializable]
public class SeatPath
{
    public int crew = 0;
    public int seat = 0;
    public SeatPath(int _crew,int _seat)
    {
        crew = _crew;
        seat = _seat;
    }
    public CrewMember Crew(SofObject sof) { return sof.crew[crew]; }
    public CrewSeat Seat(SofObject sof) { return sof.crew[crew].seats[seat]; }
}
public class CrewSeat : BasicSeat
{
    public HydraulicSystem canopy;
    public float audioRatio = 0.3f;
    public float closedRatio = 0.9f;

    public Visibility visibility;

    public Gun[] reloadableGuns;
    public Transform magTrash;
    private Vector3 localDirection;

    const float magRemoveTime = 0.5f;
    const float magTravelSpeed = 1f;

    protected List<SofAircraft> spotted;
    public SofAircraft target;
    protected Transform targetTr;

    public Vector3 DefaultDirection()
    {
        return data.tr.TransformDirection(localDirection);
    }
    public virtual Vector3 CrosshairDirection()
    {
        return DefaultDirection() * 500f;
    }
    public override float CockpitAudio()
    {
        if (!aircraft) return 0f;
        if (canopy && !canopy.disabled) {
            return Mathf.Lerp(audioRatio, closedRatio, Mathv.SmoothStart(1f - canopy.state,5));
        }
        else return audioRatio;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            localDirection = transform.root.InverseTransformDirection(transform.forward);
            localDirection = new Vector3(localDirection.x, 0f, localDirection.z).normalized;

            if (!magTrash) magTrash = transform;
            if (!zoomedPOV) zoomedPOV = defaultPOV;
            visibility.Initialize(this);
            spotted = new List<SofAircraft>();
        }
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);
        if (handsBusy) return;
        foreach (Gun gun in reloadableGuns)
        {
            if (!gun.PossibleFire() && !gun.reloading) { StartCoroutine(Reload(gun)); ; return;  }  //Check Reloadable guns
        }
    }
    public void TryReload()
    {
        if (handsBusy) return;
        for (int i = 0; i < reloadableGuns.Length; i++)
        {
            bool reloadable = reloadableGuns[i].magazine.ammo < reloadableGuns[i].magazine.capacity - 1 || !reloadableGuns[i].PossibleFire();
            reloadable &= reloadableGuns[i].transform.root == transform.root;
            if (reloadable && !reloadableGuns[i].reloading)
            {
                StartCoroutine(Reload(reloadableGuns[i]));
                return;
            }
        }
    }
    protected IEnumerator Reload(Gun gun)
    {
        if (gun.magStorage.magsLeft == 0) yield break;

        handsBusy = true;
        gun.reloading = true;

        //If mag change
        if (gun.magazine.ammo == 0 || gun.PossibleFire())
        {
            //PART !: Remove Old Magazine
            AmmoContainer oldMagRef = gun.magazine;
            Transform oldMag = gun.magazine.transform;
            leftHandGrip = oldMagRef.grip;
            yield return new WaitForSeconds(0.4f);  //Wait for hand to attach

            gun.RemoveMagazine();
            oldMag.parent = transform;

            Vector3 start = oldMag.localPosition;
            Vector3 target = transform.InverseTransformPoint(magTrash.position);
            Vector3 correctedTrajectory = target - (start + oldMagRef.ejectVector);
            float totalTime = correctedTrajectory.magnitude / magTravelSpeed + magRemoveTime;
            float count = 0f;

            //Vectors
            Vector3 ejectVector = transform.InverseTransformDirection(oldMag.TransformDirection(oldMagRef.ejectVector));
            Vector3 projectedVector = Vector3.Project(correctedTrajectory, -oldMag.TransformDirection(oldMagRef.ejectVector).normalized);
            Vector3 lateralVector = correctedTrajectory - projectedVector;

            //Slopes
            Slope2 projected = new Slope2(0f, 0f, magRemoveTime, 0f, totalTime, 1f);
            projected.Normalize();
            Slope2 lateral = new Slope2(0f, 0f, magRemoveTime, 1f, totalTime, 0f);
            lateral.Normalize();
            Slope2 eject = new Slope2(0f, 1f, magRemoveTime, 0f, totalTime, 0f);
            eject.Normalize();

            while (count < totalTime)//Animate
            {
                oldMag.localPosition = start + projectedVector * projected.Integral(count) + lateralVector * lateral.Integral(count) + ejectVector * eject.Integral(count);
                count += Time.deltaTime;
                yield return null;
            }
            Destroy(oldMag.gameObject);

            //PART 2: Insert Magazine
            Magazine mag = gun.magStorage.GetMag();
            Transform newMag = mag.transform;
            newMag.parent = gun.transform;
            newMag.localPosition = target = gun.transform.InverseTransformPoint(transform.TransformPoint(-Vector3.right * 0.2f));
            newMag.localRotation = Quaternion.identity;
            leftHandGrip = mag.grip;

            yield return new WaitForSeconds(1f); //Wait for hand to attach (and find the mag)

            start = gun.magazineLocalPos;
            correctedTrajectory = target - (start + oldMagRef.ejectVector);
            ejectVector = mag.ejectVector;

            //Vectors (slopes are the same)
            projectedVector = Vector3.Project(correctedTrajectory, -mag.ejectVector.normalized);
            lateralVector = correctedTrajectory - projectedVector;

            count = totalTime;
            while (count > 0f)//Animate
            {
                newMag.localPosition = start + projectedVector * projected.Integral(count) + lateralVector * lateral.Integral(count) + ejectVector * eject.Integral(count);
                count -= Time.deltaTime;
                yield return null;
            }
            gun.LoadMagazine(mag);
            yield return new WaitForSeconds(0.2f);
            leftHandGrip = defaultLeftHand;
        }

        //PART 3: Cock the gun
        if (!gun.PossibleFire())
        {
            if (gun.bolt)
            {
                rightHandGrip = gun.bolt.grip;
                yield return new WaitForSeconds(0.3f);
                gun.bolt.CycleBoltAnimation();

                while (gun.bolt.animatedPulling)
                {
                    yield return null;
                }
                rightHandGrip = defaultRightHand;
            }
        }
        gun.reloading = false;
        handsBusy = false;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(CrewSeat))]
public class CrewSeatEditor : BasicSeatEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        
        CrewSeat seat = (CrewSeat)target;

        seat.audioRatio = EditorGUILayout.Slider("Audio Cockpit Ratio", seat.audioRatio, 0f, 1f);
        seat.canopy = EditorGUILayout.ObjectField("Linked Canopy", seat.canopy, typeof(HydraulicSystem), true) as HydraulicSystem;
        if (seat.canopy) seat.closedRatio = EditorGUILayout.Slider("Closed Canopy Ratio", seat.closedRatio, 0f, 1f);

        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Visibility", MessageType.None);
        GUI.color = GUI.backgroundColor;
        SerializedProperty visibility = serializedObject.FindProperty("visibility");
        EditorGUILayout.PropertyField(visibility, true);

        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Weapons Management", MessageType.None);
        GUI.color = GUI.backgroundColor;

        SerializedProperty reloadableGuns = serializedObject.FindProperty("reloadableGuns");
        EditorGUILayout.PropertyField(reloadableGuns, true);
        if (seat.reloadableGuns != null && seat.reloadableGuns.Length > 0)
        {
            seat.magTrash = EditorGUILayout.ObjectField("Magazine Trash", seat.magTrash, typeof(Transform), true) as Transform;
        }
        

        if (GUI.changed)
        {
            EditorUtility.SetDirty(seat);
            EditorSceneManager.MarkSceneDirty(seat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif