using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Airframe/Bounded Airframe")]
public class BoundedAirframe : SofAirframe
{
    public Bounds bounds { get; private set; }

    public override float HpPerSquareMeter => ModulesHPData.frameHpPerSq;

    protected SimpleAirfoil simpleAirfoil;
    public override IAirfoil Airfoil
    {
        get 
        {
            if(simpleAirfoil == null)  simpleAirfoil = new SimpleAirfoil(bounds, 0f);
            return simpleAirfoil;
        }
    }

    protected SurfaceQuad CreateQuadBounds(bool flat)
    {
        Vector3 forward = bounds.size.z * Vector3.forward * 0.5f;
        Vector3 side = (flat ? bounds.size.x * Vector3.right : bounds.size.y * Vector3.up) * 0.5f;

        Vector3 lt = bounds.center + forward + side;
        Vector3 lb = bounds.center + forward - side;
        Vector3 tt = bounds.center - forward + side;
        Vector3 tb = bounds.center - forward - side;

        return new SurfaceQuad(transform, lt, lb, tt, tb);
    }
    public override void UpdateQuad()
    {
        quad = CreateQuadBounds(bounds.size.x > bounds.size.y);
    }

    public override void UpdateAerofoil()
    {
        bounds = GetBounds();
       
        base.UpdateAerofoil();
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        quad.Draw(new Color(), Color.yellow, false);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(BoundedAirframe)), CanEditMultipleObjects]
public class BoundedAirframeEditor : AirframeEditor { }
#endif
