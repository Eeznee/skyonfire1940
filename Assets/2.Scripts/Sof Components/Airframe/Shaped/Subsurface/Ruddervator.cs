using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Ruddervator")]
public class Ruddervator : Elevator, IPitchControlled, IYawControlled
{
    public override bool SymmetricalDeflections => true;
    public override float ExtractControl(AircraftAxes axes)
    {
        return Mathf.Clamp((axes.pitch + axes.yaw * Left) * 0.66f, -1f, 1f);
    }

    public override float ControlsInversion => Left;
}
#if UNITY_EDITOR
[CustomEditor(typeof(Ruddervator)), CanEditMultipleObjects]
public class RuddervatorEditor : ControlSurfaceEditor
{
}
#endif
