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

    public ControlSurface mainControlSurface { get; private set; }
    public float controlSurfaceCoefficient { get; private set; }
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

        if (hasControlSurface) controlSurfaceCoefficient = controlSurfaces.TotalOverlap *  Mathf.Sqrt(controlSurfaces.MainSurface.quad.midChord / quad.midChord);
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
        hasSlats = slats.MainSurface != null && slats.TotalOverlap != 0f;
        hasControlSurface = controlSurfaces.MainSurface != null && controlSurfaces.TotalOverlap != 0f;
        hasFlaps = flaps.MainSurface != null && flaps.TotalOverlap != 0f;

        mainControlSurface = controlSurfaces.MainSurface;
    }
    public float ControlSurfaceEffect(AircraftAxes axes)
    {
        if (!hasControlSurface) return 0f;
        return ControlSurfaceEffectUnsafe(axes);
    }
    private float ControlSurfaceEffectUnsafe(AircraftAxes axes)
    {
        return controlSurfaceCoefficient * mainControlSurface.ControlAngle(axes);
    }
    public override Vector2 SimulatedCoefficients(float angleOfAttack, float pointSpeed, AircraftAxes axes)
    {
        //Slats Effect
        if (hasSlats)
        {
            angleOfAttack -= slats.MainSurface.SlatEffect(pointSpeed, angleOfAttack) * slats.TotalOverlap;
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
            coeffs = airfoil.Coefficients(angleOfAttack, flaps.MainSurface.Design, aircraft.hydraulics.flaps.state * flaps.TotalOverlap);
        else 
            coeffs = airfoil.Coefficients(angleOfAttack);

        //Induced Drag Effect
        if (isAWing && HasAircraft)
            coeffs.x += coeffs.y * coeffs.y * aircraft.stats.wingsArea * inducedDragCoefficient;

        //Ground Effect
        if (data.relativeAltitude.Get < 50f)
        {
            coeffs.x *= sofModular.data.dragGroundEffect.Get;
            coeffs.y *= sofModular.data.liftGroundEffect.Get;
        }


        return coeffs;
    }

#if UNITY_EDITOR
    protected override bool ShowGUI => SofWindow.showWingsOverlay;
#endif

}
