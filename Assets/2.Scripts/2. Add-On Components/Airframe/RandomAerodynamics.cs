using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAerodynamics : SofComponent
{
    public float area;

    private Vector3 localAeroCenter;
    private SimpleAirfoil airfoil;

    public void SetValues(float _area, float maximumAeroCenterOffset)
    {
        area = _area;
        localAeroCenter = Random.insideUnitSphere * maximumAeroCenterOffset;
        airfoil = new SimpleAirfoil(0.5f, 0.2f, Random.Range(-20f, 20f));
    }
    private void FixedUpdate()
    {
        Vector2 coefficients = airfoil.Coefficients(data.angleOfAttack.Get);

        Vector3 lift = Aerodynamics.Lift(rb.velocity, transform.up, data.density.Get, area, coefficients.y, 1f);
        Vector3 drag = Aerodynamics.Drag(rb.velocity, data.density.Get, area, coefficients.x, 1f);

        rb.AddForceAtPosition(lift + drag,transform.TransformPoint(localAeroCenter));
    }
}
