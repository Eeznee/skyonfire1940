
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
    public Game.Squadron squadron;
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
    public Station[] stations;
    public OrdnanceLoad[] bombs;
    public OrdnanceLoad[] rockets;
    public OrdnanceLoad[] torpedoes;

    public Stabilizer hStab;
    public Stabilizer vStab;
    public MaterialsList materials;

    //Physics
    public bool useAutoMass = true;
    public float cogForwardDistance = 0f;
    public float targetEmptyMass = 3000f;
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
    public bool verticalConvergence = true;
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

    public override void Initialize()
    {
        for (int i = 0; i < stations.Length; i++) stations[i].ChooseOption(squadron.stations[i]);

        gunsPointer = new GameObject("Guns Pointer").transform;
        gunsPointer.parent = transform;
        gunsPointer.SetPositionAndRotation(transform.position, transform.rotation);
        wheels = GetComponentsInChildren<WheelCollider>();
        engines = GetComponentsInChildren<Engine>();
        crew = GetComponentsInChildren<CrewMember>();
        for(int i = 0; i < crew.Length; i++)
            for (int j = 0; j < crew[i].seats.Length; j++)
                if (crew[i].seats[j].GetComponent<BombardierSeat>()) bombardierPath = new SeatPath(i, j);
        bombSight = GetComponentInChildren<BombardierSeat>();
        rockets = Station.GetOrdnances<RocketsLoad>(stations);
        bombs = Station.GetOrdnances<BombLoad>(stations);

        Gun[] guns = GetComponentsInChildren<Gun>();
        primaries = Gun.FilterByController(GunController.PilotPrimary, guns);
        secondaries = Gun.FilterByController(GunController.PilotSecondary, guns);
        Stabilizer[] stabs = GetComponentsInChildren<Stabilizer>();
        foreach (Stabilizer stab in stabs)
        {
            if (stab.rudder) vStab = stab;
            else hStab = stab;
        }


        materials.ApplyMaterials(this);
        base.Initialize();

        squadronId = squadron.id;
        difficulty = squadron.difficulty;

        tag = (squadron.team == Game.Team.Ally) ? "Ally" : (squadron.team == Game.Team.Axis) ? "Axis" : "Neutral";
        if (squadron.team == Game.Team.Ally) GameManager.allyAircrafts.Add(this);
        else GameManager.axisAircrafts.Add(this);
        GameManager.ui.CreateMarker(this);

        bool grounded = squadron.airfield >= 0;
        throttle = grounded ? 0f : 1f;
        if (gear) gear.SetInstant(grounded);
        if (!grounded) data.rb.velocity = transform.forward * card.startingSpeed / 3.6f;
        SetEngines(!grounded, true);

        if (squadron.textureName != "") TextureTool.ChangeAircraftTexture(this, squadron.textureName);
    }
    private void OnCollisionEnter(Collision col)
    {
        Transform tr = col.GetContact(0).thisCollider.transform;
        tr = tr.name.Contains("Skin") ? tr.parent : tr;
        AirframeBase collided = tr.GetComponent<AirframeBase>();
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
        float correctionTarget = data.relativeAltitude.Get < 3f && data.gsp.Get < cruiseSpeed * 0.7f ? 0f : 1f;
        correction = Mathf.MoveTowards(correction, correctionTarget, t * 0.1f);
        if (data.gsp.Get < 3f && data.relativeAltitude.Get < 5f) correction = 0f;

        float inAirError = target - data.angleOfAttack.Get / optiAlpha;
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
            if (PlayerManager.player.aircraft == this) Log.Print((engines.Length == 1 ? "Engine " : "Engines ") + (on ? "On" : "Off"), "engines");
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
            if (PlayerManager.player.aircraft == this && input != 0)
            {
                float state = flaps.binary ? flaps.stateInput * 100f : flaps.state * 100f;
                string txt = "Flaps : " + state.ToString("0") + " %";
                if (flaps.disabled) txt = "Flaps Unoperational";
                Log.Print(txt, "flaps");
            }
        }
    }
    public void SetGear()
    {
        if (gear && hasPilot && data.relativeAltitude.Get > 4f)
        {
            gear.Set();
            if (PlayerManager.player.aircraft == this)
            {
                string txt = "Landing Gear " + (gear.stateInput == 1f ? "Deploying" : "Retracting");
                if (gear.disabled) txt = "Landing Gear Damaged";
                Log.Print(txt, "gear");
            }
        }
    }
    public void SetAirBrakes()
    {
        if (airBrakes && hasPilot)
        {
            airBrakes.Set();
            if (PlayerManager.player.aircraft == this)
            {
                string txt = "Airbrakes " + (airBrakes.stateInput == 1f ? "Deploying" : "Retracting");
                if (airBrakes.disabled) txt = "Air Brakes Unavailable";
                Log.Print(txt, "airbrakes");
            }
        }
    }
    public void SetBombBay()
    {
        if (bombBay && hasPilot)
        {
            bombBay.Set();
            if (PlayerManager.player.aircraft == this) Log.Print("Bomb Bay " + (bombBay.stateInput == 1f ? "Opening" : "Closing"), "bombbay");
        }
    }
    public void SetCannopy()
    {
        if (cannopy && hasPilot)
        {
            cannopy.Set();
            if (PlayerManager.player.aircraft == this)
            {
                string txt = "Canopy " + (cannopy.stateInput == 1f ? "Opening" : "Closing");
                if (cannopy.disabled) txt = "Canopy Is Missing";
                Log.Print(txt, "canopy");
            }
        }
    }
    public void FirePrimaries() { if (hasPilot) foreach (Gun g in primaries) if (g.data == data && (g.gunPreset.name != "MP40" || bombBay.state > 0.8f)) g.Trigger(); }
    public void FireSecondaries() { if (hasPilot) foreach (Gun g in secondaries) if (g.data == data) g.Trigger(); }
    public void ToggleGunners() { gunnersFire = !gunnersFire; }
    public void PointGuns()
    {
        Vector3 point = crew[0].Seat().zoomedPOV.position + transform.forward * convergeance;
        gunsPointer.LookAt(point, transform.up);

        foreach (Gun pg in secondaries)
            if (pg && !pg.noConvergeance)
            {
                Transform tr = pg.transform;
                tr.localRotation = Quaternion.identity;
                Vector3 direction = point - tr.position;
                if (verticalConvergence) {
                    float t = convergeance / pg.gunPreset.ammunition.defaultMuzzleVel;
                    direction -= t * t * Physics.gravity.y / 2f * tr.up;
                }
                tr.forward = Vector3.RotateTowards(tr.forward, direction ,Mathf.Deg2Rad * 180f, 0f);
            }

        foreach (Gun pg in primaries)
            if (pg && !pg.noConvergeance)
            {
                Transform tr = pg.transform;
                tr.localRotation = Quaternion.identity;
                Vector3 direction = point - tr.position;
                if (verticalConvergence)
                {
                    float t = convergeance / pg.gunPreset.ammunition.defaultMuzzleVel;
                    direction -= t * t * Physics.gravity.y / 2f * tr.up;
                }
                tr.forward = Vector3.RotateTowards(tr.forward, direction, Mathf.Deg2Rad * 180f, 0f);
            }
    }
    public void PointGuns(Vector3 position,float factor)
    {
        Vector3 defaultConvergence = transform.position + transform.forward * convergeance;

        position = Vector3.Lerp(defaultConvergence, position, factor);
        gunsPointer.LookAt(position, transform.up);
        foreach (Gun pg in primaries)
            if (pg && !pg.noConvergeance)
            {
                Transform t = pg.transform;
                t.localRotation = Quaternion.identity;
                t.forward = Vector3.RotateTowards(t.forward, position - t.position, Mathf.Deg2Rad * 10f, 0f);
            }

        foreach (Gun sg in secondaries)
            if (sg && !sg.noConvergeance)
            {
                Transform t = sg.transform;
                t.localRotation = Quaternion.identity;
                t.forward = Vector3.RotateTowards(t.forward, position - t.position, Mathf.Deg2Rad * 10f, 0f);
            }
    }

    public void DropBomb()
    {
        OrdnanceLoad.LaunchOptimal(bombs, 5f);
    }
    public void FireRocket()
    {
        OrdnanceLoad.LaunchOptimal(rockets, 0f);
    }
    public void DropTorpedo()
    {
        OrdnanceLoad.LaunchOptimal(torpedoes, 0f);
    }
    public bool CanPairUp()
    {
        if (card.forwardGuns || placeInSquad % 2 == 0) return false ;
        if (GameManager.squadrons[squadronId][placeInSquad - 1].destroyed) return false;
        return true;
    }
    public void OnDrawGizmos()
    {
        Mass emptyMass = new Mass(GetComponentsInChildren<Part>(), true);
        Vector3 cog = transform.TransformPoint(emptyMass.center);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cog - transform.right, cog + transform.right);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cog - transform.up, cog + transform.up);
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
        aircraft.materials = EditorGUILayout.ObjectField("Materials", aircraft.materials, typeof(MaterialsList), true) as MaterialsList;
        //if (aircraft.materials && GUILayout.Button("Apply materials"))
        //aircraft.materials.ApplyMaterials(aircraft);
        aircraft.controlSpeed = EditorGUILayout.Vector3Field("Controls Speed (P/Y/R)", aircraft.controlSpeed);
        GUILayout.Space(7f);

        aircraft.maxG = EditorGUILayout.FloatField("Maximum G Load", aircraft.maxG);
        aircraft.maxSpeed = EditorGUILayout.FloatField("Max Speed Kph", aircraft.maxSpeed * 3.6f) / 3.6f;

        GUILayout.Space(15f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Physics Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;

        Part[] parts = aircraft.GetComponentsInChildren<Part>();
        Mass emptyMass = new Mass(parts, true);
        Mass loadedMass = new Mass(parts, false);

        EditorGUILayout.LabelField("Empty Mass", emptyMass.mass.ToString("0.0") + " kg");
        EditorGUILayout.LabelField("Loaded Mass", loadedMass.mass.ToString("0.0") + " kg");
        EditorGUILayout.LabelField("Empty COG", emptyMass.center.ToString("F2"));

        aircraft.useAutoMass = EditorGUILayout.Toggle("Use Auto Mass", aircraft.useAutoMass);
        if (aircraft.useAutoMass)
        {
            aircraft.targetEmptyMass = EditorGUILayout.FloatField("Target Empty Mass", aircraft.targetEmptyMass);
            aircraft.cogForwardDistance = EditorGUILayout.FloatField("Target COG Z-Pos", aircraft.cogForwardDistance);
            if (GUILayout.Button("Target AutoMass"))
                Mass.ComputeAutoMass(aircraft, new Mass(aircraft.targetEmptyMass, Vector3.forward * aircraft.cogForwardDistance));
        }

        GUILayout.Space(15f);

        GUI.color = Color.magenta;
        EditorGUILayout.HelpBox("Autopilot Sensiblity", MessageType.None);
        GUI.color = GUI.backgroundColor;
        aircraft.viewPoint = EditorGUILayout.Vector3Field("External Camera ViewPoint", aircraft.viewPoint);
        aircraft.cruiseSpeed = EditorGUILayout.FloatField("Cruise Speed", aircraft.cruiseSpeed * 3.6f) / 3.6f;
        aircraft.convergeance = EditorGUILayout.FloatField("Gun Convergence", aircraft.convergeance);
        aircraft.verticalConvergence = EditorGUILayout.Toggle("Vertical Convergence", aircraft.verticalConvergence);
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
        aircraft.bombBay = EditorGUILayout.ObjectField("Bomb Bay Hydraulics", aircraft.bombBay, typeof(HydraulicSystem), true) as HydraulicSystem;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("stations"), true);
        foreach (Station s in aircraft.stations) if (aircraft.stations != null && s != null) s.UpdateOptions();
        SerializedProperty fuelTanks = serializedObject.FindProperty("fuelTanks");
        EditorGUILayout.PropertyField(fuelTanks, true);
        if (aircraft.crew[0] && aircraft.crew[0].seats.Length > 0 && aircraft.crew[0].seats[0].GetComponent<PilotSeat>() == null)
        {
            EditorGUILayout.HelpBox("First seat must be pilot", MessageType.Warning);
        }


        aircraft.deletePassWord = EditorGUILayout.PasswordField("Delete All Parts", aircraft.deletePassWord);
        if (aircraft.deletePassWord == "isni")
        {
            if (GUILayout.Button("Delete all parts"))
            {
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
