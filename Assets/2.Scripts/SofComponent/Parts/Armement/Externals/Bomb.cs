using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Sof Components/Weapons/Heavy Ordnance/Bomb")]
public class Bomb : Detachable
{
    public ExplosiveFiller filler;

    const float delayFuse = 5f;

    bool safetyActive;

    protected override void OnDrop()
    {
        base.OnDrop();

        safetyActive = Mathf.Abs(rb.velocity.magnitude) < 15f;
    }

    protected override void OnGroundContact(float timeSinceDrop)
    {
        base.OnGroundContact(timeSinceDrop);

        if (rb.velocity.y < -10f && GameManager.map.HeightAtPoint(tr.position) > 5f) Root();    //Stuck in the ground

        if (timeSinceDrop < SecurityTimer || safetyActive)                                      //Safety Triggered, bomb will not explode
        {
            Destroy(transform.root.gameObject, 20f);
            return;
        }

        StartCoroutine(Detonate());
    }

    protected IEnumerator Detonate()
    {
        yield return new WaitForSeconds(0f);
        filler.Detonate(transform.position, mass, null);
        Destroy(transform.root.gameObject);
    }
}
