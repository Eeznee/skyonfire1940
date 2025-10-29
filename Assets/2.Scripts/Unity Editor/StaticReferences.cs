using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

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

    public Mesh wheelCollisionMesh;
    public Airfoil stabilizersAirfoil;
    public PhysicsMaterial wheelPhysicMaterial;
    public PhysicsMaterial aircraftPhysicMaterial;

    public AudioMixer mixer;
    public AudioClip cameraShutterClip;

    public ParticleSystem engineBoostEffect;
    public ParticleSystem engineOverHeatEffect;
    public ParticleSystem engineFireEffect;
    public AudioClip[] engineDamagePops;

    public GameObject defaultAlliesCrewmember;
    public GameObject defaultAxisCrewmember;
    public GameObject cube;

    public InputActionAsset mainActions;
}
