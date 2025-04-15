using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Static References",menuName = "SOF/Static References")]
public class StaticReferences : ScriptableObject
{
    static StaticReferences _instance;

    public static StaticReferences Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<StaticReferences>("Static References");

                _instance.stabilizersAirfoil?.UpdateValues();
            }
            return _instance;
        }
    }

    public AircraftsList defaultAircrafts;

    public BulletHits fragmentsHits;
    public Mesh wheelCollisionMesh;
    public Airfoil stabilizersAirfoil;
    public PhysicMaterial wheelPhysicMaterial;
    public PhysicMaterial aircraftPhysicMaterial;

    public AudioClip cameraShutterClip;

    public ParticleSystem engineBoostEffect;
    public ParticleSystem engineOverHeatEffect;
    public ParticleSystem engineFireEffect;
    public AudioClip[] engineDamagePops;

    public GameObject defaultAlliesCrewmember;
    public GameObject defaultAxisCrewmember;
    public GameObject cube;
}
