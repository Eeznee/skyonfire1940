using UnityEngine;
using System.Collections.Generic;


public class SofAircraft : SofComplex
{
    public AircraftCard card;
    public Station[] stations;
    public MaterialsList materials;
    public float maxG = 8f;
    public float maxSpeed = 700 / 3.6f;
    public AircraftAxes axesSpeed = new AircraftAxes(2f, 1.5f, 2f);
    public float convergeance = 300f;

    public bool customPIDValues;
    public PID pidPitch;
    public PID pidRoll;
    public PID pidElevator;

    public Game.Squadron squadron;
    public int squadronId;
    public int placeInSquad;
    public float difficulty { get; private set; }

    public AircraftInputs inputs;

    public BombardierSeat bombardierSeat { get; private set; }
    public Bombsight bombSight { get; private set; }
    public Animator animator { get; private set; }
    public AircraftStats stats { get; private set; }
    public HydraulicsManager hydraulics { get; private set; }
    public FuelManager fuel { get; private set; }
    public ArmamentManager armament { get; private set; }
    public EnginesManager engines { get; private set; }

    public float cruiseSpeed => stats.altitudeZeroMaxSpeed;


    //Deprecated
    public float turnRadius = 250f;
    public float minCrashDelay = 6f;
    public float minInvertAltitude = 200f;

    public bool GroundedStart => squadron.airfield >= 0;

    public void ResetStationsToDefault()
    {
        foreach (Station s in stations) if (stations != null && s != null) s.SelectAndDisactivate(0);
    }

    public override void SetReferences()
    {
        foreach (Station s in stations) if (stations != null && s != null) s.SelectAndDisactivate();

        bombardierSeat = GetComponentInChildren<BombardierSeat>();
        bombSight = GetComponentInChildren<Bombsight>();
        animator = GetComponent<Animator>();

        base.SetReferences();

        stats = new AircraftStats(this);
    }
    protected override void GameInitialization()
    {
        if (!customPIDValues) SetDefaultPIDValues();

        AddEssentialComponents();

        for (int i = 0; i < stations.Length; i++) stations[i].SelectAndDestroy(squadron.stations[i]);

        base.GameInitialization();

        InitializeMarkersAndGameReferences();

        if (!GroundedStart) data.rb.velocity = transform.forward * stats.MaxSpeed(data.altitude.Get, 1f);

        CreateManagers();
    }
    private void AddEssentialComponents()
    {
        lod = this.GetCreateComponentInChildren<ObjectLOD>();
        if (!simpleDamage) bubble = transform.CreateChild("Object Bubble").gameObject.AddComponent<ObjectBubble>();
    }
    private void CreateManagers()
    {
        hydraulics = new HydraulicsManager(this);
        armament = new ArmamentManager(this);
        engines = new EnginesManager(this);
        fuel = new FuelManager(this);
        inputs = new AircraftInputs(this);
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

        inputs.FixedUpdate();
    }
    public void SetDefaultPIDValues()
    {
        pidPitch = new PID(new Vector3(0.3f, 0f, 0.03f));
        pidRoll = new PID(new Vector3(4f, 0f, 1f));
        pidElevator = new PID(new Vector3(0.3f, 0.7f, 0.01f));
    }
}

