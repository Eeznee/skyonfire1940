using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class MainSurface : ShapedAirframe, IAircraftForce
{
    public override float AreaCd() { return area * Airfoil.MinCD; }

    public SubsurfacesCollection<ControlSurface> controlSurfaces { get; private set; }
    public SubsurfacesCollection<Flap> flaps { get; private set; }
    public SubsurfacesCollection<Slat> slats { get; private set; }
    public float controlSqrt { get; private set; }
    public Wing thisWing { get; private set; }
    public bool isAWing { get; private set; }
    public bool hasSlats { get; private set; }
    public bool hasControlSurface { get; private set; }
    public bool hasFlaps { get; private set; }
    public float inducedDragCoefficient { get; private set; }

    public virtual Transform SubSurfaceParent => tr;

    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);

        controlSurfaces = new SubsurfacesCollection<ControlSurface>(this);
        flaps = new SubsurfacesCollection<Flap>(this);
        slats = new SubsurfacesCollection<Slat>(this);
        thisWing = GetComponent<Wing>();
        isAWing = thisWing != null;

        ReloadControlSurfacesCollections(null);

        if (hasControlSurface) controlSqrt = Mathf.Sqrt(controlSurfaces.MainSurface.quad.midChord / quad.midChord);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        sofModular.onComponentAdded += ReloadControlSurfacesCollections;
        sofModular.onComponentRootRemoved += ReloadControlSurfacesCollections;

        if(isAWing) inducedDragCoefficient = 1f / (Mathv.Square(aircraft.stats.wingSpan) * Mathf.PI * thisWing.oswald);
    }
    private void ReloadControlSurfacesCollections(SofComponent useless)
    {
        controlSurfaces.CheckSubsurfacesArray();
        flaps.CheckSubsurfacesArray();
        slats.CheckSubsurfacesArray();
        hasSlats = slats.MainSurface != null;
        hasControlSurface = controlSurfaces.MainSurface != null;
        hasFlaps = flaps.MainSurface != null;

    }
    public float ControlSurfaceEffect(AircraftAxes axes)
    {
        if (!hasControlSurface) return 0f;
        return ControlSurfaceEffectUnsafe(axes);
    }
    private float ControlSurfaceEffectUnsafe(AircraftAxes axes)
    {
        return controlSurfaces.TotalOverlap * controlSqrt * controlSurfaces.MainSurface.ControlAngle(axes);
    }
    public override Vector2 SimulatedCoefficients(float angleOfAttack, AircraftAxes axes)
    {
        //Slats Effect
        if (hasSlats)
        {
            Slat mainSlat = slats.MainSurface;
            float slatEffect = mainSlat.extend * mainSlat.aoaEffect * Mathf.InverseLerp(15f, 15f + mainSlat.aoaEffect * 2f, angleOfAttack);
            angleOfAttack -= slatEffect * slats.TotalOverlap;
        }

        //Control Surfaces Effect
        if (hasControlSurface)
        {
            float controlEffect = ControlSurfaceEffectUnsafe(axes);
            angleOfAttack -= controlEffect;
        }

        //Flaps Effect
        Vector2 coeffs;
        if (HasAircraft && hasFlaps && aircraft.hydraulics.flaps.state != 0f)
            coeffs = Airfoil.Coefficients(angleOfAttack, flaps.MainSurface.Design, aircraft.hydraulics.flaps.state * flaps.TotalOverlap);
        else 
            coeffs = Airfoil.Coefficients(angleOfAttack);

        //Induced Drag Effect
        if (isAWing && HasAircraft)
            coeffs.x += Mathv.Square(coeffs.y) * aircraft.stats.wingsArea * inducedDragCoefficient;

        //Ground Effect
        if (data.relativeAltitude.Get < 50f)
            coeffs.x *= sofModular.data.groundEffect.Get;
        return coeffs;
    }

#if UNITY_EDITOR
    protected override bool ShowGUI => SofWindow.showWingsOverlay;
#endif

}
