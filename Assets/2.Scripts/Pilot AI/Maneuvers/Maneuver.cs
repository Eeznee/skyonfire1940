using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Maneuver
{
    protected SofAircraft aircraft;
    protected SofAircraft target;
    protected Transform transform;

    public virtual bool MaxPitch()
    {
        return false;
    }
    public virtual void Execute()
    {

    }
    public virtual void Execute(AI.GeometricData data)
    {
        aircraft = data.aircraft;
        transform = aircraft.transform;
        target = data.target;
    }
    public virtual void Execute(SofAircraft aircraft)
    {

    }
}
public class TurnData
{
    public SofAircraft aircraft;
    public float bankAngle;
    public float turnTime;
    public float intensity;
    private float count;
    public bool ended;

    public TurnData(SofAircraft a, float ba, float tt, float i)
    {
        aircraft = a;
        bankAngle = ba;
        turnTime = tt;
        intensity = Mathf.Clamp01(i);
        count = 0f;
        ended = false;
    }
    public void TurnFixedTime()
    {
        if (ended) return;
        AircraftAxes axes = PointTracking.TrackingInputs(aircraft.transform.position + aircraft.transform.forward * 500f, aircraft, bankAngle, 1f, true);
        axes.pitch = intensity;
        aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
        count += Time.fixedDeltaTime;
        if (count >= turnTime) ended = true;
    }
}

public class ManeuversLibrary
{
    private List<ActiveManeuver> allDefensiveManeuvers;
    private List<ActiveManeuver> allNeutralManeuvers;
    private List<ActiveManeuver> allOffensiveManeuvers;
    const float nullManeuverRate = 1f;
    private float expertise;

    public ActiveManeuver PickManeuver(AI.GeometricData data, List<ActiveManeuver> maneuvers)
    {
        int count = maneuvers.Count;
        if (maneuvers.Count == 0) return null;

        float[] pickRates = new float[count + 1];
        for (int i = 0; i < count; i++)
            pickRates[i] = maneuvers[i].PickFactor(data);

        pickRates[count] = nullManeuverRate;

        int picked = RandomSelectionByPickRate(pickRates);

        if (picked != count)
        {
            maneuvers[picked].Initialize(data);
            return maneuvers[picked];
        }
        return null;
    }
    public ActiveManeuver PickManeuver(AI.GeometricData data)
    {
        switch (data.state)
        {
            case AI.DogfightState.Offensive:
                return PickManeuver(data, allOffensiveManeuvers);
            case AI.DogfightState.Defensive:
                return PickManeuver(data, allDefensiveManeuvers);
            case AI.DogfightState.Engage:
                if (data.crossAngle < 30f) return PickManeuver(data, allOffensiveManeuvers);
                break;
            default:
                break;
        }
        return null;
    }
    public static int RandomSelectionByPickRate(float[] elementsPickFactors)
    {
        float sum = 0f;
        foreach (float epf in elementsPickFactors)
        {
            sum += epf;
        }

        float pick = Random.Range(0f, sum);
        for (int i = 0; i < elementsPickFactors.Length; i++)
        {
            float val = elementsPickFactors[i];
            if (pick <= val) return i;
            pick -= val;
        }
        return 0;
    }

    public ManeuversLibrary(float _expertise, bool turretFighter)
    {
        expertise = _expertise;

        if (turretFighter)
        {
            allDefensiveManeuvers = new List<ActiveManeuver>() { new HammerHead(), 
            new Sliceback(), new ZoomOut() };
            allNeutralManeuvers = new List<ActiveManeuver>(0);
            allOffensiveManeuvers = new List<ActiveManeuver>() {  new DefiantBellyAssault(),
            new DefiantSideAssault() };
        }
        else
        {
            allDefensiveManeuvers = new List<ActiveManeuver>() { new BreakTurn(),  
            new HammerHead(), new Overshoot(), new RollingScissors(), new Sliceback() ,new ZoomOut()};
            allNeutralManeuvers = new List<ActiveManeuver>(0);
            allOffensiveManeuvers = new List<ActiveManeuver>(0);
        }

        for (int i = 0; i < Mathf.Floor(allDefensiveManeuvers.Count * (1f - expertise) + Random.value); i++)
            allDefensiveManeuvers.RemoveAt(Random.Range(0, allDefensiveManeuvers.Count - 1));
        for (int i = 0; i < Mathf.Floor(allNeutralManeuvers.Count * (1f - expertise) + Random.value); i++)
            allNeutralManeuvers.RemoveAt(Random.Range(0, allNeutralManeuvers.Count - 1));
    }
}
