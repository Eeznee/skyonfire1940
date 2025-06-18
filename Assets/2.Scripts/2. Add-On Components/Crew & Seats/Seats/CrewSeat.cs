using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Crew Seats/Crew Seat")]
public enum SeatRole { Simple, Pilot, Gunner, Bombardier }
public struct SeatId
{
    public int crewId;
    public int seatId;
    public SeatId(int _crewId, int _seatId)
    {
        crewId = _crewId;
        seatId = _seatId;
    }
    public SeatId(CrewSeat seat)
    {
        crewId = 0;
        seatId = 0;

        SofModular complex = seat.sofModular;
        for (int i = 0; i < complex.crew.Length; i++)
            for (int j = 0; j < complex.crew[i].seats.Count; j++)
                if (complex.crew[i] && complex.crew[i].seats[j] == seat)
                {
                    crewId = i; seatId = j;
                }
    }
}
public class CrewSeat : SofComponent
{
    public SeatRole role;

    public Vector3 externalViewPoint = new Vector3(0f, 2f, -12f);
    public Vector3 goProViewPoint = new Vector3(1f, 0f, 0f);

    public Transform defaultPOV;
    public bool shipGunnerProtectedFromStun = false;

    public HandGrip rightHandGrip = null;
    public HandGrip leftHandGrip = null;
    public FootRest rightFootRest = null;
    public FootRest leftFootRest = null;

    public HydraulicSystem canopy;
    public float audioRatio = 0.3f;
    public float closedRatio = 0.9f;

    public Visibility visibility;
    public Gun[] reloadableGuns;
    public Transform magTrash;


    protected bool handsBusy = false;
    protected HandGrip defaultRightHand;
    protected HandGrip defaultLeftHand;

    private Vector3 flattenedLocalDir;
    public SeatId id { get { return new SeatId(this); } }

    public CrewMember seatedCrew { get; private set; }
    public List<SofAircraft> spotted { get; protected set; }
    public SofAircraft target { get; protected set; }
    public Transform targetTr { get; protected set; }

    public virtual int Priority => 0;
    public virtual string Action => "Inactive";

    public virtual Vector3 ZoomedHeadPosition => DefaultHeadPosition;
    public virtual Vector3 DefaultHeadPosition => defaultPOV ? defaultPOV.position : transform.position + aircraft.tr.up * 0.75f;
    public virtual Vector3 CameraUp => sofObject.tr.up;
    public virtual Vector3 LookingDirection => sofObject.tr.TransformDirection(flattenedLocalDir);
    public virtual Vector3 CrosshairPosition => defaultPOV.position + sofObject.tr.forward * (aircraft ? aircraft.Convergence : 1000f);
    public float CockpitAudioRatio
    {
        get
        {
            if (!aircraft) return 0f;
            if (!canopy || canopy.disabled) return audioRatio;
            return Mathf.Lerp(audioRatio, closedRatio, Mathv.SmoothStart(1f - canopy.state, 5));
        }
    }
    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);

        role = SeatRole.Simple;
        if (GetComponent<PilotSeat>()) role = SeatRole.Pilot;
        if (GetComponent<GunnerSeat>()) role = SeatRole.Gunner;
        if (GetComponent<BombardierSeat>()) role = SeatRole.Bombardier;

        defaultRightHand = rightHandGrip;
        defaultLeftHand = leftHandGrip;

        flattenedLocalDir = tr.root.InverseTransformDirection(transform.forward);
        flattenedLocalDir.y = 0f;
        flattenedLocalDir.Normalize();
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        if (!magTrash) magTrash = transform;
        visibility.Initialize(this);
        spotted = new List<SofAircraft>();
    }
    public virtual void OnCrewEnters(CrewMember newCrew)
    {
        seatedCrew = newCrew;
    }
    public virtual void OnCrewLeaves()
    {
        seatedCrew = null;
    }

    public virtual void PlayerUpdate(CrewMember crew)
    {

    }
    public virtual void AiUpdate(CrewMember crew)
    {
        TryReload(true);
    }
    protected void NewGrips(HandGrip right, HandGrip left)
    {
        rightHandGrip = right;
        leftHandGrip = left;
    }
    public void TryReload(bool onlyEmptyGuns)
    {
        if (handsBusy) return;
        foreach (Gun gun in reloadableGuns)
        {
            if (!gun.CanBeReloaded()) continue;
            if (gun.sofModular != sofModular) continue;
            if (onlyEmptyGuns && gun.magazine.ammo > 0) continue;

            StartCoroutine(Reload(gun));
            return;
        }

    }

    const float magRemoveTime = 0.5f;
    const float magTravelSpeed = 1f;
    protected IEnumerator Reload(Gun gun)
    {
        if (!gun || !gun.magStorage || gun.magStorage.magsLeft == 0) yield break;

        handsBusy = true;
        gun.reloading = true;

        bool noMagChange = gun.magazine.ammo > 0 && gun.MustBeCocked();
        if (!noMagChange)
        {
            //PART !: Remove Old Magazine
            AmmoContainer oldMagRef = gun.magazine;
            Transform oldMag = gun.magazine.transform;
            leftHandGrip = oldMagRef.grip;
            yield return new WaitForSeconds(1f);  //Wait for hand to attach

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
            newMag.parent = gun.magazineAttachPoint.transform;
            newMag.localPosition = target = gun.magazineAttachPoint.InverseTransformPoint(transform.TransformPoint(-Vector3.right * 0.2f));
            newMag.localRotation = Quaternion.identity;
            leftHandGrip = mag.grip;

            yield return new WaitForSeconds(1f); //Wait for hand to attach (and find the mag)

            correctedTrajectory = target - oldMagRef.ejectVector;
            ejectVector = mag.ejectVector;

            //Vectors (slopes are the same)
            projectedVector = Vector3.Project(correctedTrajectory, -mag.ejectVector.normalized);
            lateralVector = correctedTrajectory - projectedVector;

            count = totalTime;
            while (count > 0f)//Animate
            {
                count = Mathf.Max(0f, count - Time.deltaTime);
                newMag.localPosition = projectedVector * projected.Integral(count) + lateralVector * lateral.Integral(count) + ejectVector * eject.Integral(count);
                yield return null;
            }
            gun.LoadMagazine(mag);
            yield return new WaitForSeconds(0.8f);
            leftHandGrip = defaultLeftHand;
        }

        //PART 3: Cock the gun
        if (gun.MustBeCocked())
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