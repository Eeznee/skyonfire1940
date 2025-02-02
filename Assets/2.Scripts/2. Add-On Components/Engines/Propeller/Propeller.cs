using UnityEngine;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//

[AddComponentMenu("Sof Components/Power Group/Propeller")]
public class Propeller : SofComponent, IMassComponent, IAircraftForce
{
    public float EmptyMass => preset ? preset.mass : 0f;
    public float LoadedMass => EmptyMass;
    public float RealMass => EmptyMass;
    public override int DefaultLayer()
    {
        return 2;
    }
    //Settings
    public float radius = 1.5f;
    public float reductionGear = 0.477f;
    public PropellerPreset preset;
    public float efficiency = 0.85f;

    //References
    public PistonEngine engine;
    public MeshRenderer meshRenderer;
    public MeshRenderer brokenProp;
    public MeshRenderer blurredProp;
    private MaterialPropertyBlock blurredBlock;

    public bool ripped { get; private set; }

    public float RadPerSec { get { return engine.radiansPerSeconds * reductionGear; } }
    public float MomentOfInertia { get { return RealMass * Mathv.SmoothStart(preset.diameter, 2) / 20f; } }
    public float OptimalRps { get { return engine.Preset.fullRps * reductionGear; } }
    public float Torque { get; private set; }
    public float Thrust { get; private set; }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        gameObject.layer = 2;
        engine = GetComponentInParent<PistonEngine>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (!engine || !blurredProp || !brokenProp) return;
        blurredBlock = new MaterialPropertyBlock();
        blurredProp.gameObject.layer = 1;
        meshRenderer.enabled = blurredProp.enabled = !ripped;
        brokenProp.enabled = ripped;
        tr.Rotate(Vector3.forward * Random.value * 360f);

        param = FindEfficiencyParameters(targetAdvanceRatio);
    }

    const float targetAdvanceRatio = 0.5f;
    private Vector2 param;


    const float INVERT90 = 1f / 90F;
    //Visual Effect
    public void Update()
    {
        if (!meshRenderer || !blurredProp || !aircraft) return;

        Vector3 cameraDir = tr.position - SofCamera.tr.position;
        float angle = 1f - Mathf.Abs(Vector3.Angle(cameraDir, aircraft.tr.forward) * INVERT90 - 1f);

        blurredProp.GetPropertyBlock(blurredBlock);
        blurredBlock.SetFloat("_Rpm", RadPerSec * 30f * Mathv.invPI);
        blurredBlock.SetFloat("_CameraAngle", Mathv.SmoothStart(angle, 5));
        blurredProp.SetPropertyBlock(blurredBlock);

        blurredProp.enabled = true;
        meshRenderer.enabled = engine.radiansPerSeconds * (Time.timeScale == 0f ? 1f : Time.timeScale) < engine.Preset.idleRPS * 0.8f && !ripped;

        tr.Rotate(-Vector3.forward * Time.deltaTime * RadPerSec * 57.3f);
    }
    public ForceAtPoint SimulatePointForce(FlightConditions flightConditions)
    {
        if (float.IsNaN(Thrust)) return new ForceAtPoint(Vector3.zero, flightConditions.position);

        Vector3 direction = flightConditions.TransformWorldDir(tr.root.forward);
        Vector3 point = flightConditions.TransformWorldPos(tr.position);

        return new ForceAtPoint(direction * Thrust, point);
    }

    const float desiredRpsToMaxEfficientRatio = 0.3f;

    void FixedUpdate()
    {
        if (Time.timeScale == 0f || !aircraft) return;

        float desiredRps = desiredRpsToMaxEfficientRatio * data.tas.Get / (targetAdvanceRatio * radius);
        float desiredRpsOptimal = OptimalRps * desiredRpsToMaxEfficientRatio;
        float rpsFactor = M.SquareSigned((RadPerSec - desiredRps) / (OptimalRps - desiredRpsOptimal));

        float maxEnginePower = engine.Power(1f, engine.Preset.fullRps);
        float torqueAtOptimalConditions = engine.Torque(maxEnginePower, engine.Preset.fullRps);

        Thrust = engine.BrakePower * PowerToThrust(RadPerSec, data.tas.Get, param) * efficiency;
        Torque = -rpsFactor * torqueAtOptimalConditions;

        if (tr.position.y < preset.diameter) engine.Rip();
    }

    public void Rip()
    {
        engine.Rip();
        meshRenderer.enabled = blurredProp.enabled = false;
        brokenProp.enabled = true;

        ripped = true;

        if (complex.lod) complex.lod.UpdateMergedModel();
    }

    void OnTriggerEnter(Collider other) { Rip(); }


    const float lowestEfficientAdvanceRatio = 0.1f;
    const float minimumPitchAngle = 20f;

    const float cubicRoot = 1f / 3f;

    private float AdvanceRatio(float bladeAngleDeg)
    {
        return Mathf.Tan(bladeAngleDeg * Mathf.Deg2Rad);
    }
    private float BladeAngle(float advanceRatio)
    {
        return Mathf.Atan(advanceRatio) * Mathf.Rad2Deg;
    }
    private float AdvanceRatio(float radPerSec, float tas)
    {
        if (radPerSec == 0f) return 0f;

        return tas / (radPerSec * radius);
    }
    private float PowerToThrust(float radPerSec, float tas, Vector2 efficiencyParameters)
    {
        if (radPerSec == 0f) return 0f;
        float advanceRatio = AdvanceRatio(radPerSec, tas);
        float AR = advanceRatio / lowestEfficientAdvanceRatio;

        float value = (efficiencyParameters.y - AR) / (AR * AR + efficiencyParameters.x);
        value = Mathf.Pow(Mathf.Abs(value), cubicRoot) * Mathf.Sign(value);

        return value / (radPerSec * radius) / lowestEfficientAdvanceRatio;
    }
    private Vector2 FindEfficiencyParameters(float optimalAdvanceRatio)
    {
        float adjustedRatio = optimalAdvanceRatio / lowestEfficientAdvanceRatio;

        float x = (M.Pow(adjustedRatio, 4) - M.Pow(adjustedRatio, 2)) / 3f;
        x = Mathf.Max(x, 0f);
        float y = 2f / (3f * adjustedRatio) + 4f * adjustedRatio / 3f;

        return new Vector2(x, y);
    }
    private float ZeroAdvanceRatioEfficiency(Vector2 efficiencyParameters)
    {
        return Mathf.Pow(efficiencyParameters.y / efficiencyParameters.x, cubicRoot);
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(Propeller))]
public class PropellerEditor : SofComponentEditor
{
    SerializedProperty gear;
    SerializedProperty preset;
    SerializedProperty efficiency;

    SerializedProperty broken;
    SerializedProperty blurred;
    protected override void OnEnable()
    {
        base.OnEnable();
        gear = serializedObject.FindProperty("reductionGear");
        preset = serializedObject.FindProperty("preset");
        efficiency = serializedObject.FindProperty("efficiency");

        broken = serializedObject.FindProperty("brokenProp");
        blurred = serializedObject.FindProperty("blurredProp");
    }
    static bool showMain = true;
    static bool showVisuals = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(preset);
            EditorGUILayout.PropertyField(gear);
            EditorGUILayout.Slider(efficiency, 0.6f, 1f);

            EditorGUI.indentLevel--;
        }
        showVisuals = EditorGUILayout.Foldout(showVisuals, "Visuals", true, EditorStyles.foldoutHeader);
        if (showVisuals)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(broken);
            EditorGUILayout.PropertyField(blurred);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif