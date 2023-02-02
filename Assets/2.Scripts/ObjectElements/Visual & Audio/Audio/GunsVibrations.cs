using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsVibrations : AudioVisual
{
    private Gun[] guns;
    private float[] gunsPower;
    const float factor = 0.04f;
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            guns = sofObject.GetComponentsInChildren<Gun>();
            gunsPower = new float[guns.Length];
            for (int i = 0; i < guns.Length; i++)
            {   
                gunsPower[i] = guns[i].gunPreset.FireRate * guns[i].gunPreset.ammunition.mass;
                gunsPower[i] *= factor;
                if (guns[i].gunPreset.singleShotsAudio) guns[i].OnFireEvent += SingleShotVibrations;
            }
        }
    }
    private void SingleShotVibrations()
    {
        VibrationsManager.SendVibrations(1f, 0.25f, aircraft);
    }
    private void Update()
    {
        if (sofObject == PlayerManager.player.sofObj)
        {
            float totalVibrations = 0f;
            for (int i = 0; i < guns.Length; i++)
            {
                Gun gun = guns[i];
                if (gun && !gun.gunPreset.singleShotsAudio && gun.Firing()) 
                {
                    
                    float sqrDistance = (PlayerCamera.camPos - gun.tr.position).magnitude;
                    totalVibrations += gunsPower[i] / Mathf.Max(sqrDistance, 1f);
                }
            }
            totalVibrations = Mathf.Log(totalVibrations + 1f, 10f);
            VibrationsManager.SendVibrations(totalVibrations, 0.15f, aircraft);
        }
    }
}