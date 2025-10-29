using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Elevator")]
public class Elevator : ControlSurface, IPitchControlled
{
    public override float ExtractControl(AircraftAxes axes)
    {
        return axes.pitch;
    }


#if UNITY_EDITOR
    protected override Color FillColor()
    {
        return new Color(1f, 0f, 0f, 0.2f);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Elevator)), CanEditMultipleObjects]
public class ElevatoreEditor : ControlSurfaceEditor
{
}
#endif
