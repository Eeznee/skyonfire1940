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

    private Vector3 defaultLocalDirection;

    public Vector3 DefaultDirection => transform.parent.TransformDirection(defaultLocalDirection);

    public virtual bool AlignedWithDefaultDirection => (FiringDirection - DefaultDirection).sqrMagnitude < 0.000001f;
    public virtual Vector3 FiringDirection => transform.rotation * Vector3.forward;
    public virtual Vector3 CameraUp => sofObject.tr.up;
    protected float HydraulicsFactor => Application.isPlaying && linkedHydraulics ? 1f - linkedHydraulics.state : hydraulicsEditorTest;
    public bool PilotMainGun => guns[0].controller != GunController.Gunner;
    public AmmunitionPreset Ammunition => guns[0].gunPreset.ammunition;
    public Vector3 AimPosition => aimingPOV ? aimingPOV.position : transform.position;

    public virtual float TargetAvailability(Vector3 pos)
    {
        return 1f;
    }

    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);
        guns = GetComponentsInChildren<Gun>();
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        defaultLocalDirection = transform.parent.InverseTransformDirection(FiringDirection);
    }
    public virtual void OperateMainManual(Vector2 input) { }
    public virtual void OperateMainTracking(Vector3 direction) { }
    public virtual void OperateSpecialManual(float input) { }
    public virtual void OperateSpecialTracking(Vector3 direction) { }
    public virtual void OperateSpecialResetDefault() { }
    public void OperateResetToDefault()
    {
        OperateMainTracking(DefaultDirection);
        OperateSpecialResetDefault();
    }

    public void OperateTrigger(bool fire, bool preventDamage)
    {
        if (!fire) return;

        if (preventSelfDamage || preventDamage)
        {
            foreach (Gun g in guns)
            {
                if (Physics.Raycast(g.tr.position, g.tr.forward, 12f, LayerMask.GetMask("SofComplex"))) return;
            }
        }

        foreach (Gun g in guns) g.Trigger();
    }
    public void SetFuze(float dis) { foreach (Gun g in guns) g.SetFuze(dis); }
    public bool Firing
    {
        get
        {
            foreach (Gun g in guns)
                if (g.Firing()) return true;
            return false;
        }
    }

    public float hydraulicsEditorTest = 1f;
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