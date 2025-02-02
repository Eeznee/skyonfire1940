using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndercarriageManager
{
    private SofAircraft aircraft;

    public UndercarriageManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;
    }

    public void Initialize(bool grounded)
    {
        if (aircraft.hydraulics.gear) aircraft.hydraulics.gear.OneFrameSetStart(1f);

        SolveSuspensionsBalance();

        if (aircraft.hydraulics.gear)
        {
            aircraft.hydraulics.gear.OneFrameSetEnd();
            aircraft.hydraulics.gear.SetInstant(grounded);
        }
    }
    void SolveSuspensionsBalance()
    {
        Wheel[] wheels = aircraft.GetComponentsInChildren<Wheel>();
        Vector3[] positions = new Vector3[wheels.Length];
        Matrix4x4 flattenWheels = FlattenMatrix(wheels);
        Vector3 centerOfGravity = flattenWheels * aircraft.GetCenterOfMass();

        for (int i = 0; i < wheels.Length; i++)
        {
            positions[i] = flattenWheels * wheels[i].localPos;
            positions[i] -= centerOfGravity;
        }

        ComputeNormalForces(positions,out float noseNormalForce, out float rearNormalForce);

       // for (int i = 0; i < positions.Length; i++) wheels[i].suspension.AdjustSuspension(positions[i].z > 0f ? noseNormalForce : rearNormalForce);
    }
    private Matrix4x4 FlattenMatrix(Wheel[] wheels)
    {
        Vector3 forwardPos = wheels[0].localPos;
        Vector3 rearwardPos = wheels[0].localPos;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].localPos.z > forwardPos.z) forwardPos = wheels[i].localPos;
            if (wheels[i].localPos.z < rearwardPos.z) rearwardPos = wheels[i].localPos;
        }

        Matrix4x4 flattenWheels = Matrix4x4.Rotate(Quaternion.LookRotation(forwardPos - rearwardPos, Vector3.up));
        flattenWheels = flattenWheels.inverse;
        return flattenWheels;
    }
    private void ComputeNormalForces(Vector3[] positions, out float noseNormalForce, out float rearNormalForce)
    {
        float totalNormalForce = aircraft.GetMass() * 0.93f  * -Physics.gravity.y;

        float noseWheelsDistance = 0f;
        float rearWheelsDistance = 0f;
        int noseWheels = 0;
        int rearWheels = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i].z > 0f)
            {
                noseWheelsDistance += positions[i].z;
                noseWheels++;
            }
            if (positions[i].z < 0f)
            {
                rearWheelsDistance += positions[i].z;
                rearWheels++;
            }
        }
        noseWheelsDistance /= noseWheels;
        rearWheelsDistance /= rearWheels;

        float noseRatio = rearWheelsDistance / (rearWheelsDistance - noseWheelsDistance);

        noseNormalForce = noseRatio * totalNormalForce / noseWheels;
        rearNormalForce = (1f - noseRatio) * totalNormalForce / rearWheels;
    }
}
