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
    public AircraftCard card;
    public Station[] stations;
    public MaterialsList materials;
    public float maxG = 8f;
    public float maxSpeed = 700 / 3.6f;
    public AircraftAxes axesSpeed = new AircraftAxes(2f, 1.5f, 2f);
    public float convergeance = 300f;

    public float cruiseSpeed = 400f / 3.6f;
    public bool customPIDValues;
    public PID pidPitch;
    public PID pidRoll;
    public PID pidElevator;

    public Game.Squadron squadron;
    public int squadronId;
    public int placeInSquad;
    public float difficulty;

    public AircraftInputs inputs;
    public bool hasPilot;

    public BombardierSeat bombardierSeat;
    public Bombsight bombSight;
    public Animator animator;
    public AircraftStats stats;
    public HydraulicsManager hydraulics;
    public FuelManager fuel;
    public ArmamentManager armament;
    public EnginesManager engines;


    //Deprecated
    public float turnRadius = 250f;
    public float minCrashDelay = 6f;
    public float minInvertAltitude = 200f;

    public override void SetReferences()
    {
        foreach (Station s in stations) if (stations != null && s != null) s.UpdateOptions();

        base.SetReferences();

        bombardierSeat = GetComponentInChildren<BombardierSeat>();
        bombSight = GetComponentInChildren<Bombsight>();
        animator = GetComponent<Animator>();
        stats = new AircraftStats(this);
        hydraulics = new HydraulicsManager(this);
        armament = new ArmamentManager(this);
        engines = new EnginesManager(this);
        fuel = new FuelManager(this);
    }
    protected override void Initialize()
    {
        if (!customPIDValues) SetDefaultPIDValues();
        inputs = new AircraftInputs(this);

        AssignImportantComponents();

        for (int i = 0; i < stations.Length; i++) stations[i].ChooseOption(squadron.stations[i]);

        materials.ApplyMaterials(this);

        base.Initialize();

        InitializeMarkersAndGameReferences();
        GroundedInitialization();

        armament.ConvergeGuns(convergeance);

        hasPilot = GetComponentInChildren<PilotSeat>() && crew.Length > 0;
    }
    private void AssignImportantComponents()
    {
        lod = GetComponentInChildren<ObjectLOD>();
        if (!lod) lod = tr.CreateChild("LOD Manager").gameObject.AddComponent<ObjectLOD>();
        if (!simpleDamage) bubble = tr.CreateChild("Object Bubble").gameObject.AddComponent<ObjectBubble>();
    }
    private void GroundedInitialization()
    {
        bool grounded = squadron.airfield >= 0;

        if(hydraulics.gear) hydraulics.gear.SetInstant(grounded);
        engines.Initialize(grounded);
        if (!grounded) data.rb.velocity = transform.forward * card.startingSpeed / 3.6f;
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
        tr = tr.name.Contains("Skin") ? tr.parent : tr;
        SofAirframe collided = tr.GetComponent<SofAirframe>();
        if (collided && col.impulse.magnitude > 10000f) collided.Rip();

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) tr.position += Vector3.up * 0.60f;
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

