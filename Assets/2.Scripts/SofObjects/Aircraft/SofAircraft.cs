using UnityEngine;
using System.Collections.Generic;


public enum AircraftWorldState
{
    Flying,
    Grounded,
    TakingOff
}

public class SofAircraft : SofComplex
{
    //References
    public AircraftCard card;
    public Station[] stations;
    public HydraulicSystem gear;
    public HydraulicSystem canopy;
    public HydraulicSystem flaps;
    public HydraulicSystem airBrakes;
    public HydraulicSystem bombBay;
    public MaterialsList materials;

    public Game.Squadron squadron;
    public FuselageCore fuselageCore;

    public FuelSystem fuelSystem;
    public Engine[] engines;
    public int bombardierId;
    public int bombardierSeat;
    public BombardierSeat bombSight;
    public OrdnanceLoad[] bombs;
    public OrdnanceLoad[] rockets;
    public OrdnanceLoad[] torpedoes;
    public Gun[] primaries;
    public Gun[] secondaries;
    public Stabilizer hStab;
    public Stabilizer vStab;


    public AircraftStats stats;
    public float maxG = 8f;
    public float maxSpeed = 700 / 3.6f;
    public bool collidersOn = false;

    //Control Surfaces
    public Vector3 controlSpeed = new Vector3(2f, 1.5f, 2f);


    //Auto pilot
    public float cruiseSpeed = 400f / 3.6f;
    public float convergeance = 300f;
    public bool customPIDValues;
    public PID pidPitch;
    public PID pidRoll;
    public PID pidElevator;

    //Deprecated
    public float turnRadius = 250f;
    public float minCrashDelay = 6f;
    public float minInvertAltitude = 200f;

    public Engine.EnginesState enginesState;
    public bool hasPilot = true;
    public bool landed = false;
    public int squadronId;
    public int placeInSquad = 0;
    public float difficulty;
    public string deletePassWord;

    public override void SetReferences()
    {
        base.SetReferences();

        stats = new AircraftStats(this);

        fuselageCore = GetComponentInChildren<FuselageCore>();
        engines = GetComponentsInChildren<Engine>();
        for (int i = 0; i < crew.Length; i++)
            for (int j = 0; j < crew[i].seats.Length; j++)
                if (crew[i] && crew[i].seats[j] && crew[i].seats[j].GetComponent<BombardierSeat>()) 
                {
                    bombardierId = i; bombardierSeat = j; 
                }

        bombSight = GetComponentInChildren<BombardierSeat>();
        rockets = Station.GetOrdnances<RocketsLoad>(stations);
        bombs = Station.GetOrdnances<BombsLoad>(stations);

        Gun[] guns = GetComponentsInChildren<Gun>();
        primaries = Gun.FilterByController(GunController.PilotPrimary, guns);
        secondaries = Gun.FilterByController(GunController.PilotSecondary, guns);
        
        HydraulicControl.AssignHydraulics(this);

        foreach (Stabilizer stab in GetComponentsInChildren<Stabilizer>())
            if (stab.vertical) vStab = stab;
            else hStab = stab;
    }
    protected override void Initialize()
    {
        for (int i = 0; i < stations.Length; i++) stations[i].ChooseOption(squadron.stations[i]);

        gunsPointer = transform.CreateChild("Guns Pointer");
        materials.ApplyMaterials(this);

        base.Initialize();

        InitializeMarkersAndGameReferences();
        InitializeGearAndEngines();

        fuelSystem = new FuelSystem(this);

        if (!customPIDValues) SetDefaultPIDValues();

        //if (squadron.textureName != "") TextureTool.ChangeAircraftTexture(this, squadron.textureName);
    }
    private void InitializeMarkersAndGameReferences()
    {
        squadronId = squadron.id;
        difficulty = squadron.difficulty;
        tag = (squadron.team == Game.Team.Ally) ? "Ally" : (squadron.team == Game.Team.Axis) ? "Axis" : "Neutral";

        if (squadron.team == Game.Team.Ally) GameManager.allyAircrafts.Add(this);
        else GameManager.axisAircrafts.Add(this);
        GameManager.ui.CreateMarker(this);
    }
    private void InitializeGearAndEngines()
    {
        bool grounded = squadron.airfield >= 0;
        throttle = grounded ? 0f : 1f;
        if (gear) gear.SetInstant(grounded);
        if (!grounded) data.rb.velocity = transform.forward * card.startingSpeed / 3.6f;
        this.SetEngines(!grounded, true);
    }
    private void OnCollisionEnter(Collision col)
    {
        Transform tr = col.GetContact(0).thisCollider.transform;
        tr = tr.name.Contains("Skin") ? tr.parent : tr;
        SofAirframe collided = tr.GetComponent<SofAirframe>();
        if (collided && col.impulse.magnitude > 10000f) collided.Rip();

    }

    //INPUTS __________________________________________________________________________
    public Transform gunsPointer;

    public bool controlSent = false;
    public float throttle = 0f;
    public bool boost = false;
    public float brake = 0f;
    public bool primaryFire = false;
    public bool secondaryFire = false;

    public Vector3 controlValue = Vector3.zero;
    public Vector3 controlUncorrected = Vector3.zero;
    public Vector3 controlTarget = Vector3.zero;

