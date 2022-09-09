using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    public enum Type { InAir,Landed,Parked }

    [HideInInspector] public Type spawnType;
    [HideInInspector] public float initialSpeed;
    [HideInInspector] public bool spawnImmediately;
    public GameObject plane;
    public float difficulty;
    public bool player = false;
    public int squadron;
    public int placeInSquad = 0;
    public Game.Team team = Game.Team.Ally;

    SofAircraft sofAircraft;

    void Start()
    {
        if (spawnImmediately) Spawn();
    }

    public SofAircraft Spawn()
    {
        if (spawnType != Type.InAir)
        {
            Vector3 pos = transform.position;
            pos.y = GameManager.map.HeightAtPoint(pos);
            pos += plane.transform.localPosition;
            plane = Instantiate(plane, pos, transform.rotation * plane.transform.localRotation);
        }
        else
            plane = Instantiate(plane, transform.position, transform.rotation);

        plane.GetComponent<Rigidbody>().velocity = plane.transform.forward * initialSpeed / 3.6f;
        sofAircraft = plane.GetComponent<SofAircraft>();
        sofAircraft.SpawnInitialization(spawnType,team,squadron,placeInSquad,difficulty);

        if (player)
            GameManager.SetPlayer(sofAircraft,true);

        return sofAircraft;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Spawner spawn = (Spawner)target;

        spawn.spawnType = (Spawner.Type)EditorGUILayout.EnumPopup("Spawning Type", spawn.spawnType);
        spawn.initialSpeed = (spawn.spawnType == Spawner.Type.InAir) ? EditorGUILayout.FloatField("Initial Speed In Km/h", spawn.initialSpeed) : 0f;
        spawn.spawnImmediately = EditorGUILayout.Toggle("Spawn Immediately",spawn.spawnImmediately);
        if (spawn.spawnImmediately) base.OnInspectorGUI();
    }
}
#endif
