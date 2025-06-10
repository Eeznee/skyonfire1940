using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsVibrations : AudioComponent
{
    private Gun[] guns;
    private float[] gunsPower;
    const float factor = 0.04f;
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        guns = sofObject.GetComponentsInChildren<Gun>();
        gunsPower = new float[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            gunsPower[i] = guns[i].gunPreset.FireRate * guns[i].gunPreset.ammunition.mass;
            gunsPower[i] *= factor;
            if (guns[i].gunPreset.singleShotsAudio) guns[i].OnFireEvent += SingleShotVibrations;
        }
    }
    private void SingleShotVibrations(float delay)
    {
        VibrationsManager.SendVibrations(1f, 0.25f, aircraft);
    }
    private void Update()
    {
        if (sofObject == Player.sofObj)
        {
            float totalVibrations = 0f;
            for (int i = 0; i < guns.Length; i++)
            {
                Gun gun = guns[i];
                if (gun && !gun.gunPreset.singleShotsAudio && gun.Firing())
                {

                    float sqrDistance = (SofCamera.tr.position - gun.tr.position).magnitude;
                    totalVibrations += gunsPower[i] / Mathf.Max(sqrDistance, 1f);
                }
            }
            totalVibrations = Mathf.Log(totalVibrations + 1f, 10f);
            VibrationsManager.SendVibrations(totalVibrations, 0.15f, aircraft);
        }
    }
}