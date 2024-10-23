using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CrewMember))]
[AddComponentMenu("Sof Components/Crew Seats/Crew Forces Effect")]
public class CrewForcesEffect : MonoBehaviour
{
    const float maxStamina = 15f;
    const float sustain = 4.7f;
    const float staminaRegenFactor = 2f;
    const float invertNegSustain = -1f/2.3f;
    const float bloodSustain = 3f;
    const float bloodMax = 3f;

    const float bufferMax = 2f;
    const float invertGlocTime = 1f/3f;
    const float gSwitchSustain = 0.2f;
    const float sicknessTrigger = 4f;
    const float painRecoveryRate = 0.07f;

    private float invertMaxStamina;
    private float invertBloodMax;

    private float blood = 0f;
    private float stamina = 15f;
    private float buffer = 0f;
    private bool gloc = false;
    private float sickness = 0f;
    private bool sick = false;
    private float sicknessFeeling = 0f;
    private bool previousG = true;
    private float pain = 0f;
    private CrewMember crew;

    public static float Weight() { return 70f; }
    public float Stamina() { return stamina* invertMaxStamina; }
    public float Blood() { return blood* invertBloodMax; }
    public bool Gloc() { return gloc; }
    public float Sickness(){ return crew.structureDamage <= 0f ? 0f : sicknessFeeling; }
    public float Pain() { return crew.structureDamage <= 0f ? 0f : pain; }


    private void Awake()
    {
        crew = GetComponent<CrewMember>();
        invertMaxStamina = 1f / maxStamina;
        invertBloodMax = 1f / bloodMax;

        accelerationCompensation = Vector3.zero;
    }
    private void Update()
    {
        ApplyForces(crew.data.gForce, Time.deltaTime);
    }
    public void ApplyForces(float g, float dt)
    {
        float staminaGain = sustain - (g > 0f ? g : g * sustain * invertNegSustain);
        if (staminaGain > 0f) staminaGain *= staminaRegenFactor;
        stamina = Mathf.Min(stamina + dt * staminaGain,maxStamina);
        if (stamina <= 0f)
        {
            buffer = Mathf.MoveTowards(buffer, bufferMax, dt);
            if (buffer == bufferMax) gloc = true;
        } 
        else
        {
            buffer = Mathf.MoveTowards(buffer,0f,dt* bufferMax * invertGlocTime);
            if (buffer == 0f) gloc = false;
        }
        blood += (g - 1f) * dt;
        blood = Mathf.MoveTowards(blood, 0f, bloodSustain * dt);
        blood = Mathf.Clamp(blood, -bloodMax, bloodMax);

        if ((previousG && g < -0f) || (!previousG && g > 2.5f)) { sickness++; previousG = !previousG; }
        sickness = Mathf.MoveTowards(sickness, 0f, gSwitchSustain * dt);
        if (sickness > sicknessTrigger) sick = true;
        if (sick && sickness == 0f) sick = false;
        sicknessFeeling = Mathf.MoveTowards(sicknessFeeling, sick ? 1f : 0f, dt/5f);

        float targetPain = (1f - Stamina()) * Mathf.Clamp01(-Blood() * 1.5f);
        pain = Mathf.MoveTowards(pain, targetPain, (targetPain > pain ? 2f : painRecoveryRate) * dt);


        Vector3 gOffsetLocal = crew.data.acceleration / Physics.gravity.y;
        gOffsetLocal.y *= 0.5f;
        Vector3 accelerationOffset = crew.data.tr.TransformDirection(gOffsetLocal);
        accelerationOffset += (crew.data.tr.up - Vector3.up) * 4f;
        accelerationCompensation = Vector3.MoveTowards(accelerationCompensation, accelerationOffset, dt * 2f);
        accelerationCompensation = Vector3.ClampMagnitude(accelerationCompensation, accelerationOffset.magnitude);
        headPositionOffset = (accelerationOffset - accelerationCompensation) / 50f;
    }

    public Vector3 headPositionOffset { get; private set; }
    private Vector3 accelerationCompensation = Vector3.zero;
}
