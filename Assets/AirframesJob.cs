using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;

[BurstCompile]
public class AirframesJob : MonoBehaviour
{
    private SofAirframe[] airframes;
    private AirframesForcesJob job;
    private void Start()
    {
        airframes = Object.FindObjectsOfType<MainSurface>();
        CreateJob();
    }

    private void CreateJob()
    {
        NativeArray<float3> _Force = new NativeArray<float3>(airframes.Length, Allocator.Persistent);
        NativeArray<float> _SurfaceArea = new NativeArray<float>(airframes.Length, Allocator.Persistent);

        for (int i = 0; i < airframes.Length; i++)
        {
            _Force[i] = Vector3.zero;
            _SurfaceArea[i] = airframes[i].area;
        }

        job = new AirframesForcesJob()
        {
            SurfaceArea = _SurfaceArea,
            Force = _Force,
            AirDensity = 1f//data.density.Get,
        };
    }
    private void OnDestroy()
    {
        
    }
    public void FixedUpdate()
    {
        NativeArray<quaternion> _Rotations = new NativeArray<quaternion>(airframes.Length, Allocator.Persistent);
        NativeArray<float3> _Velocity = new NativeArray<float3>(airframes.Length, Allocator.Persistent);

        for (int i = 0; i < airframes.Length; i++)
        {
            _Rotations[i] = Quaternion.LookRotation(airframes[i].quad.chordDir.WorldDir, airframes[i].quad.upDir.WorldDir);
            _Velocity[i] = airframes[i].rb.GetPointVelocity(airframes[i].quad.centerAero.WorldPos);
        }

        job.Rotations = _Rotations;
        job.Velocity = _Velocity;

        JobHandle jobHandle = job.Schedule(airframes.Length,16);
        jobHandle.Complete();

        for (int i = 0; i < airframes.Length; i++)
        {
            airframes[i].rb.AddForceAtPosition(job.Force[i], airframes[i].quad.centerAero.WorldPos);
        }
        _Rotations.Dispose();
        _Velocity.Dispose();
    }
}

[BurstCompile]
public struct AirframesForcesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<quaternion> Rotations;
    [ReadOnly] public NativeArray<float3> Velocity;
    [ReadOnly] public NativeArray<float> SurfaceArea;

    [ReadOnly] public float AirDensity;


    [WriteOnly] public NativeArray<float3> Force;



    public void Execute(int index)
    {
        float3 aeroDir = math.mul(Rotations[index], new float3(1f, 0f, 0f));
        float3 upDir = math.mul(Rotations[index], new float3(0f, 1f, 0f));
        float3 chordDir = math.mul(Rotations[index], new float3(0f, 0f, 1f));

        float3 projectedVel = Velocity[index] - aeroDir * math.dot(Velocity[index], aeroDir);
        float projectedVelMagnitude = math.length(projectedVel);

        float alpha = math.degrees(math.atan2(math.dot(projectedVel, upDir),math.dot(projectedVel, chordDir)));

        float cl = -alpha * 0.1f + 0.05f;
        float cd = 0f;// cl * cl * (1f + math.abs(cl)) * 0.03f * 0.5f + 0.01f;

        float aeroFactor = 0.5f * AirDensity * SurfaceArea[index] * projectedVelMagnitude;
        float lift = cl * aeroFactor;
        float drag = cd * aeroFactor;

        float3 liftDir = math.cross(projectedVel,aeroDir);
        Force[index] = lift * liftDir + drag * -Velocity[index]; 
    }
}