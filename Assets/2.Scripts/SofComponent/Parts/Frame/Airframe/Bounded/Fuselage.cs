using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[AddComponentMenu("Sof Components/Airframe/Fuselage")]
public class Fuselage : BoundedAirframe
{
    
    public override float MaxSpd()
    {
        return base.MaxSpd() * 1.5f;
    }
    protected override AeroSurface CreateFoilSurface()
    {
        return new DoubleAeroSurface(this, new SimpleAirfoil(bounds,0f), CreateQuadBounds(true), CreateQuadBounds(false));
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        foilSurface.quad.Draw(Vector4.zero, new Vector4(1f,0f,0f,0.2f),false);
        ((DoubleAeroSurface)foilSurface).secondQuad.Draw(Vector4.zero, new Vector4(0f, 1f, 0f, 0.2f), false);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Fuselage)), CanEditMultipleObjects]
public class FuselageEditor : BoundedAirframeEditor { }
#endif
