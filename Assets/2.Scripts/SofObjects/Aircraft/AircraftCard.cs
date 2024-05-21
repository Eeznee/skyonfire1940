using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public enum PurchasableGroup{
    BaseGame,
    BritishPack,
    GermanPack,
    Workshop
}

[CreateAssetMenu(fileName = "New Aircraft Card", menuName = "SOF/Game Data/Aircraft Card")]
public class AircraftCard : ScriptableObject
{
    public string completeName = "Aircraft Mk.I";
    public string fileName = "aircraft_mki";
    public PurchasableGroup purchasableGroup = PurchasableGroup.BaseGame;
    public GameObject aircraft;
    public SofAircraft sofAircraft;
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
#if !MOBILE_INPUT
        return true;
#else
        switch (purchasableGroup)
        {
            case PurchasableGroup.BaseGame: return true;
            case PurchasableGroup.BritishPack: return PlayerPrefs.GetInt("spitfire_mki_cannons", 0) == 1;
            case PurchasableGroup.GermanPack: return PlayerPrefs.GetInt("bf_110_c6", 0) == 1;
            case PurchasableGroup.Workshop: return PlayerPrefs.GetInt("workshop", 0) == 1;
        }
        return false;
#endif
    }
    [HideInInspector]public int id;
}
