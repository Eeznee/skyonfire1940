using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmamentManager
{
    private SofAircraft aircraft;

    public OrdnanceLoad[] bombs;
    public OrdnanceLoad[] rockets;
    public OrdnanceLoad[] torpedoes;

    public Gun[] primaries;
    public Gun[] secondaries;
    public Gun[] guns;

    public MagazineStorage[] magazineStorages;

    public ArmamentManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        guns = aircraft.GetComponentsInChildren<Gun>();
        primaries = Gun.FilterByController(GunController.PilotPrimary, guns);
        secondaries = Gun.FilterByController(GunController.PilotSecondary, guns);

        magazineStorages = aircraft.GetComponentsInChildren<MagazineStorage>();

        rockets = aircraft.GetComponentsInChildren<RocketsLoad>(); //Station.GetOrdnances<RocketsLoad>(aircraft.Stations);
        bombs = aircraft.GetComponentsInChildren<BombsLoad>();
        torpedoes = aircraft.GetComponentsInChildren<TorpedosLoad>();

        aircraft.OnInitialize += OnInitialize;
    }

    void OnInitialize()
    {
        ConvergeGuns(aircraft.Convergence);
    }

    public void CheatPointGuns(Vector3 worldPoint)
    {
        Quaternion rotation = Quaternion.LookRotation(worldPoint - aircraft.transform.position);
        rotation = Quaternion.Inverse(aircraft.transform.rotation) * rotation;

        foreach (Gun gun in guns)
            if (gun && gun.controller != GunController.Gunner)
            {
                //Quaternion pointRotation = Quaternion.LookRotation(worldPoint - gun.tr.position);
                gun.cheatConvergence = rotation;
                gun.cheatTime = Time.time + 1f;
            }
    }
    public void ConvergeGuns(float convergence)
    {
        Transform tr = aircraft.transform;

        Vector3 point = aircraft.crew[0].Seat.defaultPOV.position + tr.forward * convergence;
        float distance = (point - tr.position).magnitude;

        List<Gun> guns = new List<Gun>(primaries);
        guns.AddRange(secondaries);

        foreach (Gun gun in guns)
            if (gun && !gun.noConvergeance && gun.controller != GunController.Gunner)
            {
                float t = 1.1f * distance / gun.gunPreset.ammunition.defaultMuzzleVel;
                Vector3 gravityCompensation = t * t * -Physics.gravity.y * 0.5f * tr.up;
                Vector3 aimPoint = point + gravityCompensation;
                Quaternion pointRotation = Quaternion.LookRotation(aimPoint - gun.tr.position, tr.up);
                gun.convergence = Quaternion.Inverse(gun.tr.rotation) * pointRotation;
            }   
    }
    /*
    public void PointGuns(Vector3 position, float factor)
    {
        if (convergedDefault && factor == 0f) return;
        convergedDefault = factor == 0f;

        Vector3 defaultConvergence = crew[0].Seat.zoomedPOV.position + transform.forward * aircraft.convergeance;

        position = Vector3.Lerp(defaultConvergence, position, factor);
        float distance = (position - transform.position).magnitude;

        gunsPointer.LookAt(position, transform.up);

        List<Gun> guns = new List<Gun>(primaries);
        guns.AddRange(secondaries);

        foreach (Gun gun in guns)
            if (gun && !gun.noConvergeance && gun.controller != GunController.Gunner)
            {
                float t = 1.1f * distance / gun.gunPreset.ammunition.defaultMuzzleVel;
                Vector3 gravityCompensation = -t * t * Physics.gravity.y * 0.5f * tr.up;
                Quaternion pointRotation = Quaternion.LookRotation(position + gravityCompensation - gun.tr.position);
                gun.convergence = Quaternion.Inverse(gun.tr.rotation) * pointRotation;
            }
    }
    */

    public void FireGuns(bool firePrimaries, bool fireSecondaries, float triggerTime)
    {
        if (firePrimaries)
        {
            foreach (Gun g in primaries) if (g.aircraft == aircraft && (g.gunPreset.name != "MP40" || aircraft.hydraulics.bombBay.state > 0.8f)) g.Trigger(triggerTime);
        }
        if (fireSecondaries)
        {
            foreach (Gun g in secondaries) if (g.aircraft == aircraft) g.Trigger(triggerTime);
        }
    }
    
    public void DropBomb()
    {
        OrdnanceLoad.LaunchOptimal(bombs, 5f);
        DropTorpedo();
    }
    public void FireRocket()
    {
        OrdnanceLoad.LaunchOptimal(rockets, 0f);
    }
    public void DropTorpedo()
    {
        if (aircraft.data.grounded.Get) return;
        OrdnanceLoad.LaunchOptimal(torpedoes, 0f);
    }

    public float OrdnanceMass(OrdnanceLoad[] loads)
    {
        float total = 0f;
        foreach (OrdnanceLoad load in loads)
        {
            total += load.RealMass - load.EmptyMass;
        }
        return total;
    }
    public float TotalAmmoMass
    {
        get
        {
            float total = 0f;

            foreach (Gun gun in guns)
            {
                total += gun.RealMassIncludingMagazine - gun.EmptyMass;
            }
            foreach(MagazineStorage magStorage in magazineStorages)
            {
                total += magStorage.RealMass - magStorage.EmptyMass;
            }

            return total;
        }
    }

    public float TotalOrdnanceMass => OrdnanceMass(bombs) + OrdnanceMass(rockets) + OrdnanceMass(torpedoes);
    public float TotalExpendablesMunitionsMass => TotalAmmoMass + TotalOrdnanceMass;
}
