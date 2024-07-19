using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



abstract public class GunMount : SofComponent
{
    public Transform aimingPOV;
    public HydraulicSystem linkedHydraulics;
    public bool preventSelfDamage;

    public virtual bool ForceJoystickControls => false;

    protected Gun[] guns;

    public Vector3 AimPosition => aimingPOV.position;

    public virtual float TargetAvailability(Vector3 pos)
    {
        return 1f;
    }

    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        guns = GetComponentsInChildren<Gun>();
    }
    public virtual void OperateMainManual(Vector2 input) { }
    public virtual void OperateMainTracking(Vector3 direction) { }
    public virtual void OperateSpecialManual(float input) { }
    public virtual void OperateSpecialTracking(Vector3 direction) { }
    public void OperateTrigger(bool fire, bool preventDamage)
    {
        if (!fire) return;

        if (preventSelfDamage || preventDamage)
        {
            foreach (Gun g in guns)
            {
                Vector3 realPhysicsPos = rb.position + g.tr.position - tr.root.position;
                if (Physics.Raycast(realPhysicsPos, g.tr.forward, 12f, LayerMask.GetMask("SofComplex"))) return;
            }
        }

        foreach (Gun g in guns) g.Trigger();
    }
    public void SetFuze(float dis) {  foreach (Gun g in guns) g.fuzeDistance = dis; }
    public bool Firing
    {
        get
        {
            foreach (Gun g in guns)
                if (g.Firing()) return true;
            return false;
        }
    }
    public float hydraulicsEditorTest = 0f;

    public virtual Vector3 FiringDirection => transform.rotation * Vector3.forward;
    public virtual Vector3 CameraUp => sofObject.tr.up;
    protected float HydraulicsFactor => Application.isPlaying && linkedHydraulics ? 1f - linkedHydraulics.state : hydraulicsEditorTest;
    public bool PilotMainGun => guns[0].controller != GunController.Gunner;
    public float MuzzleVelocity => guns[0].gunPreset.ammunition.defaultMuzzleVel;
}
#if UNITY_EDITOR
[CustomEditor(typeof(GunMount))]
public class GunMountEditor : Editor
{
    SerializedProperty aimingPOV;
    SerializedProperty preventSelfDamage;
    SerializedProperty linkedHydraulics;

    static bool showMain = true;
    static bool turretMovement = true;
    static bool turretMovementHydraulics = true;

    protected virtual void OnEnable()
    {
        aimingPOV = serializedObject.FindProperty("aimingPOV");
        preventSelfDamage = serializedObject.FindProperty("preventSelfDamage");
        linkedHydraulics = serializedObject.FindProperty("linkedHydraulics");
    }

    protected virtual void MainSettings()
    {
        EditorGUILayout.PropertyField(aimingPOV);
        EditorGUILayout.PropertyField(preventSelfDamage);
    }

    protected virtual void Movement()
    {

    }
    protected virtual void MovementHydraulics()
    {
        EditorGUILayout.PropertyField(linkedHydraulics);
        GunMount gunMount = (GunMount)target;
        if (gunMount.linkedHydraulics)
            gunMount.hydraulicsEditorTest = EditorGUILayout.Slider("Editor Hydraulics Simulation", gunMount.hydraulicsEditorTest, 0f, 1f);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;
            MainSettings();
            EditorGUI.indentLevel--;
        }
        turretMovement = EditorGUILayout.Foldout(turretMovement, "Constrains", true, EditorStyles.foldoutHeader);
        if (turretMovement)
        {
            EditorGUI.indentLevel++;
            Movement();
            EditorGUI.indentLevel--;
        }
        turretMovementHydraulics = EditorGUILayout.Foldout(turretMovementHydraulics, "Constrains Hydraulics", true, EditorStyles.foldoutHeader);
        if (turretMovementHydraulics)
        {
            EditorGUI.indentLevel++;
            MovementHydraulics();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif