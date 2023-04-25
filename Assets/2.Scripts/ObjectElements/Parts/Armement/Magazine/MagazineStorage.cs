using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class MagazineStorage : Part
{
    public Magazine magRef;

    public Vector3[] positions;
    public Vector3[] localRotations;

    [HideInInspector] public int magsCount;
    [HideInInspector] public int magsLeft;

    public override float EmptyMass()
    {
        return 0f;
    }
    public override float Mass()
    {
        return magsLeft * magRef.LoadedMass();
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            magsLeft = magsCount = Mathf.Min(positions.Length,localRotations.Length);
            Merge();
        }
    }
    private void Merge()
    {
        CombineInstance[] combineInstances = new CombineInstance[magsCount];
        for (int i = 0; i < magsCount; i++)
        {
            combineInstances[i].mesh = magRef.simpleMesh;
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
        if (true)
        {
            if (magsLeft <= 0) return null;
            Magazine mag = Instantiate(magRef, transform.root);
            mag.Initialize(data, true);
            magsLeft--;
            return mag;
        }
        else
        {
            if (transform.childCount == 0) return null;
            int rand = Random.Range(0, transform.childCount);
            Magazine mag = transform.GetChild(rand).GetComponent<Magazine>();
            //mag.UnMerge();
            mag.transform.parent = transform.root;
            magsLeft = transform.childCount;
            return mag;
        }
    }
    public void ChildrenToPositions()
    {
        magsCount = transform.childCount;
        positions = new Vector3[magsCount];
        localRotations = new Vector3[magsCount];
        for (int i = 0; i < magsCount; i++)
        {
            Transform child = transform.GetChild(i);

            positions[i] = child.localPosition;
            localRotations[i] = child.localRotation.eulerAngles;
        }
    }
    public void PositionsToChildren()
    {
        magsCount = Mathf.Min(positions.Length, localRotations.Length);
        for (int i = 0; i < magsCount; i++)
        {
            Transform magTr = Instantiate(magRef, transform).transform;
            magTr.localPosition = positions[i];
            magTr.localRotation = Quaternion.Euler(localRotations[i]);
        }
    }
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

        EditorGUILayout.LabelField("Magazines Count",magStorage.magsCount.ToString());

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