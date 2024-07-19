using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Propeller", menuName = "SOF/Aircraft Modules/Propeller")]
public class PropellerPreset : ScriptableObject
{
    [HideInInspector] public float propPitch = 0f;
    [HideInInspector] public float rpsSet = 0f;
    [HideInInspector] public float speedSet = 0f;
    public AnimationCurve curve1;
    public AnimationCurve curve2;

    //Shape
    public float mass = 150f;
    public float diameter = 2.6f;
    public int bladesAmount = 3;
    public Vector2[] propShape;
    public OldAirfoil bladeShape;
    public ModuleMaterial material;
    //Variable Pitch
    public float phiOff = 10f;
    public float pitchSpeed = 10f;

    //Return thrust and torque

    public Vector2 GetForces(float forwardSpeed, float rps, float phi)
    {
        float density = Aerodynamics.GetAirDensity(20f, Aerodynamics.SeaLvlPressure);
        return GetForces(forwardSpeed, rps, phi, density);
    }
    public Vector2 GetForces(float forwardSpeed, float rps, float phi, float density)
    {
        Vector2 totalForces = Vector2.zero;
        for (int i = 0; i < propShape.Length; i++)
        {
            //First Calculations
            float area = bladesAmount * propShape[i].y * diameter / 2f / propShape.Length;
            float distance = (diameter / 2f) * ((float)i / propShape.Length);
            Vector2 vel = new Vector2(forwardSpeed, distance * rps);
            float speed = vel.magnitude;
            //Angles
            float phiAngle = phi + propShape[i].x;
            float airFlowAngle = (vel.y != 0f) ? Mathf.Atan(vel.x / vel.y) * Mathf.Rad2Deg : 90f;
            float alpha = airFlowAngle - phiAngle;
            //Forces
            Vector2 airfoilDrag = -vel * speed / 2f * area * bladeShape.Cd(alpha) * density;
            Vector2 airfoilLift = new Vector2(-vel.y, vel.x) * speed / 2f * area * bladeShape.Cl(alpha) * density;
            Vector2 forces = airfoilDrag + airfoilLift;
            totalForces.x += forces.x;
            totalForces.y += forces.y * distance;
        }
        return totalForces;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PropellerPreset))]
public class PropellerPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        PropellerPreset propeller = (PropellerPreset)target;

        propeller.propPitch = EditorGUILayout.Slider("Propeller Pitch Angle", propeller.propPitch, 0f, 80f);
        propeller.rpsSet = EditorGUILayout.Slider("Propeller RPM", propeller.rpsSet * 10f, 0f, 3000f) / 10f;
        propeller.speedSet = EditorGUILayout.Slider("Aircraft Airspeed", propeller.speedSet, 0f, 1000f);

        if (GUILayout.Button("Compute Thrust At Varied Speed"))
        {
            propeller.curve1 = AnimationCurve.Linear(0f, propeller.GetForces(0f, propeller.rpsSet, propeller.propPitch).x, 1000, propeller.GetForces(1000f / 3.6f, propeller.rpsSet, propeller.propPitch).x);
            propeller.curve2 = AnimationCurve.Linear(0f, propeller.GetForces(0f, propeller.rpsSet, propeller.propPitch).y, 1000, propeller.GetForces(1000f / 3.6f, propeller.rpsSet, propeller.propPitch).y);
            for (int i = 0; i <= 1000; i += 10)
            {
                propeller.curve1.AddKey(i, propeller.GetForces(i / 3.6f, propeller.rpsSet, propeller.propPitch).x);
                propeller.curve2.AddKey(i, propeller.GetForces(i / 3.6f, propeller.rpsSet, propeller.propPitch).y);
            }
        }
        if (GUILayout.Button("Compute Thrust At Varied AR"))
        {
            Vector2 maxForces = propeller.GetForces(propeller.speedSet / 3.6f, propeller.speedSet / 3.6f * propeller.diameter / 10f, propeller.propPitch);
            propeller.curve1 = AnimationCurve.Linear(0f, 0f, 4f, maxForces.x);
            propeller.curve2 = AnimationCurve.Linear(0f, 0f, 4f, maxForces.y);
            for (int i = 0; i < 1000; i++)
            {
                float rps = propeller.speedSet / 3.6f * propeller.diameter / (i / 250f);
                propeller.curve1.AddKey(i / 250f, propeller.GetForces(propeller.speedSet / 3.6f, rps, propeller.propPitch).x);
                propeller.curve2.AddKey(i / 250f, propeller.GetForces(propeller.speedSet / 3.6f, rps, propeller.propPitch).y);
            }
        }

        base.OnInspectorGUI();
        /*
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Blade Properties", MessageType.None);
        GUI.color = GUI.backgroundColor;
        EditorGUILayout.HelpBox("At angle 0 , drag = thrust = efficienct = 1", MessageType.Info);


        blade.dragCurve = EditorGUILayout.CurveField("Drag Curve", blade.dragCurve);
        blade.efficiencyCurve = EditorGUILayout.CurveField("Efficiency Curve", blade.efficiencyCurve);
        if (blade.efficiencyCurve.Evaluate(0f) == 1f && blade.DragCoeff(0f) == 1f)
        {
            EditorGUILayout.HelpBox("These curves are approved", MessageType.Info);

        } else
        {
            EditorGUILayout.HelpBox("At angle 0 , efficienct MUST be equal to 1 !!!", MessageType.Warning);
        }
        */


        if (GUI.changed)
        {
            EditorUtility.SetDirty(propeller);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif