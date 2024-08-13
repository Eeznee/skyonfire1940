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
    public ModuleMaterial radiatorMat;
    public PhysicMaterial aircraftMat;
    public ModuleMaterial crewMat;
}
