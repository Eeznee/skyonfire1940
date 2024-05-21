#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Materials List", menuName = "SOF/Materials/Module Material List")]
public class MaterialsList : ScriptableObject
{
    public ModuleMaterial airframeMat;
    public ModuleMaterial stabilizerMat;
    public ModuleMaterial controlsMat;
    public ModuleMaterial sparMat;
    public ModuleMaterial skinMat;
    public ModuleMaterial wheelMat;
    public PhysicMaterial aircraftMat;

    public ModuleMaterial Material(SofModule m)
    {
        if (m.GetComponent<Wheel>()) return wheelMat;
        if (m.GetComponent<WingSkin>()) return skinMat;
        if (m.GetComponent<Wing>()) return sparMat;
        if (m.GetComponent<Slat>() || m.GetComponent<Flap>() || m.GetComponent<ControlSurface>()) return controlsMat;
        if (m.GetComponent<Stabilizer>()) return stabilizerMat;
        if (m.GetComponent<SofAirframe>()) return airframeMat;
        return airframeMat;
    }

    public void ApplyMaterials(SofAircraft aircraft)
    {
        foreach (MeshCollider col in aircraft.GetComponentsInChildren<MeshCollider>())
        {
            col.convex = true;
            col.sharedMaterial = aircraftMat;
        }

        foreach (SofAirframe frame in aircraft.GetComponentsInChildren<SofAirframe>())
            frame.material = airframeMat;
        foreach (Stabilizer stab in aircraft.GetComponentsInChildren<Stabilizer>())
            stab.material = stabilizerMat;
        foreach (ControlSurface cs in aircraft.GetComponentsInChildren<ControlSurface>())
            cs.material = controlsMat;
        foreach (Flap flap in aircraft.GetComponentsInChildren<Flap>())
            flap.material = controlsMat;
        foreach (Slat slat in aircraft.GetComponentsInChildren<Slat>())
            slat.material = controlsMat;
        foreach (Wing spar in aircraft.GetComponentsInChildren<Wing>())
            spar.material = sparMat;
        foreach (WingSkin skin in aircraft.GetComponentsInChildren<WingSkin>())
            skin.material = skinMat;
        foreach (Wheel wheel in aircraft.GetComponentsInChildren<Wheel>())
            wheel.material = wheelMat;
    }
}
