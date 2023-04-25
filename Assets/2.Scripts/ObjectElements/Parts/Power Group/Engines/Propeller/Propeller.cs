using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class Propeller : Module
{
    //Settings
    public float reductionGear = 0.477f;
    public PropellerPreset preset;
    public float efficiency = 0.85f;

    //References
    public PistonEngine engine;
    private EnginePreset enginePreset;
    public MeshRenderer prop;
    public MeshRenderer brokenProp;
    public MeshRenderer blurredProp;
    private MaterialPropertyBlock blurredBlock;

    //Data
    public float rps { get { return engine.rps * reductionGear; } }
    public float MomentOfInertia { get { return emptyMass * Mathv.SmoothStart(preset.diameter, 2) / 20f; } }
    public float TotalSpeed { get { return Mathf.Sqrt(Mathv.SmoothStart(rps * preset.diameter / 13f, 2) + Mathv.SmoothStart(data.ias.Get, 2)); } }
    //Forces
    public float torque = 0f;
    public float thrust = 0f;
    public override void Initialize(ObjectData d, bool firstTime)
    {
        material = preset.material;
        gameObject.layer = 2;
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            engine = GetComponentInParent<PistonEngine>();

            if (!engine || !blurredProp || !brokenProp) return;
            enginePreset = engine.preset;
            prop = GetComponent<MeshRenderer>();
            blurredBlock = new MaterialPropertyBlock();
            blurredProp.gameObject.layer = 1;
            prop.enabled = blurredProp.enabled = !ripped;
            brokenProp.enabled = ripped;
            tr.Rotate(Vector3.forward * Random.value * 360f);
        }
    }
    
    public void Update()
    {
        //Visual Effect
        if (prop && blurredProp)
        {
            if (complex && complex.lod.LOD() <= 1)
            {
                Vector3 cameraDir = tr.position - PlayerCamera.camPos;
                float angle = 1f - Mathf.Abs(Vector3.Angle(cameraDir, data.forward.Get)/90f - 1f);

                blurredProp.GetPropertyBlock(blurredBlock);
                blurredBlock.SetFloat("_Rpm", rps * 30f / Mathf.PI);
                blurredBlock.SetFloat("_CameraAngle", Mathv.SmoothStart(angle,5));
                blurredProp.SetPropertyBlock(blurredBlock);

                blurredProp.enabled = true;
                prop.enabled = engine.rps * (Time.timeScale == 0f ? 1f : Time.timeScale) < enginePreset.idleRPS * 0.8f && !ripped;

                tr.Rotate(-Vector3.forward * Time.deltaTime * rps * 57.3f);
            } else
            {
                prop.enabled = false;
                blurredProp.enabled = false;
            }
        }
    }
    //
    void FixedUpdate()
    {
        if (Time.timeScale != 0f && aircraft)
        {
            Vector2 tweakedForces = new Vector2();
            float rpsLerper = Mathf.InverseLerp(enginePreset.idleRPS, enginePreset.nominalRPS, engine.rps);
            float desiredRps = enginePreset.TargetRPS(engine.throttleInput, aircraft.boost);

            tweakedForces.x = efficiency * engine.trueThrottle * engine.brakePower / TotalSpeed;
            tweakedForces.y = -engine.Power(rpsLerper, aircraft.boost, engine.rps) / desiredRps;

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
            rb.AddForceAtPosition(upForce, upPosition , ForceMode.Force);
            rb.AddForceAtPosition(-upForce, downPosition, ForceMode.Force);
        }
    }

    public override void KineticDamage(float damage, float caliber, float fireCoeff) { return; }

    public override void Rip()
    {
        base.Rip();
        engine.Rip();
        engine.rps /= 10f;
        prop.enabled = blurredProp.enabled = false;
        brokenProp.enabled = true;
    }

    void OnTriggerEnter(Collider other) { Rip(); }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(Propeller))]
public class PropellerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Propeller prop = (Propeller)target;

        //Settings
        GUILayout.Space(15f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Propeller Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        prop.emptyMass = EditorGUILayout.FloatField("Mass", prop.emptyMass);
        prop.reductionGear = EditorGUILayout.FloatField("Reduction Gear", prop.reductionGear);
        prop.preset = EditorGUILayout.ObjectField("Propeller Preset", prop.preset, typeof(PropellerPreset), false) as PropellerPreset;
        prop.efficiency = EditorGUILayout.Slider("Efficiency", prop.efficiency, 0.6f, 1f);

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Visual", MessageType.None);
        GUI.color = GUI.backgroundColor;
        prop.brokenProp = EditorGUILayout.ObjectField("Broken Propeller Renderer", prop.brokenProp, typeof(MeshRenderer), true) as MeshRenderer;
        prop.blurredProp = EditorGUILayout.ObjectField("Smooth Blur Renderer", prop.blurredProp, typeof(MeshRenderer), true) as MeshRenderer;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(prop);
            EditorSceneManager.MarkSceneDirty(prop.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif