using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airfield : MonoBehaviour
{
    public string airfieldName;
    public int id;
    public Transform[] spawnLocations;
    int spawned = 0;

    public Transform GetNextSpawn()
    {
        Transform tr = spawnLocations[spawned];
        spawned ++;
        return tr;
    }
}
