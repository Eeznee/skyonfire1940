using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airfield : MonoBehaviour
{
    public string airfieldName;
    public int id;
    public Spawner[] spawnLocations;
    int spawned = 0;

    public SofAircraft[] Spawn(Game.Squadron squad)
    {
        SofAircraft[] aircrafts = new SofAircraft[Mathf.Min(squad.amount, spawnLocations.Length)];
        for (int i = 0; i < squad.amount; i++)
        {
            if (spawned >= spawnLocations.Length) break;
            Spawner spawner = spawnLocations[spawned];

            spawner.plane = squad.aircraftCard.aircraft;
            spawner.player = squad.includePlayer && i == 0;
            spawner.team = squad.team;
            spawner.placeInSquad = i;
            spawner.squadron = squad.id;
            spawner.difficulty = squad.difficulty;
            aircrafts[i] = spawner.Spawn();
            spawned++;
        }
        return aircrafts;
    }
}
