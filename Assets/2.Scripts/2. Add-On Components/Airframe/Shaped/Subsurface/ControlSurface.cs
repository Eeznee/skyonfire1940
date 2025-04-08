using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public interface IPitchControlled : IControlSurface { }
public interface IRollControlled : IControlSurface { }
public interface IYawControlled : IControlSurface { }


public interface IControlSurface { 
    public SofAircraft aircraft { get; } 
    public ControlSurface ThisSurface { get; }
    public float ControlsResistance(AircraftAxes axes);
    public float CurrentControlAngle { get; }
    public float ControlAngle(float controlState);
}

public abstract class ControlSurface : Subsurface
{
    [SerializeField] private float maxDeflection = 20f;
    [SerializeField] private float minDeflection = 20f;

    private Vector3 localRotateAxis;

    public float MaxDeflection => maxDeflection;
    public float MinDeflection => minDeflection;
    public ControlSurface ThisSurface => this;
    public virtual bool SymmetricalDeflections => false;

    public float CurrentControlAngle => ControlAngle(CurrentControl);
    protected float CurrentControl => aircraft ?  ExtractControl(aircraft.controls.current) : 0f;
    public abstract float ExtractControl(AircraftAxes axes);
    public virtual float ControlsResistance(AircraftAxes axes)
    {
        return area * DeflectionLimit(ExtractControl(axes)) * Mathf.Deg2Rad;
    }
    public float ControlAngle(AircraftAxes axes)
    {
        return ControlAngle(ExtractControl(axes));
    }
    public float ControlAngle(float controlState)
    {
        return controlState * DeflectionLimit(controlState);
    }
    public virtual float DeflectionLimit(float controlStateSign)
    {
        return (controlStateSign >= 0f) ? MaxDeflection : MinDeflection;
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        if (SymmetricalDeflections) minDeflection = maxDeflection;
        minDeflection = Mathf.Abs(minDeflection);
        localRotateAxis = quad.controlSurfaceAxis.LocalDir * Left;
    }
    private void Update()
    {
        if (!aircraft) return;

        transform.localRotation = Quaternion.AngleAxis(CurrentControlAngle, localRotateAxis);
    }

    public Vector3 Gradient(FlightConditions conditions, bool positiveControlState)
    {
        float maxAngle = positiveControlState ? MaxDeflection : MinDeflection;
        float aoaGradient = Parent.controlSurfaces.TotalOverlap * Parent.controlSqrt * maxAngle;
        float clGradient = aoaGradient * Parent.Airfoil.Gradient();

        Vector3 vel = conditions.PointVelocity(Parent.quad.centerAero.Pos(conditions));
        Vector3 aeroDir = Parent.quad.aeroDir.Dir(conditions);

        Vector3 liftPerDegree = Aerodynamics.Lift(vel, aeroDir, conditions.airDensity, Parent.area, clGradient, Parent.AerodynamicIntegrity);

        return liftPerDegree;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ControlSurface)), CanEditMultipleObjects]
public class ControlSurfaceEditor : ShapedAirframeEditor
{
    SerializedProperty minDeflection;
    SerializedProperty maxDeflection;
    protected override void OnEnable()
    {
        base.OnEnable();
        minDeflection = serializedObject.FindProperty("minDeflection");
        maxDeflection = serializedObject.FindProperty("maxDeflection");
    }
    static bool showFoil = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        ControlSurface surface = (ControlSurface)target;

        showFoil = EditorGUILayout.Foldout(showFoil, "Control Surface", true, EditorStyles.foldoutHeader);
        if (showFoil)
        {
            EditorGUI.indentLevel++;

            if (!surface.SymmetricalDeflections)
            {
                EditorGUILayout.PropertyField(minDeflection, new GUIContent("Negative Max Angle"));
            }
            EditorGUILayout.PropertyField(maxDeflection, new GUIContent("Positive Max Angle"));


            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
