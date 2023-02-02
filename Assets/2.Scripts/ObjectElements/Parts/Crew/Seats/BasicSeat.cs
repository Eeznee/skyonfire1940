using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BasicSeat : ObjectElement
{
    public float audioCockpitRatio;
    public Vector3 headLookDirection = new Vector3(1f, 0f, 0f);
    protected bool handsBusy = false;

    public Transform defaultPOV;
    public Transform zoomedPOV;

    public HandGrip rightHandGrip = null;
    public HandGrip leftHandGrip = null;
    protected HandGrip defaultRightHand;
    protected HandGrip defaultLeftHand;

    public FootRest rightFootRest = null;
    public FootRest leftFootRest = null;

    public virtual float CockpitAudio()
    {
        return 0f;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            headLookDirection = transform.forward;
            defaultRightHand = rightHandGrip;
            defaultLeftHand = leftHandGrip;
        }
    }
    public virtual int Priority() { return 0; }
    public virtual Vector3 HeadPosition(bool player)
    {
        Transform pov = player && CameraFov.zoomed ? zoomedPOV : defaultPOV;
        if (pov.root == transform.root) return pov.position;
        else return transform.position + data.up * 0.75f;
    }
    public virtual string Action()
    {
        return "Inactive";
    }
    protected void NewGrips(HandGrip right, HandGrip left)
    {
        rightHandGrip = right;
        leftHandGrip = left;
    }
    public virtual SeatInterface SeatUI() { return SeatInterface.Empty; }
    public virtual void ResetSeat()
    {
    }
    public virtual void PlayerUpdate(CrewMember crew)
    {
        headLookDirection = Camera.main.transform.forward;
    }
    public virtual void PlayerFixed(CrewMember crew)
    {

    }
    public virtual void AiUpdate(CrewMember crew)
    {
        headLookDirection = defaultPOV.forward;
    }
    public virtual void AiFixed(CrewMember crew)
    {
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BasicSeat))]
public class BasicSeatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        BasicSeat seat = (BasicSeat)target;

        GUI.color = Color.blue;
        EditorGUILayout.HelpBox("POV References", MessageType.None);
        GUI.color = GUI.backgroundColor;

        seat.defaultPOV = EditorGUILayout.ObjectField("Default Head Position", seat.defaultPOV, typeof(Transform), true) as Transform;
        seat.zoomedPOV = EditorGUILayout.ObjectField("Zoomed Head Position", seat.zoomedPOV, typeof(Transform), true) as Transform;

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Limbs References", MessageType.None);
        GUI.color = GUI.backgroundColor;

        seat.rightHandGrip = EditorGUILayout.ObjectField("Right Hand Grip", seat.rightHandGrip, typeof(HandGrip), true) as HandGrip;
        seat.leftHandGrip = EditorGUILayout.ObjectField("Left Hand Grip", seat.leftHandGrip, typeof(HandGrip), true) as HandGrip;
        seat.rightFootRest = EditorGUILayout.ObjectField("Right Foot Rest", seat.rightFootRest, typeof(FootRest), true) as FootRest;
        seat.leftFootRest = EditorGUILayout.ObjectField("Left Foot Rest", seat.leftFootRest, typeof(FootRest), true) as FootRest;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(seat);
            EditorSceneManager.MarkSceneDirty(seat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif