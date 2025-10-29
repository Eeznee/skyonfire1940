using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFlamesFX : MonoBehaviour
{
    public void SetparticleSystem(SofShip ship, float zPosition)
    {
        transform.parent = ship.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        float midShipLength = ship.MidBowPoint - ship.MidSternPoint;
        float flamesLength = Mathf.Max(10f, midShipLength * 0.5f);
        float minPos = ship.MidSternPoint + flamesLength * 0.5f;
        float maxPos = ship.MidBowPoint - flamesLength * 0.5f;

        Vector3 pos = new Vector3(0f, ship.heightAboveWater * 0.75f, Mathf.Clamp(zPosition, minPos, maxPos));
        Vector3 scale = new Vector3(ship.width, ship.heightAboveWater * 0.5f, flamesLength);


        foreach (ParticleSystem ps in particleSystems)
        {
            ChangeBoxProperties(ps, pos, scale);
        }
    }

    public void StopFire()
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
        }
    }

    private void ChangeBoxProperties(ParticleSystem ps, Vector3 position, Vector3 scale)
    {
        ps.Play();

        var shapeModule = ps.shape;

        shapeModule.shapeType = ParticleSystemShapeType.Box;

        shapeModule.position = position;
        shapeModule.rotation = new Vector3 (0f,0f,0f);
        shapeModule.scale = scale;
    }
}
