using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashEffect : ObjectElement
{
    bool crashed = false;

    public float verticalVelKph = 150f;
    public GameObject groundExplosion;
    public GameObject waterSplash;

    const float minGThreshold = 15f;
    const float humanGTolerance = 28f;
    const float humanGLethal = 40f;
    const float airframeGTolerance = 50f;

    const float mult = 1.5f;

    private void FixedUpdate()
    {
        if (crashed || Time.timeScale == 0f) return;

        //Two methods to check for crash (both are not reliable 100%)
        //Method 1 : use acceleration value
        if (data.acceleration.sqrMagnitude > mult * Mathf.Pow(minGThreshold * -Physics.gravity.y, 2) && data.relativeAltitude < 30f)
        {
            Crash(data.acceleration.magnitude / (-Physics.gravity.y * mult));
        }
        if (crashed) return;
        //Method 2 : use relative altitude and vertical speed
        float timeToCrash = data.relativeAltitude / -aircraft.data.rb.velocity.y;
        if (timeToCrash < Time.fixedDeltaTime * 10f && timeToCrash > 0f)
        {
            //weirdly, the estimated G is approximatively the vertical velocity
            float estimatedG = -rb.velocity.y * 1.2f;
            if (estimatedG > minGThreshold) Crash(estimatedG);
        }
    }

    void Crash(float g)
    {
        crashed = true;
        aircraft.destroyed = true;

        if (g > 45f)
        {
            //Crash effects
            if (data.altitude - data.relativeAltitude > 1f) //If on the ground
                Instantiate(groundExplosion, transform.position, groundExplosion.transform.rotation, transform);
            else                                            //If water
                Instantiate(waterSplash, transform.position + (transform.position.y - 0.05f) * Vector3.down, Quaternion.identity);
        }

        foreach (CrewMember c in aircraft.GetComponentsInChildren<CrewMember>())
            c.Damage(c.maxHp * Mathf.InverseLerp(humanGTolerance,humanGLethal, g * Random.Range(0.9f, 1.1f)));
        if (GameManager.player.tr != transform.root) return;
        foreach (Airframe frame in aircraft.GetComponentsInChildren<Airframe>())
            if (g > airframeGTolerance * Random.Range(0.8f, 1.2f)) frame.Rip();
    }
}
