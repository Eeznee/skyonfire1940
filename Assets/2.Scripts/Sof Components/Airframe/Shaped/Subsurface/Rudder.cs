using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Rudder")]
public class Rudder : ControlSurface, IYawControlled
{
    public override bool SymmetricalDeflections => true;
    public override float ExtractControl(AircraftAxes axes)
    {
        return axes.yaw;
    }

#if UNITY_EDITOR
    protected override Color FillColor()
    {
        return new Color(0f, 1f, 0f, 0.2f);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Rudder)), CanEditMultipleObjects]
public class RudderEditor : ControlSurfaceEditor
{
}
#endif
