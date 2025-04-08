using UnityEngine;
using System.Collections.Generic;
using System;

public enum FlightPhase
{
    Landed,
    TakingOff,
    InFlight
}

public class SofAircraft : SofComplex
{
    public AircraftCard card;
    [SerializeField] private Station[] stations;

    public Station[] Stations => stations;


    [SerializeField] private float maxG = 8f;
    [SerializeField] private float speedLimitKph = 700f;
    [SerializeField] private float convergeance = 300f;

    public float SpeedLimitMps => speedLimitKph * UnitsConverter.kphToMps;
    public float MaxGForce => maxG;
    public float Convergence => convergeance;


    [SerializeField] private bool customPIDValues;
    [SerializeField] public PID pidPitch;
    [SerializeField] public PID pidRoll;
    [SerializeField] private float stickTorqueFactor = 1f;
    [SerializeField] private float stallMarginAngle = 1f;
    [SerializeField] private bool hydraulicControls = false;


    public bool CustomPIDValues => customPIDValues;
    public float StickTorqueFactor => stickTorqueFactor;
    public float StallMarginAngle => stallMarginAngle;
    public bool HydraulicControls => hydraulicControls;



    public Game.Squadron squadron;
    public int placeInSquad;



    public PilotSeat mainSeat { get; private set; }
    public BombardierSeat bombardierSeat { get; private set; }
    public Bombsight bombSight { get; private set; }
    public Animator animator { get; private set; }
    public AircraftStats stats { get; private set; }


    //Subsystems
    public AircraftInputs controls;
    public HydraulicsManager hydraulics;
    public FuelManager fuel;
    public ArmamentManager armament;
    public EnginesManager engines;
    public ForcesCompiler forcesCompiler { get; private set; }

    public float cruiseSpeed => stats.altitudeZeroMaxSpeed;
    public float Difficulty => squadron.difficulty;
    public int SquadronId => squadron.id;
    public bool GroundedStart => squadron.airfield >= 0;


    private bool landed;
    private float timeSinceLastLanding;
    public bool Landed => landed;
    public float TimeSinceLastLanding => timeSinceLastLanding;


    [NonSerialized] public Vector2 ptAbstractControls = Vector2.zero;
    [NonSerialized] public Vector2 ptMultipliers = Vector2.one;


    public void ResetStationsToDefault()
    {
        foreach (Station s in Stations) if (Stations != null && s != null) s.SelectAndDisactivate(0);
    }

    public override void SetReferences()
    {
        foreach (Station s in Stations) if (Stations != null && s != null) s.SelectAndDisactivate();

        bombardierSeat = GetComponentInChildren<BombardierSeat>();
        mainSeat = GetComponentInChildren<PilotSeat>();
        bombSight = GetComponentInChildren<Bombsight>();
        animator = GetComponent<Animator>();

        base.SetReferences();

        stats = new AircraftStats(this);

        hydraulics = new HydraulicsManager(this);
        armament = new ArmamentManager(this);
        engines = new EnginesManager(this);
        fuel = new FuelManager(this);
        controls = new AircraftInputs(this);
    }
    protected override void GameInitialization()
    {
        ptAbstractControls = Vector2.zero;
        ptMultipliers = Vector2.one;

        forcesCompiler = gameObject.AddComponent<ForcesCompiler>();
        if (!customPIDValues) SetDefaultPIDValues();

        AddEssentialComponents();

        for (int i = 0; i < Stations.Length; i++) Stations[i].SelectAndDestroy(squadron.stations[i]);

        base.GameInitialization();

        InitializeMarkersAndGameReferences();

        if (!GroundedStart) rb.velocity = transform.forward * stats.MaxSpeed(data.altitude.Get, 1f);
        timeSinceLastLanding = GroundedStart ? 0f : 600f;
    }
    private void AddEssentialComponents()
    {
        lod = this.GetCreateComponentInChildren<ObjectLOD>();
        if (!simpleDamage) bubble = transform.CreateChild("Object Bubble").gameObject.AddComponent<ObjectBubble>();
    }
    private void InitializeMarkersAndGameReferences()
    {
        tag = (squadron.team == Game.Team.Ally) ? "Ally" : (squadron.team == Game.Team.Axis) ? "Axis" : "Neutral";

        if (squadron.team == Game.Team.Ally) GameManager.allyAircrafts.Add(this);
        else GameManager.axisAircrafts.Add(this);
        MarkersManager.Add(this);
    }
    private void OnCollisionEnter(Collision col)
    {
        Transform tr = col.GetContact(0).thisCollider.transform;
        tr = tr.GetComponent<WingSkin>() ? tr.parent : tr;
        SofAirframe collided = tr.GetComponent<SofAirframe>();
        if (collided && col.impulse.magnitude > 10000f) collided.Rip();

    }
    void Update()
    {
        engines.Update();
    }
    private void FixedUpdate()
    {
        WaterPhysics();

        //Call the pilot seat behaviour before inputs and force compilation
        CrewMember mainPilot = mainSeat.seatedCrew;
        if(mainPilot != null && !mainPilot.ripped && mainPilot.complex == complex)
        {
            if (mainPilot == Player.crew) mainSeat.PlayerFixed(mainPilot);
            else mainSeat.AiFixed(mainPilot);
        }

        landed = data.tas.Get < stats.MinTakeOffSpeedNoFlaps && data.relativeAltitude.Get < 10f;
        if (landed) timeSinceLastLanding = 0f;
        else timeSinceLastLanding += Time.fixedDeltaTime;

        controls.FixedUpdate();
        forcesCompiler.ApplyForcesOnFixedUpdate();
    }
    public void SetDefaultPIDValues()
    {
        pidPitch = new PID(new Vector3(0.03f, 0f, 0.005f));
        pidRoll = new PID(new Vector3(2f, 0f, 0.5f));
    }
}

