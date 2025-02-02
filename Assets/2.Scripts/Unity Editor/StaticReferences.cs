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

    public BulletHits fragmentsHits;
    public Mesh wheelCollisionMesh;
    public Airfoil stabilizersAirfoil;
    public PhysicMaterial wheelPhysicMaterial;
    public PhysicMaterial aircraftPhysicMaterial;
}
