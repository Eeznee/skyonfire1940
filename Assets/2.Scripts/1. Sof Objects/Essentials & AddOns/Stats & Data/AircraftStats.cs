using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AircraftStats
{
    public SofAircraft aircraft { get; private set; }
    public Wing rootWing { get; private set; }
    public EnginePreset MainEnginePreset { get; private set; }
    public IAirfoil mainAirfoil { get; private set; }
    public float wingSpan { get; private set; }
    public float wingsArea { get; private set; }
    public float wingsIncidence { get; private set; }
    public float altitudeZeroMaxSpeed { get; private set; }
    public float wingLoading { get; private set; }
    public float totalAreaCd { get; private set; }

    public float MinTakeOffSpeedNoFlaps { get; private set; }
    public float MinTakeOffSpeedHalfFlaps { get; private set; }

    private float rollRateSpeedCoefficent;
    private float turningRadiusCoefficent;
    private float negTurningRadiusCoefficent;

    public float LandedRestAngle {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return aircraft.transform.localRotation.eulerAngles.x;
#endif
            return aircraft.card.aircraft.transform.localRotation.eulerAngles.x;
        }
    } 
    public float TurningRadius => turningRadiusCoefficent * Mass;
    public float NegTurningRadius => negTurningRadiusCoefficent * Mass;
    public float MaxTurnRate => 360f * aircraft.data.gsp.Get / (TurningRadius * 2f * Mathf.PI);
    public float MaxNegTurnRate => 360f * aircraft.data.gsp.Get / (NegTurningRadius * 2f * Mathf.PI);

    public float Mass => aircraft.LoadedMass.mass;

    public AircraftStats(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        engines = aircraft.GetComponentsInChildren<Engine>();
        MainEnginePreset = engines.Length > 0 ? engines[0].Preset : null;
        airframes = aircraft.GetComponentsInChildren<SofAirframe>();
        wings = aircraft.GetComponentsInChildren<Wing>();

        foreach (Wing wing in wings)
            if (!wing.parent)
                rootWing = wing;
        
        ComputeStats();
        aircraft.OnInitialize += OnInitialize;
    }

    void OnInitialize()
    {
        ComputeStats();
    }

    private SofAirframe[] airframes;
    private Wing[] wings;
    private Engine[] engines;
    private void ComputeStats()
    {
        ComputeWingStats();
        MinTakeOffSpeedNoFlaps = MinTakeOffSpeed(false);
        MinTakeOffSpeedHalfFlaps = MinTakeOffSpeed(true);
        ComputeTotalAreaCd();
        ComputeRollRate();
        ComputeTurningRadius();

        altitudeZeroMaxSpeed = MaxSpeed(0f, 1f);
    }
    private void ComputeWingStats()
    {
        wingsArea = 0f;
        wingSpan = 0f;
        foreach (Wing wing in wings)
        {
            if (!wing.parent)
            {
                wingsArea += wing.EntireWingArea;
            }
            if (!wing.child)
            {
                Vector3 tipPos = wing.quad.topAeroPos.WorldPos;
                wingSpan = Mathf.Max(aircraft.tr.InverseTransformPoint(tipPos).x * 2f, wingSpan);
            }
        }

        wingLoading = wingsArea == 0f ? 1f : Mass / wingsArea;
        mainAirfoil = rootWing ? rootWing.Airfoil : StaticReferences.Instance.stabilizersAirfoil;
        wingsIncidence = rootWing ? rootWing.shape.incidence : 0f;
    }
    private void ComputeRollRate()
    {
        float totalWingsCoeff = 0f;
        float totalAileronStrength = 0f;
        AircraftAxes fullRoll = new AircraftAxes(0f, 1f, 0f);

        foreach (Wing wing in wings)
        {
            Vector3 worldAeroCenter = wing.quad.centerAero.WorldPos;
            float distanceFromCog = aircraft.tr.InverseTransformPoint(worldAeroCenter).x;
            distanceFromCog = Mathf.Abs(distanceFromCog);

            float airfoilGradient = wing.Airfoil.Gradient();

            float coeff = airfoilGradient * wing.area * distanceFromCog * distanceFromCog;
            totalWingsCoeff += coeff;

            float aileronStrength = Mathf.Abs(wing.ControlSurfaceEffect(fullRoll));
            totalAileronStrength += aileronStrength * airfoilGradient * wing.area * distanceFromCog;
        }

        totalWingsCoeff /= wingSpan * 0.5f;

        float equilibrumRollAngle = totalAileronStrength / totalWingsCoeff;
        rollRateSpeedCoefficent = equilibrumRollAngle / (0.5f * wingSpan);
    }
    private void ComputeTurningRadius()
    {
        float cl = mainAirfoil.Coefficients(mainAirfoil.HighPeakAlpha - 3f).y;
        float liftForce = cl * 0.5f * aircraft.data.density.Get * wingsArea;

        float negCl = mainAirfoil.Coefficients(mainAirfoil.LowPeakAlpha + 2f).y;
        float negLiftForce = negCl * 0.5f * aircraft.data.density.Get * wingsArea;

        turningRadiusCoefficent = 1f / liftForce;
        negTurningRadiusCoefficent = Mathf.Abs(1f / negLiftForce);
    }

    private float MinTakeOffSpeed(bool halfFlaps)
    {
        float weight = Mass * -Physics.gravity.y;
        float density = Aerodynamics.seaLvlDensity;

        float clAreaCombined = 0f;
        foreach(Wing wing in wings)
        {
            float alpha = Mathf.Max(-LandedRestAngle, 10f) - wing.shape.incidence;

            bool useFlaps = wing.hasFlaps && halfFlaps;
            float cl;
            if (useFlaps) cl = wing.Airfoil.Coefficients(alpha, wing.flaps.MainSurface.Design, 0.5f).y;
            else cl = wing.Airfoil.Coefficients(alpha).y;

            clAreaCombined += cl * wing.area;
        }

        return Mathf.Sqrt(weight / (0.5f * clAreaCombined * density));
    }
    private void ComputeTotalAreaCd()
    {
        totalAreaCd = 0f;
        foreach (SofAirframe airframe in airframes)
        {
            totalAreaCd += airframe.AreaCd();
        }

    }
    const float dragAdjustment = 1.45f;
    public float MaxSpeed(float altitude, float throttle)
    {
        if (MainEnginePreset == null) return 0f;

        float totalAreaDrag = totalAreaCd * 0.5f * Aerodynamics.GetAirDensity(altitude);
        totalAreaCd *= dragAdjustment;

        float totalJetThrust = 0f;
        float totalPistonPower = 0f;
        float relativeAirDensity = Aerodynamics.GetAirDensity(altitude) * Aerodynamics.invertSeaLvlDensity;

        foreach (Engine engine in engines)
        {
            if (engine.Preset == null) continue;

            if (engine.Class == EngineClass.JetEngine)
            {
                JetEngine jetEngine = engine as JetEngine;

                totalJetThrust += jetEngine.JetPreset.MaxThrust * throttle * relativeAirDensity;
            }
            if (engine.Class == EngineClass.PistonEngine)
            {
                PistonEngine pistonEngine = engine as PistonEngine;

                float power = pistonEngine.PistonPreset.BestPower(altitude, EngineRunMode.Continuous);
                Propeller propeller = pistonEngine.GetComponentInChildren<Propeller>();
                totalPistonPower += power * throttle * (propeller ? propeller.Efficiency : 0.9f);
            }
        }
        return EquilibrumSpeed(totalPistonPower, totalJetThrust, totalAreaDrag);
    }

    public float EquilibrumSpeed(float power, float thrust, float areaDrag)
    {
        float p = -thrust / areaDrag;
        float q = -power / areaDrag;
        return CardanoFormula(p, q);
    }

    //This is used to solve the thrust, power, drag and speed equation, it is a complicated formula and it uses complex numbers in some cases.
    public float CardanoFormula(float p, float q)
    {
        float cardanoNumber = M.Pow(p, 3) / 27f + M.Pow(q, 2) / 4f;

        if(cardanoNumber > 0f)
        {
            float C = -q / 2f + Mathf.Sqrt(cardanoNumber);
            C = Mathv.CubicRoot(C);

            return C - p / (3f * C);
        }
        else
        {
            Vector2 complexC = new Vector2(-q / 2f, Mathf.Sqrt(-cardanoNumber));
            complexC = ComplexNumbers.Pow(complexC, 1f / 3f);

            Vector2 result = complexC - ComplexNumbers.Divide(p, 3f * complexC);

            return result.x;
        }
    }

    public float RollRateCurrentSpeed()
    {
        return rollRateSpeedCoefficent * aircraft.data.tas.Get;
    }
    public float RollRate(float TAS)
    {
        return rollRateSpeedCoefficent * TAS;
    }

}
