using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Aircraft Card", menuName = "Aircraft/Aircraft Card")]
public class AircraftCard : ScriptableObject
{
    public string completeName = "Aircraft Mk.I";
    public string fileName = "aircraft_mki";
    public bool baseGameIncluded = true;
    public bool workshopIncluded = false;
    public GameObject aircraft;
    public GameObject fixedModel;
    public Game.Team team = Game.Team.Ally;
    public bool bomb;
    public bool bombBay;
    public bool forwardGuns;
    public bool gunner;
    public bool airbrakes;
    public bool fighter;
    public bool bomber;
    public Formation formation;
    public float startingSpeed = 350;
    public Sprite icon;

    public bool Available()
    {
        if (baseGameIncluded) return true;

        bool purchased = PlayerPrefs.GetInt(fileName, 0) == 1;
        if (purchased) return true;

        bool workshop = PlayerPrefs.GetInt("workshop", 0) == 1 && workshopIncluded;
        if (workshop) return true;

#if !MOBILE_INPUT
        return true;
#else
        return false;
#endif
    }

    [HideInInspector]public int id;
}