    void Update()
    {
        bool destroyedEngine = true;
        bool allOn = true;
        bool allOff = true;
        throttle = 0f;
        foreach (Engine e in engines)
        {
            throttle = Mathf.Max(throttle, e.throttleInput);
            destroyedEngine = destroyedEngine && e.ripped;
            allOn = allOn && e.Working();
            allOff = allOff && !e.Working();
        }
        enginesState = (Engine.EnginesState)(destroyedEngine ? 0 : (allOff ? 1 : (allOn ? 2 : 3)));

        if (data.gsp.Get < 40f && data.relativeAltitude.Get < 5f) landed = true;
        if (data.gsp.Get > cruiseSpeed * 0.7f) landed = false;

        if (enginesState == Engine.EnginesState.Destroyed && data.gsp.Get < 10f) destroyed = true;
    }
    private void FixedUpdate()
    {
        DamageTickFixedUpdate();
        WaterPhysics();

        Vector3 target = controlSent ? controlTarget : Vector3.zero;
        controlValue.x = Mathf.MoveTowards(controlValue.x, target.x, controlSpeed.x * Time.fixedDeltaTime);
        controlValue.y = Mathf.MoveTowards(controlValue.y, target.y, controlSpeed.y * Time.fixedDeltaTime);
        controlValue.z = Mathf.MoveTowards(controlValue.z, target.z, controlSpeed.z * Time.fixedDeltaTime);
        controlSent = false;
    }

    public void SetControls(Vector3 controlInput, bool correctedPitch, bool instant)
    {
        if (hasPilot && !controlSent)
        {
            controlTarget = controlUncorrected = controlInput;
            if (correctedPitch) CorrectPitch(Time.fixedDeltaTime);

            if (instant) controlValue = controlTarget;
            controlSent = true;
        }
    }
    public void SetDefaultPIDValues()
    {
        pidPitch = new PID(new Vector3(0.3f, 0f, 0.03f));
        pidRoll = new PID(new Vector3(4f, 0f, 1f));
        pidElevator = new PID(new Vector3(0.3f, 0.7f, 0.01f));
    }

    private void CorrectPitch(float t)
    {
        float target = controlTarget.x; 

        float spd = Mathf.Max(data.tas.Get, 30f);
        AirfoilSim airfoil = stats.airfoil.airfoilSim;
        float maxCl = Mathf.Lerp(-airfoil.minCl, airfoil.maxCl, target * 0.5f + 0.5f);
        float maxLift = maxCl * data.density.Get * Mathv.SmoothStart(spd, 2) * stats.wingsArea * 0.45f;
        float gravityEffect = Physics.gravity.y * tr.up.y * target;

        float maxTurnRate = (maxLift / rb.mass + gravityEffect) / spd;

        float turnRateState = data.turnRate.Get / maxTurnRate;

        float inAirError = target - turnRateState;
        controlTarget.x = Mathf.Clamp(pidElevator.UpdateAndDebugUnclamped(inAirError, t), -1f, 1f);
    }
    private bool convergedDefault = false;
    public void PointGuns(Vector3 position, float factor)
    {
        if (convergedDefault && factor == 0f) return;
        convergedDefault = factor == 0f;

        Vector3 defaultConvergence = crew[0].Seat.zoomedPOV.position + transform.forward * aircraft.convergeance;

        position = Vector3.Lerp(defaultConvergence, position, factor);
        gunsPointer.LookAt(position, transform.up);
        List<Gun> guns = new List<Gun>(primaries);
        guns.AddRange(secondaries);
        float distance = (position - transform.position).magnitude;
        foreach (Gun gun in guns)
            if (gun && !gun.noConvergeance)
            {
                float t = 1.2f * distance / gun.gunPreset.ammunition.defaultMuzzleVel;
                Vector3 gravityCompensation = -t * t * Physics.gravity.y * 0.5f * Vector3.up;
                gun.bulletSpawn.LookAt(position + gravityCompensation);
            }
    }
    public bool CanPairUp()
    {
        if (card.forwardGuns || placeInSquad % 2 == 0) return false;
        if (GameManager.squadrons[squadronId][placeInSquad - 1].destroyed) return false;
        return true;
    }
    public float TotalAreaCd()
    {
        float areaCd = 0f;
        foreach (SofAirframe airframe in GetComponentsInChildren<SofAirframe>())
            areaCd += airframe.AreaCd();
        return areaCd;
    }
    public float MaxSpeed(float altitude, float throttle)
    {
        float totalDrag = TotalAreaCd() * 0.5f * Aerodynamics.GetAirDensity(altitude);
        bool jet = engines[0].preset.type == EnginePreset.Type.Jet;
        if (jet)
        {
            float relativeAirDensity = Aerodynamics.GetAirDensity(altitude) * Aerodynamics.invertSeaLvlDensity;
            float thrust = throttle * engines[0].preset.maxThrust * engines.Length * relativeAirDensity;
            return Mathf.Sqrt(thrust/totalDrag);
        } else
        {
            float totalPower = throttle * engines.Length * engines[0].preset.gear1.Evaluate(altitude) * 745.7f * GetComponentInChildren<Propeller>().efficiency;
            return Mathf.Pow(totalPower / totalDrag, 0.333333f);
        }
    }
}

