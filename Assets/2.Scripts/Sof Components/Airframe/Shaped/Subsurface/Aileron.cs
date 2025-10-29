using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Aileron")]
public class Aileron : ControlSurface, IRollControlled
{
    public override float ExtractControl(AircraftAxes axes)
    {
        return axes.roll * Left;
    }
    public override float ControlsInversion => Left;

#if UNITY_EDITOR
    protected override Color FillColor()
    {
        return new Color(0f, 0f, 1f, 0.2f);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Aileron)), CanEditMultipleObjects]
public class AileronEditor : ControlSurfaceEditor
{
}
#endif
