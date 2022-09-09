
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SofAircraft : SofComplex
{
    //References
    public AircraftCard card;
    public Engine[] engines;
    public WheelCollider[] wheels;
    public HydraulicSystem gear;
    public HydraulicSystem cannopy;
    public HydraulicSystem flaps;
    public HydraulicSystem airBrakes;
    public HydraulicSystem bombBay;
    public BombardierSeat bombSight;
    public SeatPath bombardierPath;
    public FuelTank[] fuelTanks;
    public Gun[] primaries;
    public Gun[] secondaries;
    public HardPoint[] hardPoints;
    public Stabilizer hStab;
    public Stabilizer vStab;
    public AirfoilPreset foil;

    //Physics
    public Vector3 emptyCOI = new Vector3(5f, 5f, 3f);
    public Vector3 emptyCOG = Vector3.zero;
    public float wingSpan = 1f;
    public float maxG = 8f;
    public float maxSpeed = 700 / 3.6f;
    public bool collidersOn = false;

    //Control Surfaces
    public Vector3 controlSpeed = new Vector3(2f, 1.5f, 2f);
    public Vector3 controlInput = Vector3.zero;

    //Auto pilot
    public float cruiseSpeed = 400f / 3.6f;
    public float convergeance = 500f;
    public float bankTurnAngle = 15f;
    public float optiAlpha = 12f;
    public float turnRadius = 250f;
    public float minCrashDelay = 6f;
    public float minInvertAltitude = 200f;
    public PID pidPitch;
    public PID pidRoll;
    public PID pidElevator;

    //Data
    public Engine.EnginesState enginesState;
    public bool hasPilot = true;
    public bool landed = false;
    public int squadronId;
    public int placeInSquad = 0;
    public float difficulty;
    public string deletePassWord;

    public void SpawnInitialization(Spawner.Type spawntype, Game.Team _team, int _squadId, int _wing,float _diff)
    {
        tag = (_team == Game.Team.Ally) ? "Ally" : (_team == Game.Team.Axis) ? "Axis" : "Neutral";
        if (_team == Game.Team.Ally) GameManager.allyAircrafts.Add(this);
        else GameManager.axisAircrafts.Add(this);

        squadronId = _squadId;
        placeInSquad = _wing;
        difficulty = _diff;

        throttle = spawntype == Spawner.Type.InAir ? 1f : 0f;
        SetEngines(spawntype != Spawner.Type.Parked, true);
        if (gear) { gear.SetInstant(spawntype != Spawner.Type.InAir); }
        foreach (HardPoint point in hardPoints) point.LoadGroup();

        GameManager.ui.CreateMarker(this);
    }
    public override void Awake()
    {
        gunsPointer = new GameObject("Guns Pointer").transform;
        gunsPointer.parent = transform;
        gunsPointer.SetPositionAndRotation(transform.position, transform.rotation);
        wheels = GetComponentsInChildren<WheelCollider>();
        engines = GetComponentsInChildren<Engine>();

        primaries = GetComponentsInChildren<PrimaryGun>();
        secondaries = GetComponentsInChildren<SecondaryGun>();
        Stabilizer[] stabs = GetComponentsInChildren<Stabilizer>();
        foil = GetComponentInChildren<Airfoil>().airfoil;
        foreach (Stabilizer stab in stabs)
        {
            if (stab.rudder) vStab = stab;
            else hStab = stab;
        }

        base.Awake();
        data.rb.centerOfMass = emptyCOG;
        data.rb.inertiaTensor = emptyCOI * data.mass;
    }
    private void OnCollisionEnter(Collision col)
    {
        Transform tr = col.GetContact(0).thisCollider.transform;
        tr = tr.name.Contains("Skin") ? tr.parent : tr;
        Fuselage collided = tr.GetComponent<Fuselage>();
        if (collided && col.impulse.magnitude > 10000f) collided.Rip();
    }

    //INPUTS __________________________________________________________________________
    public Transform gunsPointer;
    public Vector3 controlTarget = Vector3.zero;
    public bool controlSent = false;
    public float throttle = 0f;
    public bool boost = false;
    public float flapsInput = 0f;
    public float mixture = 0.5f;
    public float brake = 0f;
    public bool primaryFire = false;
    public bool secondaryFire = false;
    public bool gunnersFire = true;
    private float correction = 1f;

    void Update()
    {
        flapsInput = flaps ? flaps.state : 0f;

        bool destroyedEngine = true;
        bool allOn = true;
        bool allOff = true;
        foreach (Engine e in engines)
        {
            destroyedEngine = destroyedEngine && e.ripped;
            allOn = allOn && e.Working();
            allOff = allOff && !e.Working();
        }
        enginesState = (Engine.EnginesState)(destroyedEngine ? 0 : (allOff ? 1 : (allOn ? 2 : 3)));

        if (data.gsp < 40f && data.relativeAltitude < 5f) landed = true;
        if (data.gsp > cruiseSpeed * 0.7f) landed = false;

        if (enginesState == Engine.EnginesState.Destroyed && data.gsp < 10f) destroyed = true;
    }
    private void FixedUpdate()
    {
        Vector3 target = controlSent ? controlTarget : Vector3.zero;
        controlInput.x = Mathf.MoveTowards(controlInput.x, target.x, controlSpeed.x * Time.fixedDeltaTime);
        controlInput.y = Mathf.MoveTowards(controlInput.y, target.y, controlSpeed.y * Time.fixedDeltaTime);
        controlInput.z = Mathf.MoveTowards(controlInput.z, target.z, controlSpeed.z * Time.fixedDeltaTime);

        controlSent = false;
    }
    //Axis
    public void SetControls(Vector3 CsInputs, bool correctedPitch, bool instant)
    {
        if (hasPilot)
        {
            controlTarget = CsInputs;
            if (instant) controlInput = CsInputs;
            if (correctedPitch) UpdatePidElevator(CsInputs.x, Time.fixedDeltaTime);
            controlSent = true;
        }
    }
    private void UpdatePidElevator(float target, float t)
    {
        float correctionTarget = data.relativeAltitude < 3f && data.gsp < cruiseSpeed * 0.7f ? 0f : 1f;
        correction = Mathf.MoveTowards(correction, correctionTarget, t * 0.1f);
        if (data.gsp < 3f && data.relativeAltitude < 5f) correction = 0f;

        float inAirError = target - data.angleOfAttack / optiAlpha;
        controlTarget.x = Mathf.Lerp(target, pidElevator.Update(inAirError, t), correction);
    }
    //Engines
    public void SetEngines(bool on, bool instant)
    {
        if (hasPilot)
        {
            foreach (Engine pe in engines)
            {
                pe.throttleInput = throttle;
                pe.Set(on, instant);
            }
            if (GameManager.player.aircraft == this) Log.Print((engines.Length == 1 ? "Engine " : "Engines ") + (on ? "On" : "Off"), "engines");
        }
    }
    public void SetEngines()
    {
        if (hasPilot)
            SetEngines(enginesState == Engine.EnginesState.Off, false);
    }
    public void SetThrottle(float thr)
    {
        if (hasPilot)
        {
            throttle = Mathf.Clamp01(thr);
            foreach (Engine engine in engines) engine.throttleInput = throttle;
        }
    }
    //Mechanical
    public void SetFlaps(int input) { 
        if (flaps && hasPilot)
        {
            flaps.SetDirection(input);
            if (GameManager.player.aircraft == this && input != 0)
            {
                float state = flaps.binary ? flaps.stateInput * 100f : flaps.state * 100f;
                string txt = "Flaps : " + state.ToString("0") + " %";
                if (flaps.Destroyed()) txt = "Flaps Unoperational";
                Log.Print(txt, "flaps");
            }
        }
    }
    public void SetGear()
    {
        if (gear && hasPilot && data.relativeAltitude > 4f)
        {
            gear.Set();
            if (GameManager.player.aircraft == this)
            {
                string txt = "Landing Gear " + (gear.stateInput == 1f ? "Deploying" : "Retracting");
                if (gear.Destroyed()) txt = "Landing Gear Unoperational";
                Log.Print(txt, "gear");
            }
        }
    }
    public void SetAirBrakes()
    {
        if (airBrakes && hasPilot)
        {
            airBrakes.Set();
            if (GameManager.player.aircraft == this)
            {
                string txt = "Airbrakes " + (airBrakes.stateInput == 1f ? "Deploying" : "Retracting");
                if (airBrakes.Destroyed()) txt = "Air Brakes Unoperational";
                Log.Print(txt, "airbrakes");
            }
        }
    }
    public void SetBombBay()
    {
        if (bombBay && hasPilot)
        {
            bombBay.Set();
            if (GameManager.player.aircraft == this) Log.Print("Bomb Bay " + (bombBay.stateInput == 1f ? "Opening" : "Closing"), "bombbay");
        }
    }
    public void SetCannopy()
    {
        if (cannopy && hasPilot)
        {
            cannopy.Set();
            if (GameManager.player.aircraft == this)
            {
                string txt = "Canopy " + (cannopy.stateInput == 1f ? "Opening" : "Closing");
                if (cannopy.Destroyed()) txt = "Canopy Is Gone";
                Log.Print(txt, "canopy");
            }
        }
    }
    public void FirePrimaries() { if (hasPilot && !(primaries.Length > 20 && bombBay.state < 0.8f)) foreach (Gun g in primaries) g.Trigger(); }
    public void FireSecondaries() { if (hasPilot) foreach (Gun g in secondaries) g.Trigger(); }
    public void ToggleGunners() { gunnersFire = !gunnersFire; }
    public void PointGuns(Vector3 position,float factor)
    {
        position = Vector3.Lerp(transform.position + transform.forward * 300f, position, factor);
        gunsPointer.LookAt(position, transform.up);
        foreach (Gun pg in primaries)
        {
            if (pg && !pg.noConvergeance)
            {
                Transform t = pg.transform;
                t.localRotation = Quaternion.identity;
                t.forward = Vector3.RotateTowards(t.forward, position - t.position, Mathf.Deg2Rad * 10f, 0f);
            }
        }

        foreach (Gun sg in secondaries)
            if (sg && !sg.noConvergeance)
            {
                Transform t = sg.transform;
                t.localRotation = Quaternion.identity;
                t.forward = Vector3.RotateTowards(t.forward, position - t.position, Mathf.Deg2Rad * 10f, 0f);
            }
    }
    public void PointGuns()
    {
        PointGuns(transform.position + transform.forward * 300f,1f);
    }
    public void DropBomb()
    {
        HardPoint best = hardPoints[0].BestHardPoint();
        if (best) best.Drop();
    }

    public bool CanPairUp()
    {
        if (card.forwardGuns || placeInSquad % 2 == 0) return false ;
        if (GameManager.squadrons[squadronId][placeInSquad - 1].destroyed) return false;
        return true;
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 cog = transform.TransformPoint(emptyCOG);
        Gizmos.DrawWireSphere(cog, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cog - transform.right * emptyCOI.x * 0.5f, cog + transform.right * emptyCOI.x * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cog - transform.up * emptyCOI.y * 0.5f, cog + transform.up * emptyCOI.y * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(cog - transform.forward * emptyCOI.z * 0.5f, cog + transform.forward * emptyCOI.z * 0.5f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofAircraft))]
public class SofAircraftEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofAircraft aircraft = (SofAircraft)target;
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Aircraft Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        aircraft.card = EditorGUILayout.ObjectField("Aircraft Card", aircraft.card, typeof(AircraftCard), false) as AircraftCard;
        aircraft.bubble = EditorGUILayout.ObjectField("Bubble collider", aircraft.bubble, typeof(SphereCollider), true) as SphereCollider;
        aircraft.controlSpeed = EditorGUILayout.Vector3Field("Controls Speed (P/Y/R)", aircraft.controlSpeed);
        GUILayout.Space(7f);


        GUILayout.Space(15f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Physics Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        aircraft.emptyCOI = EditorGUILayout.Vector3Field("Empty Coeff Of Inertia", aircraft.emptyCOI);
        aircraft.emptyCOG = EditorGUILayout.Vector3Field("Empty Center Of Gravity", aircraft.emptyCOG);
        aircraft.maxG = EditorGUILayout.FloatField("Maximum G Load", aircraft.maxG);
        aircraft.maxSpeed = EditorGUILayout.FloatField("Max Speed Kph", aircraft.maxSpeed * 3.6f) / 3.6f;
        Part[] parts = aircraft.GetComponentsInChildren<Part>();
        EditorGUILayout.LabelField("Empty Mass", FlightModel.TotalMass(aircraft.GetComponentsInChildren<Part>(), true) + " kg");
        EditorGUILayout.LabelField("Loaded Mass", FlightModel.TotalMass(aircraft.GetComponentsInChildren<Part>(), false) + " kg");
        GUILayout.Space(15f);
        GUI.color = Color.magenta;
        EditorGUILayout.HelpBox("Autopilot Sensiblity", MessageType.None);
        GUI.color = GUI.backgroundColor;
        aircraft.viewPoint = EditorGUILayout.Vector3Field("External Camera ViewPoint", aircraft.viewPoint);
        aircraft.cruiseSpeed = EditorGUILayout.FloatField("Cruise Speed", aircraft.cruiseSpeed * 3.6f) / 3.6f;
        aircraft.convergeance = EditorGUILayout.FloatField("Gun Convergeance", aircraft.convergeance);
        aircraft.bankTurnAngle = EditorGUILayout.FloatField("Bank Turn Angle", aircraft.bankTurnAngle);
        aircraft.optiAlpha = EditorGUILayout.FloatField("Optimum Alpha", aircraft.optiAlpha);
        aircraft.turnRadius = EditorGUILayout.FloatField("Turn Radius", aircraft.turnRadius);
        aircraft.minCrashDelay = EditorGUILayout.FloatField("Minimum Crash Prevention Delay (sec)", aircraft.minCrashDelay);
        aircraft.minInvertAltitude = EditorGUILayout.FloatField("Minimum Altitude Inverted Flight", aircraft.minInvertAltitude);
        GUILayout.Space(15f);

        aircraft.pidPitch.pidValues = EditorGUILayout.Vector3Field("PID Pitch", aircraft.pidPitch.pidValues);
        aircraft.pidRoll.pidValues = EditorGUILayout.Vector3Field("PID Roll", aircraft.pidRoll.pidValues);
        aircraft.pidElevator.pidValues = EditorGUILayout.Vector3Field("PID Elevator", aircraft.pidElevator.pidValues);


        GUILayout.Space(15f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("References", MessageType.None);
        GUI.color = GUI.backgroundColor;
        aircraft.flaps = EditorGUILayout.ObjectField("Flaps Hydraulics", aircraft.flaps, typeof(HydraulicSystem), true) as HydraulicSystem;
        aircraft.gear = EditorGUILayout.ObjectField("Gear Hydraulics", aircraft.gear, typeof(HydraulicSystem), true) as HydraulicSystem;
        aircraft.airBrakes = EditorGUILayout.ObjectField("Air Brakes Hydraulics", aircraft.airBrakes, typeof(HydraulicSystem), true) as HydraulicSystem;
        aircraft.cannopy = EditorGUILayout.ObjectField("Cannopy Hydraulics", aircraft.cannopy, typeof(HydraulicSystem), true) as HydraulicSystem;
        if (aircraft.crew.Length > 3)
        {
            aircraft.bombBay = EditorGUILayout.ObjectField("Bomb Bay Hydraulics", aircraft.bombBay, typeof(HydraulicSystem), true) as HydraulicSystem;
            aircraft.bombSight = EditorGUILayout.ObjectField("Bombardier Bomb Sight", aircraft.bombSight, typeof(BombardierSeat), true) as BombardierSeat;
            SerializedProperty bombardierPath = serializedObject.FindProperty("bombardierPath");
            EditorGUILayout.PropertyField(bombardierPath, true);
        }

        SerializedProperty hardPoints = serializedObject.FindProperty("hardPoints");
        EditorGUILayout.PropertyField(hardPoints, true);
        SerializedProperty crew = serializedObject.FindProperty("crew");
        EditorGUILayout.PropertyField(crew, true);
        SerializedProperty fuelTanks = serializedObject.FindProperty("fuelTanks");
        EditorGUILayout.PropertyField(fuelTanks, true);
        if (aircraft.crew[0] && aircraft.crew[0].seats.Length > 0 && aircraft.crew[0].seats[0].GetComponent<PilotSeat>() == null)
        {
            EditorGUILayout.HelpBox("First seat must be pilot", MessageType.Warning);
        }


        aircraft.deletePassWord = EditorGUILayout.PasswordField("Delete All Parts", aircraft.deletePassWord);
        if (aircraft.deletePassWord == "i1s2n3i4")
        {
            if (GUILayout.Button("Delete all parts"))
            {
                foreach (DragSurface d in aircraft.GetComponentsInChildren<DragSurface>())
                    DestroyImmediate(d);
                foreach (ObjectElement p in aircraft.GetComponentsInChildren<ObjectElement>())
                    DestroyImmediate(p);
                DestroyImmediate(aircraft.data.rb);
                DestroyImmediate(aircraft.data);
                DestroyImmediate(aircraft);
            }
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(aircraft);
            EditorSceneManager.MarkSceneDirty(aircraft.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
