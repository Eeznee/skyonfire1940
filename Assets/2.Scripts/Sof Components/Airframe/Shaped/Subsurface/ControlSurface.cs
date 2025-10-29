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

    public float CurrentControlAngle { get; }
    public float ControlAngle(float controlState);

    public float ControlsResistance(AircraftAxes axes);
    public float ControlMoment(FlightConditions flightConditions);
    public float ControlMoment();



}
public static class IControlSurfaceExtension
{
    public static float CombinedForces(this IControlSurface[] surfaces)
    {
        float force = 0f;
        foreach (IControlSurface surface in surfaces) if (surface.ThisSurface != null && !surface.ThisSurface.ripped) force += surface.ControlMoment();
        return force;
    }
    public static float CombinedForces(this IControlSurface[] surfaces, FlightConditions flightConditions)
    {
        float force = 0f;
        foreach (IControlSurface surface in surfaces) if (surface.ThisSurface != null && !surface.ThisSurface.ripped) force += surface.ControlMoment(flightConditions);
        return force;
    }
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
    public virtual float ControlsInversion => 1f;

    public float ControlMoment(FlightConditions flightConditions)
    {
        float aoa = AngleOfAttack(flightConditions);
        float coeff = Coefficient(aoa, ControlAngle(flightConditions.axes));
        
        float velSquared = flightConditions.PointVelocity(quad.centerAero.WorldPos).sqrMagnitude;
        return coeff * 0.5f * velSquared * flightConditions.airDensity * area * ControlsInversion;
    }
    public float ControlMoment()
    {
        float coeff = Coefficient(parentSurface.angleOfAttack, CurrentControlAngle);
        return coeff * 0.5f * parentSurface.airSpeed * parentSurface.airSpeed * data.density.Get * area * ControlsInversion;
    }

    const float aoaFactor = 0.0012f;
    const float deflectionFactor = 0.002f;
    public float Coefficient(float aoa, float deflection)
    {
        return deflectionFactor * -deflection + aoaFactor * aoa;
    }

    public float ControlAngle(AircraftAxes axes)
    {
        float controlState = ExtractControl(axes);
        return controlState * (controlState >= 0f ? maxDeflection : minDeflection);
    }
    public float ControlAngle(float controlState)
    {
        return controlState * (controlState >= 0f ? maxDeflection : minDeflection);
    }
    public float DeflectionLimit(float controlStateSign)
    {
        return (controlStateSign >= 0f) ? maxDeflection : minDeflection;
    }
    public override void SetReferences(SofModular _complex)
    {
        if(aircraft) aircraft.OnUpdateLOD0 -= UpdateControlSurfaceAngle;
        base.SetReferences(_complex);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        if (SymmetricalDeflections) minDeflection = maxDeflection;
        minDeflection = Mathf.Abs(minDeflection);
        localRotateAxis = quad.controlSurfaceAxis.LocalDir * Left;
        aircraft.OnUpdateLOD0 += UpdateControlSurfaceAngle;
    }
    private void UpdateControlSurfaceAngle()
    {
        transform.localRotation = Quaternion.AngleAxis(CurrentControlAngle, localRotateAxis);
    }
    public Vector3 Gradient(FlightConditions conditions, bool positiveControlState, bool brokenForTracking)
    {
        float maxAngle = positiveControlState ? MaxDeflection : MinDeflection;
        float clGradient = Parent.controlSurfaceCoefficient * maxAngle * (brokenForTracking ? Parent.Airfoil.GradientRadians() : Parent.Airfoil.GradientDegrees());

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

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
