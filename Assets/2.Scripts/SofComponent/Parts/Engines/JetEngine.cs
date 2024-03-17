using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class JetEngine : Engine
{
    public Transform inlet;
    public float thrust;
    const float torqueCoeff = 0.2f;
    const float minTorque = 60f;
    const float friction = 10f;

    public override float ConsumptionRate() { return preset.ConsumptionRate(trueThrottle,thrust); }
    private void FixedUpdate()
    {
        EngineFixedUpdate();
        if (!igniting)
        {
            float targetRps = Mathf.Lerp(preset.idleRPS, preset.fullRps, throttleInput);
            float torque = Working() ? Mathf.Max(Mathf.Abs(targetRps - rps) * torqueCoeff, minTorque) : friction;
            rps = Mathf.MoveTowards(rps, Working() ? targetRps : 0f, torque * Time.fixedDeltaTime);

            thrust = data.relativeDensity.Get * trueThrottle * preset.maxThrust;
            rb.AddForceAtPosition(thrust * transform.forward, transform.position);
        }
    }
    private void Update()
    {
        inlet.Rotate(Vector3.forward * rps * Mathf.Rad2Deg * Time.deltaTime);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(JetEngine))]
public class JetEngineEditor : EngineEditor
{
    SerializedProperty inlet;

    protected override void OnEnable()
    {
        base.OnEnable();
        inlet = serializedObject.FindProperty("inlet");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(inlet);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif