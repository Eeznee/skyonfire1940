using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public enum PurchasableGroup
{
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
    public Faction faction;
    public bool forwardGuns;
    public bool fighter;
    public bool bomber;
    public Formation formation;
    public Sprite icon;

    public float standardEmptyMass;
    public float standardLoadedMass;

    [HideInInspector] public int id;

    public void UpdateAircraft(int listId)
    {
        id = listId;
        if (!sofAircraft) sofAircraft = aircraft.GetComponent<SofAircraft>();

        sofAircraft.card = this;
        sofAircraft.ResetStationsToDefault();
        sofAircraft.SetReferences();

        Mass emptyMass = new Mass(sofAircraft.massComponents.ToArray(), MassCategory.Empty);
        Mass loadedMass = new Mass(sofAircraft.massComponents.ToArray(), MassCategory.Loaded);

        standardEmptyMass = emptyMass.mass;
        standardLoadedMass = loadedMass.mass;
    }

    public bool Available()
    {
        if (!Extensions.IsMobile) return true;

        else
        {
            switch (purchasableGroup)
            {
                case PurchasableGroup.BaseGame: return true;
                case PurchasableGroup.BritishPack: return PlayerPrefs.GetInt("spitfire_mki_cannons", 0) == 1;
                case PurchasableGroup.GermanPack: return PlayerPrefs.GetInt("bf_110_c6", 0) == 1;
                case PurchasableGroup.Workshop: return PlayerPrefs.GetInt("workshop", 0) == 1;
            }
            return false;
        }
    }
}
