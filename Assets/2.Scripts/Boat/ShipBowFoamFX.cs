using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBowFoamFX : MonoBehaviour
{
    SofShip ship;
    private void Start()
    {
        ship = GetComponentInParent<SofShip>();
        if (ship == null)
        {
            Destroy(gameObject);
            return;
        }
        ship.UpdateBounds();
        transform.localPosition = new Vector3(0f, 1f, ship.BowPoint - 0.5f);

        ParticleSystem ps = GetComponent<ParticleSystem>();
        var shapeModule = ps.shape;
        shapeModule.radius = ship.width * 0.5f;
    }
}
