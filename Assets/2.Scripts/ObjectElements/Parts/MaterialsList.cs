#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Part Part Materials List", menuName = "Aircraft/Part Material List")]
public class MaterialsList : ScriptableObject
{
    public PartMaterial airframeMat;
    public PartMaterial stabilizerMat;
    public PartMaterial controlsMat;
    public PartMaterial sparMat;
    public PartMaterial skinMat;
    public PartMaterial wheelMat;
    public PhysicMaterial aircraftMat;

    public void ApplyMaterials(SofAircraft aircraft)
    {
        foreach (MeshCollider col in aircraft.GetComponentsInChildren<MeshCollider>())
        {
            col.convex = true;
            col.sharedMaterial = aircraftMat;
        }
        foreach (Fuselage frame in aircraft.GetComponentsInChildren<Fuselage>())
            frame.material = airframeMat;
        foreach (Stabilizer stab in aircraft.GetComponentsInChildren<Stabilizer>())
            stab.material = stabilizerMat;
        foreach (ControlSurface cs in aircraft.GetComponentsInChildren<ControlSurface>())
            cs.material = controlsMat;
        foreach (Flap flap in aircraft.GetComponentsInChildren<Flap>())
            flap.material = controlsMat;
        foreach (Slat slat in aircraft.GetComponentsInChildren<Slat>())
            slat.material = controlsMat;
        foreach (Airfoil spar in aircraft.GetComponentsInChildren<Airfoil>())
            spar.material = sparMat;
        foreach (AirfoilSkin skin in aircraft.GetComponentsInChildren<AirfoilSkin>())
            skin.material = skinMat;
        foreach (Wheel wheel in aircraft.GetComponentsInChildren<Wheel>())
            wheel.material = wheelMat;
    }
}
