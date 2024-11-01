using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Wing")]
public class Wing : MainSurface
{
    public override float HpPerSquareMeter => ModulesHPData.sparHpPerSq;
    public override ModuleArmorValues Armor => ModulesHPData.SparArmor;


    public bool split = false;
    public float splitFraction = 0.5f;

    public float oswald = 0.75f;
    public WingSkin skin;
    public Mesh skinMesh;

    public Wing parent { get; private set; }
    public Wing child { get; private set; }
    public Wing root { get; private set; }

    public ComplexAeroSurface outerSplitAero { get; private set; }


    public float alpha { get; private set; }


    public SurfaceQuad OuterQuad => split ? outerSplitAero.quad : aeroSurface.quad;
    public override float AirframeDamage => base.AirframeDamage * (skin ? skin.structureDamage : 1f);
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
            return aircraft.maxSpeed * coeff;
        }
    }
    public override float MaxG
    {
        get
        {
            float coeff = 1f;
            if (parent) coeff += 0.15f;
            if (!child) coeff += 0.15f;
            return aircraft.maxG * coeff;
        }
    }
    public override float Integrity => skin.structureDamage;

    public override void SetReferences(SofComplex _complex)
    {
        child = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Wing>() : null;
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
        base.SetReferences(_complex);
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);
    }
    public override void CreateAeroSurface()
    {
        if (split)
        {
            SurfaceQuad[] splitQuad = quad.Split(splitFraction);
            aeroSurface = new ComplexAeroSurface(this, splitQuad[0]);
            outerSplitAero = new ComplexAeroSurface(this, splitQuad[1]);
        }
        else base.CreateAeroSurface();

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
    protected override void FixedUpdate()
    {
        alpha = ApplyForces();
        //if(name == "Right Wing Base" || name == "Right Wing Section 1")Debug.Log(alpha);
    }
    public override Vector2 Coefficients(float angleOfAttack)
    {
        if (split)
        {
            Vector2 c1 = aeroSurface.Coefficients(angleOfAttack);
            Vector2 c2 = outerSplitAero.Coefficients(angleOfAttack);

            return Vector2.Lerp(c1, c2,outerSplitAero.quad.Area / area);
        }
        return base.Coefficients(angleOfAttack);
    }

#if UNITY_EDITOR
    private Color AirfoilSurfaceColor(ComplexAeroSurface complexAero)
    {
        if (complexAero.control) return aileronColor;
        if (complexAero.flap) return flapColor;
        return Vector4.zero;
    }
    public override void Draw()
    {
        aeroSurface.quad.Draw(AirfoilSurfaceColor(aeroSurface), aeroSurface.slat ? Color.green : bordersColor, true);
        if (split)
            outerSplitAero.quad.Draw(AirfoilSurfaceColor(outerSplitAero), outerSplitAero.slat ? Color.green : bordersColor, true);
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
