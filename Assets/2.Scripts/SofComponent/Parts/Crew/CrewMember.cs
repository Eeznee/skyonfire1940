using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CrewMember : SofModule
{
    //State
    public float audioCockpitRatio = 1f;
    public int currentSeat = 0;

    public CrewSeat[] seats;
    public CrewMember[] crewGroup;
    public Parachute parachute;
    public Parachute specialPlayerParachute;

    public HumanBody humanBody;

    private float bailingCount;
    private bool bailingOut = false;

    const float bailOutTime = 3f;
    const float minBailOutAltitude = 30f;
    const float minCrashTime = 2f;
    public Vector3 headLookAt = Vector3.forward;
    public const float eyeShift = 0.05f;
    public Vector3 EyesPosition() { return transform.position + transform.parent.up * eyeShift; }
    public override float Mass() { return HumanBody.Weight(); }
    public override float EmptyMass() { return 0f; }
    public CrewSeat Seat { get { return seats[currentSeat]; } }
    public string Action() { return seats[currentSeat].Action(); }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        humanBody = new HumanBody(this);
        seats = seats.RemoveNulls();
        crewGroup = complex.crew;
        SwitchSeat(currentSeat);
    }
    private void Update()
    {
        //Conscious or dead conditions
        if (ripped || Time.timeScale == 0f || !sofObject) return;
        humanBody.ApplyForces(data.gForce, Time.deltaTime);
        if ((Player.crew == this && GameManager.gm.vr) || humanBody.Gloc()) return;

        //Bailout sequence
        bool bailing = bailingOut;
        bailing &= data.relativeAltitude.Get + data.vsp.Get * minCrashTime > minBailOutAltitude;
        if (Seat.canopy) bailing &= Seat.canopy.state > 0.5f || Seat.canopy.disabled;
        if (bailing)
        {
            bailingCount -= Time.deltaTime;
            if (bailingCount < 0f) Bailout();
            if (Player.crew == this) Log.Print("Bailout in " + bailingCount.ToString("0.0") + " s", "bailout");
        }
        //Actions in delta time
        if (Player.crew == this) seats[currentSeat].PlayerUpdate(this);
        else
        {
            //Automatic bail out
            if (aircraft && (aircraft.burning || aircraft.destroyed) && Player.aircraft != aircraft) StartBailout(Random.Range(1f, 3f));
            //Pick the seat with highest priority
            currentSeat = 0;
            for (int i = 1; i < seats.Length; i++) if (seats[i].Priority() > seats[currentSeat].Priority()) currentSeat = i;
            seats[currentSeat].AiUpdate(this);
        }
    }
    private void FixedUpdate()
    {
        //Actions in fixed delta time
        if ((Player.crew == this && GameManager.gm.vr) || humanBody.Gloc() || ripped || Time.timeScale == 0f || !sofObject) return;
        if (Player.crew == this) seats[currentSeat].PlayerFixed(this); else seats[currentSeat].AiFixed(this);
    }
    public void SwitchSeat(int seat)
    {
        if (seat == currentSeat) return;
        seats[currentSeat].ResetSeat();
        currentSeat = seat;
        seats[seat].ResetSeat();
    }
    public override void Rip()
    {
        if (aircraft && this == aircraft.crew[0]) { aircraft.hasPilot = false; aircraft.destroyed = true; }
        base.Rip();
    }
    public void StartBailout(float delay)
    {
        if (!aircraft || ripped || bailingOut) return;
        bailingOut = true;
        if (Seat.canopy) Seat.canopy.Set(1f);
        bailingCount = bailOutTime + delay;
    }
    public void CancelBailout()
    {
        if (!aircraft || ripped) return;
        bailingOut = false;
        if (Seat.canopy) Seat.canopy.Set(0f);
    }
    public void Bailout()
    {
        if (!aircraft || ripped) return;
        bailingOut = false;
        if (this == aircraft.crew[0]) { aircraft.hasPilot = false; aircraft.destroyed = true; }
        Parachute para = specialPlayerParachute && Player.crew == this ? specialPlayerParachute : parachute;
        Instantiate(para, transform.position, transform.rotation).TriggerParachute(aircraft, this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CrewMember))]
public class CrewMemberEditor : Editor
{
    static bool showMain = true;
    SerializedProperty material;
    SerializedProperty currentSeat;
    SerializedProperty seats;

    static bool showParachutes = true;
    SerializedProperty parachute;
    SerializedProperty specialPlayerParachute;

    protected virtual void OnEnable()
    {
        material = serializedObject.FindProperty("material");
        currentSeat = serializedObject.FindProperty("currentSeat");
        seats = serializedObject.FindProperty("seats");

        parachute = serializedObject.FindProperty("parachute");
        specialPlayerParachute = serializedObject.FindProperty("specialPlayerParachute");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CrewMember crew = (CrewMember)target;


        GUILayout.Space(15f);
        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(material);

            EditorGUILayout.PropertyField(currentSeat);
            EditorGUILayout.PropertyField(seats);

            EditorGUI.indentLevel--;
        }
        GUILayout.Space(15f);
        showParachutes = EditorGUILayout.Foldout(showParachutes, "Parachute", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(parachute);
            EditorGUILayout.PropertyField(specialPlayerParachute);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
