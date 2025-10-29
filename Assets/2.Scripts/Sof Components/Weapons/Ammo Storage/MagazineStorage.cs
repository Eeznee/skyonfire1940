using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Weapons/Guns/Magazine Storage")]
public class MagazineStorage : SofComponent, IMassComponent
{
    public float RealMass => magRef ? magRef.FullyLoadedMass * magsLeft : 0f;
    public float LoadedMass => magRef ? magRef.FullyLoadedMass * MagsCount : 0f;
    public float EmptyMass => 0f;

    public int MagsCount => (positions != null && localRotations != null) ? Mathf.Min(positions.Length, localRotations.Length) : 0;

    public Magazine magRef;

    public Vector3[] positions;
    public Vector3[] localRotations;

    [HideInInspector] public int magsLeft;

    public override void Initialize(SofModular _complex)
    {
        magsLeft = MagsCount;
        base.Initialize(_complex);
        UpdateMergedModel();
    }
    private void UpdateMergedModel()
    {
        CombineInstance[] combineInstances = new CombineInstance[magsLeft];
        for (int i = 0; i < magsLeft; i++)
        {
            combineInstances[i].mesh = magRef.simpleMeshForStorage;
            combineInstances[i].transform = Matrix4x4.TRS(positions[i], Quaternion.Euler(localRotations[i]), Vector3.one);
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combineInstances);

        if (!GetComponent<MeshRenderer>())
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = magRef.GetComponent<MeshRenderer>().sharedMaterial;

        if (!GetComponent<MeshFilter>()) gameObject.AddComponent<MeshFilter>();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }
    public Magazine GetMag()
    {
        if (magsLeft <= 0) return null;
        Magazine mag = Instantiate(magRef, transform.root);
        mag.SetInstanciatedComponent(sofModular);
        magsLeft--;
        UpdateMergedModel();
        return mag;
    }

#if UNITY_EDITOR
    public void ChildrenToPositions()
    {
        positions = new Vector3[transform.childCount];
        localRotations = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            positions[i] = child.localPosition;
            localRotations[i] = child.localRotation.eulerAngles;
        }
    }
    public void PositionsToChildren()
    {
        for (int i = 0; i < MagsCount; i++)
        {
            Transform magTr = Instantiate(magRef, transform).transform;
            magTr.localPosition = positions[i];
            magTr.localRotation = Quaternion.Euler(localRotations[i]);
        }
    }
#endif
}
#if UNITY_EDITOR


[CustomEditor(typeof(MagazineStorage))]
public class MagazineStorageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        MagazineStorage magStorage = (MagazineStorage)target;

        EditorGUILayout.LabelField("Magazines Count", magStorage.MagsCount.ToString());

        if (magStorage.transform.childCount > 0 && GUILayout.Button("Children to positions"))
            magStorage.ChildrenToPositions();
        if (magStorage.transform.childCount == 0 && GUILayout.Button("Positions to children"))
            magStorage.PositionsToChildren();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(magStorage);
            EditorSceneManager.MarkSceneDirty(magStorage.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}


#endif