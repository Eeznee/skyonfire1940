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

    public ArmamentManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        guns = aircraft.GetComponentsInChildren<Gun>();
        primaries = Gun.FilterByController(GunController.PilotPrimary, guns);
        secondaries = Gun.FilterByController(GunController.PilotSecondary, guns);

        rockets = Station.GetOrdnances<RocketsLoad>(aircraft.Stations);
        bombs = Station.GetOrdnances<BombsLoad>(aircraft.Stations);
        torpedoes = Station.GetOrdnances<TorpedosLoad>(aircraft.Stations);

        aircraft.OnInitialize += OnInitialize;
    }

    void OnInitialize()
    {
        ConvergeGuns(aircraft.Convergence);
    }

    public void CheatPointGuns(Vector3 worldPoint, float cheatFactor)
    {
        foreach (Gun gun in guns)
            if (gun && gun.controller != GunController.Gunner)
            {
                Quaternion pointRotation = Quaternion.LookRotation(worldPoint - gun.tr.position);
                gun.cheatConvergence = Quaternion.Inverse(gun.tr.rotation) * pointRotation;
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
                Vector3 gravityCompensation = -t * t * Physics.gravity.y * 0.5f * tr.up;

                Quaternion pointRotation = Quaternion.LookRotation(point + gravityCompensation - gun.tr.position);
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

    public void FirePrimaries() { foreach (Gun g in primaries) if (g.aircraft == aircraft && (g.gunPreset.name != "MP40" || aircraft.hydraulics.bombBay.state > 0.8f)) g.Trigger(); }
    public void FireSecondaries() { foreach (Gun g in secondaries) if (g.aircraft == aircraft) g.Trigger(); }
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
        OrdnanceLoad.LaunchOptimal(torpedoes, 0f);
    }
}
