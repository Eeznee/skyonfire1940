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
    public override ModuleArmorValues Armor(Collider collider)
    {
        if (collider == MainCollider) return ModulesHPData.DuraluminArmor;
        else return ModulesHPData.SparArmor;
    }

    public float oswald = 0.75f;

    public SparSettings[] sparSettings = new SparSettings[] { new SparSettings() };

    public float surfaceDamage { get; private set; }
    const float caliberToHoleRatio = 15f;


    public Wing parent { get; private set; }
    public Wing child { get; private set; }
    public Wing root { get; private set; }

    public override Transform SubSurfaceParent => root.tr;
    public override float AerodynamicIntegrity => surfaceDamage;
    public override float StructuralIntegrity => base.StructuralIntegrity * surfaceDamage;
    public override IAirfoil Airfoil => airfoil ? airfoil : StaticReferences.Instance.stabilizersAirfoil;

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
        surfaceDamage = 1f;
        if(airfoil == null) Debug.LogError(aircraft.name + "  " + name + " does not have an airfoil assigned");

        foreach (SparSettings spar in sparSettings)
        {
            spar.CreateBoxCollider(this);
        }
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

    public override void ProjectileDamage(float hpDamage, float caliber, float fireCoeff, Collider colliderHit)
    {
        if(colliderHit == MainCollider)
        {
            float holeArea = Mathv.SmoothStart(caliber * caliberToHoleRatio / 2000f, 2) * Mathf.PI;
            surfaceDamage = Mathf.Clamp01(surfaceDamage - holeArea / area * surfaceDamage);
        }
        else
        {
            base.ProjectileDamage(hpDamage, caliber, fireCoeff, colliderHit);
        }
    }
    public override void ExplosionDamage(Vector3 explosionOrigin, float tnt, out float damage, out float hole)
    {
        base.ExplosionDamage(explosionOrigin, tnt, out damage, out hole);

        surfaceDamage = Mathf.Clamp01(surfaceDamage - hole / area * surfaceDamage);
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
