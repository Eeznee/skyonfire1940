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
    private bool hasSkin;
    public Mesh skinMesh;
    public SparSettings[] sparSettings = new SparSettings[] { new SparSettings() };

    public Wing parent { get; private set; }
    public Wing child { get; private set; }
    public Wing root { get; private set; }

    public override Transform SubSurfaceParent => root.tr;
    public override float AirframeDamage => base.AirframeDamage * (hasSkin ? skin.structureDamage : 1f);
    public override float AerodynamicIntegrity => hasSkin ? skin.structureDamage : structureDamage;
    public override IAirfoil Airfoil => airfoil ? airfoil : StaticReferences.Instance.stabilizersAirfoil;

    protected override Collider MainCollider => skin ? skin.skinCollider : base.MainCollider;
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


    public override void SetReferences(SofModular _complex)
    {
        child = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Wing>() : null;
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
        base.SetReferences(_complex);

        airfoil?.UpdateValues();
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);
        if(airfoil == null) Debug.LogError(aircraft.name + "  " + name + " does not have an airfoil assigned");

        foreach (SparSettings spar in sparSettings)
        {
            spar.CreateBoxCollider(this);
        }
        hasSkin = skin != null;
    }

    public void CopyRootValues(Wing rootWing)
    {
        root = rootWing;
        airfoil = root.airfoil;
        oswald = root.oswald;
        sparSettings = root.sparSettings;
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
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (SofWindow.showWingsSpars && !Application.isPlaying)
        {
            foreach (SparSettings spar in sparSettings)
            {
                spar.DrawGizmos(this);
            }
        }
    }
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
