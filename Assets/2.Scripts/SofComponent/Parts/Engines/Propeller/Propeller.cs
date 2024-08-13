using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class Propeller : SofComponent, IMassComponent
{
    public float EmptyMass => preset.mass;
    public float LoadedMass => EmptyMass;
    public override int DefaultLayer()
    {
        return 2;
    }
    //Settings
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

    //Data
    public float rps { get { return engine.rps * reductionGear; } }
    public float MomentOfInertia { get { return LoadedMass * Mathv.SmoothStart(preset.diameter, 2) / 20f; } }
    public float TotalSpeed { get { return Mathf.Sqrt(Mathv.SmoothStart(rps * preset.diameter / 13f, 2) + Mathv.SmoothStart(data.ias.Get, 2)); } }
    //Forces
    public float torque = 0f;
    public float thrust = 0f;
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
    }
    const float INVERT90 = 1f / 90F;
    //Visual Effect
    public void Update()
    {
        if (!meshRenderer || !blurredProp || !complex) return;

        Vector3 cameraDir = tr.position - SofCamera.tr.position;
        float angle = 1f - Mathf.Abs(Vector3.Angle(cameraDir, data.forward.Get) * INVERT90 - 1f);

        blurredProp.GetPropertyBlock(blurredBlock);
        blurredBlock.SetFloat("_Rpm", rps * 30f * Mathv.invPI);
        blurredBlock.SetFloat("_CameraAngle", Mathv.SmoothStart(angle, 5));
        blurredProp.SetPropertyBlock(blurredBlock);

        blurredProp.enabled = true;
        meshRenderer.enabled = engine.rps * (Time.timeScale == 0f ? 1f : Time.timeScale) < engine.preset.idleRPS * 0.8f && !ripped;

        tr.Rotate(-Vector3.forward * Time.deltaTime * rps * 57.3f);
    }
    //
    void FixedUpdate()
    {
        if (Time.timeScale != 0f && aircraft)
        {
            Vector2 tweakedForces = new Vector2();
            float rpsLerper = Mathf.InverseLerp(engine.preset.idleRPS, engine.preset.nominalRPS, engine.rps);
            float desiredRps = engine.preset.TargetRPS(engine.throttleInput, aircraft.engines.boost);

            tweakedForces.x = efficiency * engine.trueThrottle * engine.brakePower / TotalSpeed;
            tweakedForces.y = -engine.Power(rpsLerper, aircraft.engines.boost, engine.rps) / desiredRps;

            Vector2 simulatedForces = preset.GetForces(data.ias.Get, rps, preset.phiOff, data.density.Get);

            thrust = Mathf.Lerp(simulatedForces.x, tweakedForces.x, engine.trueThrottle);
            torque = engine.Working() ? tweakedForces.y : simulatedForces.y * reductionGear;
        }

        if (aircraft && TotalSpeed > 0f && !float.IsNaN(thrust)) rb.AddForceAtPosition(transform.forward * thrust, transform.position, ForceMode.Force);
        if (false && aircraft && TotalSpeed > 0f && !float.IsNaN(torque))
        {
            Vector3 upForce = transform.root.up * torque / preset.diameter; //The diameter and torque are divided by 2, cancelling each other
            Vector3 upPosition = transform.position - transform.root.right * preset.diameter / 2f;
            Vector3 downPosition = transform.position + transform.root.right * preset.diameter / 2f;
            rb.AddForceAtPosition(upForce, upPosition, ForceMode.Force);
            rb.AddForceAtPosition(-upForce, downPosition, ForceMode.Force);
        }

        if (tr.position.y < preset.diameter) engine.Rip();
    }

    public void Rip()
    {
        engine.Rip();
        engine.rps /= 10f;
        meshRenderer.enabled = blurredProp.enabled = false;
        brokenProp.enabled = true;
    }

    void OnTriggerEnter(Collider other) { Rip(); }
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