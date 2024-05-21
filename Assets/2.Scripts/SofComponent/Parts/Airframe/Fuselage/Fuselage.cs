using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Fuselage : BoundedAirframe
{
    
    public override float MaxSpd()
    {
        return base.MaxSpd() * 1.5f;
    }
    protected override AirfoilSurface CreateFoilSurface()
    {
        return new DoubleAirfoilSurface(this, foil, CreateQuadBounds(true), CreateQuadBounds(false));
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        foilSurface.quad.Draw(Vector4.zero, new Vector4(1f,0f,0f,0.2f),false);
        ((DoubleAirfoilSurface)foilSurface).secondQuad.Draw(Vector4.zero, new Vector4(0f, 1f, 0f, 0.2f), false);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Fuselage)), CanEditMultipleObjects]
public class FuselageEditor : BoundedAirframeEditor
{
}
#endif