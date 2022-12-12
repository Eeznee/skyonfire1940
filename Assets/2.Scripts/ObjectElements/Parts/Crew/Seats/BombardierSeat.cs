using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BombardierSeat : CrewSeat
{
    public float fov = 12f;
    public float maxAngle = 80f;
    public float minAngle = -20f;
    public float rotationSpeed = 10f;
    public float maxSideAngle = 30f;
    public float maxYawInput = 0.3f;

    public float speedSetting = 350f / 3.6f;
    public float relativeAltSetting = 1000f;
    public float angle = 60f;
    public float dropAngle = 60f;
    public float sideAngle = 0f;

    //0 is manual, 1 is free and 2 is auto
    public int viewMode = 0;
    public int amountSelection = 0;
    public int intervalSelection = 2;

    readonly int[] dropAmounts = { 1, 2, 4, 8, 16, 100 };
    readonly float[] dropIntervals = { 0f, 0.05f, 0.1f, 0.2f, 0.5f };

    float targetAltitude = 0f;
    public bool releaseSequence;
    public bool stabilized;

    public override int Priority() { return 1; }
    public override void ResetSeat()
    {
        base.ResetSeat();
        releaseSequence = false;
    }
    public override SeatInterface SeatUI() { return SeatInterface.Bombardier; }

    public override Vector3 HeadPosition(bool player)
    {
        return defaultPOV.position;
    }

    private void Update()
    {
        if (viewMode == 2)
            angle = Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad) - Time.deltaTime * speedSetting / relativeAltSetting) * Mathf.Rad2Deg;
    }
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);

        //Player inputs
        float elevationInput = PlayerActions.instance.actions.General.Pitch.ReadValue<float>();
        float sideInput = PlayerActions.instance.actions.General.Roll.ReadValue<float>();

        //Bomb drop angle calculations
        float timeToFall = Mathf.Sqrt(2f / -Physics.gravity.y * relativeAltSetting);
        float horizontalDistance = timeToFall * speedSetting;
        dropAngle = Mathf.Atan(horizontalDistance / relativeAltSetting) * Mathf.Rad2Deg;
        speedSetting = data.gsp;
        relativeAltSetting = data.altitude - targetAltitude;

        switch (viewMode)
        {
            case 0:
                angle = dropAngle;
                sideAngle = 0f;
                break;
            case 1:
                angle += elevationInput * Time.deltaTime * rotationSpeed;
                sideAngle += sideInput * Time.deltaTime * rotationSpeed;
                break;
            case 2:
                angle += elevationInput * Time.deltaTime * rotationSpeed;
                sideAngle = 0f;
                break;
        }

        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        sideAngle = Mathf.Clamp(sideAngle, -maxSideAngle, maxSideAngle);

        //Apply rotation to the bomb sight
        zoomedPOV.localRotation = Quaternion.identity;
        Quaternion stabilisedRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(aircraft.transform.forward, Vector3.up));
        zoomedPOV.rotation = Quaternion.RotateTowards(zoomedPOV.rotation, stabilisedRot, 15f);
        zoomedPOV.Rotate(Vector3.left * (angle - 90f));
        zoomedPOV.Rotate(Vector3.up * sideAngle);

        if (zoomedPOV.forward.y >= 0f) targetAltitude = 0f;
        else
        {
            float factor = (targetAltitude - zoomedPOV.position.y) / zoomedPOV.forward.y;
            Vector3 point = zoomedPOV.position + zoomedPOV.forward * factor;
            targetAltitude = GameManager.map.HeightAtPoint(point);
        }

        headLookDirection = -transform.root.up;
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        aircraft.controlInput.y = -PlayerActions.instance.actions.General.Rudder.ReadValue<float>() * maxYawInput;
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);

        SofAircraft leader = GameManager.squadrons[aircraft.squadronId][0];
        if (leader == aircraft) return;
        BombardierSeat leadSight = leader.bombSight;

        intervalSelection = leadSight.intervalSelection;
        amountSelection = leadSight.amountSelection;

        if (aircraft.bombBay.stateInput != leader.bombBay.stateInput) aircraft.SetBombBay();
        if (leadSight.releaseSequence && !releaseSequence) StartReleaseSequence();
    }
    public void ToggleInterval()
    {
        intervalSelection = (intervalSelection + 1) % dropIntervals.Length;
    }
    public void ToggleAmount()
    {
        amountSelection = (amountSelection + 1) % dropAmounts.Length;
    }
    public void ToggleMode()
    {
        viewMode = (viewMode + 1) % 3;
    }
    public void StartReleaseSequence()
    {
        StartCoroutine(ReleaseSequence());
    }

    IEnumerator ReleaseSequence()
    {
        if (releaseSequence) yield break;

        releaseSequence = true;
        int totalBombs = dropAmounts[amountSelection];
        int bombsReleased = 0;
        float interval = dropIntervals[intervalSelection];
        float releaseCounter = interval * totalBombs;
        float counter = 0f;

        while (counter <= releaseCounter)
        {
            while ((interval == 0f || bombsReleased <= counter / interval) && bombsReleased < totalBombs)
            {
                aircraft.DropBomb();
                bombsReleased++;
            }
            counter += Time.deltaTime;
            yield return null;
        }

        releaseSequence = false;
        yield return null;
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BombardierSeat))]
public class BombardierSeatEditor : CrewSeatEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        //
        BombardierSeat seat = (BombardierSeat)target;

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Bombsight settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        seat.fov = EditorGUILayout.FloatField("Field Of View", seat.fov);
        seat.maxAngle = EditorGUILayout.FloatField("Max Angle", seat.maxAngle);
        seat.minAngle = EditorGUILayout.FloatField("Min Angle", seat.minAngle);
        seat.maxSideAngle = EditorGUILayout.FloatField("Max Angle Sides", seat.maxSideAngle);
        seat.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", seat.rotationSpeed);
        seat.maxYawInput = EditorGUILayout.FloatField("Max Yaw Input", seat.maxYawInput);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(seat);
            EditorSceneManager.MarkSceneDirty(seat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif