using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftSpawnSettings : MonoBehaviour
{
    public enum Type { InAir, Landed, Parked }

    [HideInInspector] public Type spawnType;
    [HideInInspector] public float initialSpeed;
    [HideInInspector] public bool spawnImmediately;

    public Game.Team team = Game.Team.Ally;
    public int squadron;
    public int placeInSquad = 0;
    public bool player = false;
    public float difficulty;

    public int aircraftId;
    public string texturePath;
    public string picturePath;
}
