using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.OnScreen;

public class LeadCrosshair : MonoBehaviour
{
    Image image;
    SofAircraft target;
    const float maxRangeAircraft = 500f;
    const float maxRangeAA = 1000f;
    private void Start()
    {
        image = GetComponent<Image>();
    }
    void LateUpdate()
    {
        bool enabled = true;
        enabled &= Player.modular && Player.crew;
        enabled &= !Player.crew.ripped && !Player.crew.ActionsUnavailable;
        enabled &= !TimeManager.paused;
        enabled &= Player.role == SeatRole.Pilot || Player.role == SeatRole.Gunner;
        enabled &= SofSettingsSO.CurrentSettings.leadIndicator;

        if (enabled)
        {
            if(Time.frameCount % 10 == 0) target = Target();
            List<Gun> operationalGuns = GunsToUseForLeading();

            enabled &= operationalGuns.Count > 0 && target;

            if (enabled)
                transform.position = GetLeadScreenPoint(operationalGuns, target);
        }

        image.enabled = enabled;
    }

    public Vector3 GetLeadScreenPoint(List<Gun> guns, SofAircraft target)
    {
        AmmunitionPreset ammo = guns[0].gunPreset.ammunition;
        foreach (Gun gun in guns) if (gun.gunPreset.ammunition.caliber > ammo.caliber) ammo = gun.gunPreset.ammunition;

        Vector3 lead = Ballistics.PerfectLead(Player.sofObj, Player.sofObj.tr.position, target, ammo.defaultMuzzleVel, ammo.DragCoeff);
        Vector3 perfectLeadPoint = target.tr.position + lead;

        Vector3 averageMuzzlePos = Vector3.zero;
        Vector3 averageMuzzleDir = Vector3.zero;

        foreach (Gun gun in guns)
        {
            averageMuzzlePos += gun.WorldMuzzlePos / guns.Count;
            averageMuzzleDir += gun.ConvergedRotation * Vector3.forward;
        }

        Quaternion muzzleRotation = Quaternion.LookRotation(averageMuzzleDir);
        Quaternion perfectLeadRotation = Quaternion.LookRotation(perfectLeadPoint - averageMuzzlePos);

        Quaternion adjustRotation = perfectLeadRotation * Quaternion.Inverse(muzzleRotation);



        Vector3 leadIndicatorPosition = SofCamera.tr.position + adjustRotation * (Player.seat.CrosshairPosition - SofCamera.tr.position);

        Vector3 screenPos = SofCamera.cam.WorldToScreenPoint(leadIndicatorPosition);
        screenPos.z = 0f;
        return screenPos;
    }

    private List<Gun> GunsToUseForLeading()
    {
        List<Gun> guns = new List<Gun>();
        float largestCaliber = 0f;

        if (Player.role == SeatRole.Pilot)
        {
            foreach (Gun gunToCheck in Player.aircraft.armament.primaries)
            {
                if (Extensions.IsMobile && !OnScreenConditionalTrigger.PrimariesEnabled) break; 

                if (gunToCheck && !gunToCheck.MustBeReloaded)
                {
                    float caliber = gunToCheck.gunPreset.ammunition.caliber;
                    if (caliber > largestCaliber)
                    {
                        largestCaliber = caliber;
                        guns.Clear();
                    }
                    if(caliber == largestCaliber)
                    {
                        guns.Add(gunToCheck);
                    }
                }
            }
            foreach (Gun gunToCheck in Player.aircraft.armament.secondaries)
            {
                if (Extensions.IsMobile && !OnScreenConditionalTrigger.SecondariesEnabled) break;

                if (gunToCheck && !gunToCheck.MustBeReloaded)
                {
                    float caliber = gunToCheck.gunPreset.ammunition.caliber;
                    if (caliber > largestCaliber)
                    {
                        largestCaliber = caliber;
                        guns.Clear();
                    }
                    if (caliber == largestCaliber)
                    {
                        guns.Add(gunToCheck);
                    }
                }
            }
        }
        else if (Player.role == SeatRole.Gunner)
        {
            foreach (Gun gunToCheck in Player.gunnerSeat.gunMount.GetComponentsInChildren<Gun>())
            {
                if (gunToCheck && !gunToCheck.MustBeReloaded) guns.Add(gunToCheck);
            }
        }
        return guns;
    }
    private SofAircraft Target()
    {
        SofObject player = Player.sofObj;
        Vector3 camDir = SofCamera.directionInput.normalized;

        float maxRange = Player.aircraft ? maxRangeAircraft : maxRangeAA;
        float highestDotProduct = 0f;
        SofAircraft targetSelected = null;

        foreach (SofAircraft[] squadron in GameManager.squadrons)
        {
            foreach (SofAircraft aircraft in squadron)
            {
                if (!aircraft) continue;
                if (aircraft.tag == player.tag) continue;

                float distance = (aircraft.tr.position - player.tr.position).magnitude;
                Vector3 approxLead = aircraft.tr.position;
                approxLead += aircraft.rb.linearVelocity * distance / 500f;
                float dot = Vector3.Dot(camDir, (approxLead - SofCamera.tr.position).normalized);


                if (distance < maxRange && dot > highestDotProduct)
                {
                    targetSelected = aircraft;
                    highestDotProduct = dot;
                }
            }
        }

        return targetSelected;
    }
}
