using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Wing")]
public class Wing : MainSurface
{
    public Airfoil airfoil;

    public override float HpPerSquareMeter => ModulesHPData.sparHpPerSq;
    public override ModuleArmorValues Armor => ModulesHPData.SparArmor;

    public float oswald = 0.75f;
    public WingSkin skin;
    public Mesh skinMesh;

    public Wing parent { get; private set; }
    public Wing child { get; private set; }
    public Wing root { get; private set; }

    public override Transform SubSurfaceParent => root.tr;
    public override float AirframeDamage => base.AirframeDamage * (skin ? skin.structureDamage : 1f);
    public override IAirfoil Airfoil => airfoil;
    public float EntireWingArea
    {
        get
        {
            if (root != this)
                return root.EntireWingArea;

            float totalArea;
            Wing currentWing = this;
            for (totalArea = area; currentWing.child != null; totalArea += currentWing.area)
                currentWing = currentWing.child;
            return totalArea;
        }
    }
    public override float MaxSpd
    {
        get
        {
            float coeff = 1f;
            if (!parent) coeff += 0.1f;
            if (child) coeff += 0.1f;
            return aircraft.SpeedLimitMps * coeff;
        }
    }
    public override float MaxG
    {
        get
        {
            float coeff = 1f;
            if (parent) coeff += 0.15f;
            if (!child) coeff += 0.15f;
            return aircraft.MaxGForce * coeff;
        }
    }
    public override float AerodynamicIntegrity => skin.structureDamage;

    public override void SetReferences(SofComplex _complex)
    {
        child = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Wing>() : null;
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
        base.SetReferences(_complex);

        airfoil.UpdateValues();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);
        if(Airfoil == null) Debug.LogError(aircraft.name + "  " + name + " does not have an airfoil assigned");
    }

    public void CopyRootValues(Wing rootWing)
    {
        root = rootWing;
        airfoil = root.airfoil;
        oswald = root.oswald;
    }
    public override void UpdateAerofoil()
    {
        if (!parent)
        {
            Wing[] wings = GetComponentsInChildren<Wing>();
            root = this;
            foreach (Wing wing in wings) wing.CopyRootValues(this);
        }

        base.UpdateAerofoil();
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        quad.Draw(Vector4.zero, bordersColor, true);
    }
    public void RecursiveSnap()
    {
        if (parent)
        {
            bool snapAffectedShape = shape.SnapTo(parent);
            if (snapAffectedShape)
                UpdateAerofoil();
        }

        if (child) child.RecursiveSnap();
    }
#endif
}
