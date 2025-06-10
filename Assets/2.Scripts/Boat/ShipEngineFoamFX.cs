using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipEngineFoamFX : MonoBehaviour
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
        transform.localPosition = new Vector3(0f, 0f, ship.SternPoint + 2f);
    }
}
