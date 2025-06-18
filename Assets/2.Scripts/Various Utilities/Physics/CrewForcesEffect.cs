using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewForcesEffect
{
    const float maxStamina = 15f;
    const float invertMaxStamina = 1f / maxStamina;

    const float maxGSustain = 4.7f;
    const float minGSustain = 2f;

    const float bufferDuration = 2f;
    const float glocTime = 6f;



    private CrewMember crew;
    private float stamina = 15f;
    private float buffer = 0f;
    private bool gloc = false;

    public float StaminaRatio => stamina * invertMaxStamina;
    public bool Gloc => gloc;
    public bool Healthy => stamina == maxStamina && !gloc;

    public CrewForcesEffect(CrewMember _crew)
    {
        crew = _crew;

        accelerationCompensation = Vector3.zero;
        crew.sofModular.OnAttachPlayer += ResetVisualEffects;
    }
    public void UpdateForces(float dt)
    {
        bool player = crew.sofModular == Player.modular;
        float g = crew.data.gForce;

        if (player)
        {
            UpdateStaminaAndGLOC(g, dt);
            UpdatePlayerVisualEffects(g, dt);
        }
        else
        {
            if (Healthy && g < maxGSustain && g > -minGSustain) return;
            UpdateStaminaAndGLOC(g, dt);
        }
    }
    public void UpdateStaminaAndGLOC(float g, float dt)
    {
        float staminaGain;

        if (g < 0f) staminaGain = minGSustain - Mathf.Abs(g);
        else staminaGain = maxGSustain - g;

        stamina += dt * staminaGain;
        if (stamina > maxStamina) stamina = maxStamina;

        if (stamina <= 0f)
        {
            if (buffer >= 1f) gloc = true;
            else buffer = Mathf.MoveTowards(buffer, 1f, dt / bufferDuration);
        }
        else
        {
            if (buffer <= 0f) gloc = false;
            else buffer = Mathf.MoveTowards(buffer, 0f, dt / glocTime);
        }
    }

    private float blood = 0f;
    private float sickness = 0f;
    private bool sick = false;
    private float sicknessFeeling = 0f;
    private float pain = 0f;
    private bool previousG = true;

    const float sicknessTrigger = 4f;
    const float painRecoveryRate = 0.07f;
    const float bloodSustain = 3f;
    const float bloodMax = 3f;
    const float invertBloodMax = 1f / bloodMax;
    const float gSwitchSustain = 0.2f;

    public Vector3 headPositionOffset { get; private set; }
    private Vector3 accelerationCompensation = Vector3.zero;


    public float Blood() { return blood * invertBloodMax; }
    public float Sickness() { return crew.structureDamage <= 0f ? 0f : sicknessFeeling; }
    public float Pain() { return crew.structureDamage <= 0f ? 0f : pain; }

    public void UpdatePlayerVisualEffects(float g, float dt)
    {
        blood += (g - 1f) * dt;
        blood = Mathf.MoveTowards(blood, 0f, bloodSustain * dt);
        blood = Mathf.Clamp(blood, -bloodMax, bloodMax);

        if ((previousG && g < -0f) || (!previousG && g > 2.5f)) { sickness++; previousG = !previousG; }
        sickness = Mathf.MoveTowards(sickness, 0f, gSwitchSustain * dt);
        if (sickness > sicknessTrigger) sick = true;
        if (sick && sickness == 0f) sick = false;
        sicknessFeeling = Mathf.MoveTowards(sicknessFeeling, sick ? 1f : 0f, dt / 5f);

        float targetPain = (1f - StaminaRatio) * Mathf.Clamp01(-Blood() * 1.5f);
        pain = Mathf.MoveTowards(pain, targetPain, (targetPain > pain ? 2f : painRecoveryRate) * dt);


        Vector3 gOffsetLocal = crew.data.acceleration / Physics.gravity.y;
        gOffsetLocal.y *= 0.5f;
        Vector3 accelerationOffset = crew.sofObject.tr.TransformDirection(gOffsetLocal);
        accelerationOffset += (crew.sofObject.tr.up - Vector3.up) * 4f;
        accelerationCompensation = Vector3.MoveTowards(accelerationCompensation, accelerationOffset, dt * 2f);
        accelerationCompensation = Vector3.ClampMagnitude(accelerationCompensation, accelerationOffset.magnitude);
        headPositionOffset = (accelerationOffset - accelerationCompensation) * 0.01f;
    }
    public void ResetVisualEffects()
    {
        blood = 0f;
        sickness = 0f;
        sicknessFeeling = 0f;
        pain = 0f;
    }
}
