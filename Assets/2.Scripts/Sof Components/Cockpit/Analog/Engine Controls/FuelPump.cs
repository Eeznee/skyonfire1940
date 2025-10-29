using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class FuelPump : AnalogInteractable
{
    public PistonEngine engine;

    public int pumpActions = 3;
    private float pumped;
    private float previousInput;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        pumped = 0f;
        previousInput = input = 1f;
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        if (engine.pumped) return;
        float delta = Mathf.Max(0f, previousInput - input);
        pumped += delta;
        if (pumped * 1.2f > pumpActions)
            engine.pumped = true;
        previousInput = input;
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(input);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(FuelPump))]
public class FuelPumpEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        FuelPump pump = (FuelPump)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Starter Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        pump.engine = EditorGUILayout.ObjectField("Piston Engine", pump.engine, typeof(PistonEngine), true) as PistonEngine;
        pump.pumpActions = EditorGUILayout.IntField("Pumping Actions", pump.pumpActions);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif