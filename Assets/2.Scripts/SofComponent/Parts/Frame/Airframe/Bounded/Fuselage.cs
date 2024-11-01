using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[AddComponentMenu("Sof Components/Airframe/Fuselage")]
public class Fuselage : BoundedAirframe
{
    public override float MaxSpd => base.MaxSpd * 1.5f;

    public SurfaceQuad secondQuad;

    public override void UpdateQuad()
    {
        quad = CreateQuadBounds(true);
        secondQuad = CreateQuadBounds(false);
    }
    public override void UpdateArea()
    {
        area = quad.Area + secondQuad.Area;
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        quad.Draw(Vector4.zero, new Vector4(1f,0f,0f,0.2f),false);
        secondQuad.Draw(Vector4.zero, new Vector4(0f, 1f, 0f, 0.2f), false);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Fuselage)), CanEditMultipleObjects]
public class FuselageEditor : BoundedAirframeEditor { }
#endif
