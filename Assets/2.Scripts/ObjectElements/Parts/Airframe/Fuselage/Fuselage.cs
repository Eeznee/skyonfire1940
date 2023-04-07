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
    protected override Aero CreateAero()
    {
        return new DoubleAero(this, foil, CreateQuadBounds(true), CreateQuadBounds(false));
    }
#if UNITY_EDITOR
    protected override void Draw()
    {
        RecalculateBounds();
        aero.quad.Draw(new Color(0f, 0f, 0f, 0f), Color.red,false);
        ((DoubleAero)aero).secondQuad.Draw(new Color(0f, 0f, 0f, 0f), Color.green,false);  
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(Fuselage))]
public class FuselageEditor : BoundedAirframeEditor
{
}
#endif
