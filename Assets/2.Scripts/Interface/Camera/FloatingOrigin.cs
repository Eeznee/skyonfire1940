using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 100.0f;
    public float physicsThreshold = 1000.0f;

#if OLD_PHYSICS
    public float defaultSleepVelocity = 0.14f;
    public float defaultAngularVelocity = 0.14f;
#else
    public float defaultSleepThreshold = 0.14f;
#endif

    ParticleSystem.Particle[] parts = null;

    void MoveParticles(Vector3 offset)
    {
        Object[] objects = FindObjectsOfType(typeof(ParticleSystem));
        foreach (Object o in objects)
        {
            ParticleSystem sys = (ParticleSystem)o;

            var sysTrails = sys.trails;
            sysTrails.enabled = false;

            int particlesNeeded = sys.main.maxParticles;

            if (sys.main.simulationSpace != ParticleSystemSimulationSpace.World || particlesNeeded <= 0) continue;

            bool wasPaused = sys.isPaused;
            bool wasPlaying = sys.isPlaying;

            if (!wasPaused)
                sys.Pause();

            // ensure a sufficiently large array in which to store the particles
            if (parts == null || parts.Length < particlesNeeded)
                parts = new ParticleSystem.Particle[particlesNeeded];

            // now get the particles
            int num = sys.GetParticles(parts);
            for (int i = 0; i < num; i++) parts[i].position -= offset;

            sys.SetParticles(parts, num);

            if (wasPlaying) sys.Play();
        }
    }

    void LateUpdate()
    {
        Vector3 cameraPosition = gameObject.transform.position;
        cameraPosition.y = 0f;
        if (cameraPosition.magnitude > threshold)
        {
            foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
                g.transform.position -= cameraPosition;

            MoveParticles(cameraPosition);

            float physicsThreshold2 = physicsThreshold * physicsThreshold; // simplify check on threshold
            foreach (Rigidbody r in FindObjectsOfType(typeof(Rigidbody)))
                r.sleepThreshold = r.transform.position.sqrMagnitude > physicsThreshold2 ? float.MaxValue : defaultSleepThreshold;
        }
    }
}
/*
public class FloatingOrigin : MonoBehaviour
{
    private void MoveTrailRenderers(Vector3 offset)
    {
        var trails = FindObjectsOfType<TrailRenderer>();
        foreach (var trail in trails)
        {
            Vector3[] positions = new Vector3[trail.positionCount];

            int positionCount = trail.GetPositions(positions);
            for (int i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            trail.SetPositions(positions);
        }
    }

    private void MoveLineRenderers(Vector3 offset)
    {
        var lines = FindObjectsOfType<LineRenderer>() as LineRenderer[];
        foreach (var line in lines)
        {
            Vector3[] positions = new Vector3[line.positionCount];

            int positionCount = line.GetPositions(positions);
            for (int i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            line.SetPositions(positions);
        }
    }

    private void MoveParticles(Vector3 offset)
    {
        var particles = FindObjectsOfType<ParticleSystem>() as ParticleSystem[];
        foreach (ParticleSystem system in particles)
        {
            if (system.main.simulationSpace != ParticleSystemSimulationSpace.World)
                continue;

            int particlesNeeded = system.main.maxParticles;

            if (particlesNeeded <= 0)
                continue;

            // ensure a sufficiently large array in which to store the particles
            if (parts == null || parts.Length < particlesNeeded)
            {
                parts = new ParticleSystem.Particle[particlesNeeded];
            }

            // now get the particles
            int num = system.GetParticles(parts);

            for (int i = 0; i < num; i++)
            {
                parts[i].position -= offset;
            }

            system.SetParticles(parts, num);
        }
    }
}
*/