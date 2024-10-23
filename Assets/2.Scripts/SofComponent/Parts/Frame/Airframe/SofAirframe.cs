using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public abstract class SofAirframe : SofFrame, IDamageTick, IMassComponent
{
    const float minimumHp = 10f;
    public override float MaxHp => area * HpPerSquareMeter + minimumHp;
    public abstract float HpPerSquareMeter {get;}

    public float area { get; private set; }

    public AeroSurface foilSurface;

    public override ModuleArmorValues Armor => ModulesHPData.DuraluminArmor;

    public virtual float PropSpeedEffect() { return 0f; }
    public override float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    public virtual float AreaCd() { return 0f; }
    protected abstract Quad CreateQuad();
    protected abstract AeroSurface CreateFoilSurface();



    public virtual void UpdateAerofoil()
    {
        foilSurface = CreateFoilSurface();
        area = foilSurface.Area();
    }
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        UpdateAerofoil();
    }
    protected virtual void FixedUpdate()
    {
        if (!aircraft) foilSurface.ApplyForces();
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SofWindow.showAirframesOverlay)
        {
            Draw();
        }
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofAirframe)), CanEditMultipleObjects]
public class AirframeEditor : FrameEditor
{
    protected override string BasicName()
    {
        return "Airframe";
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofAirframe frame = (SofAirframe)target;
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
    }
}
#endif
