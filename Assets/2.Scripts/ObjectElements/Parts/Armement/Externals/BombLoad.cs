using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BombLoad : OrdnanceLoad
{
    public bool bombBay;
    public Bomb bombRef;
    public ExternalGroup externalGroup;

    private Bomb[] bombs;

    public override float SingleMass()
    {
        return bombRef.Mass();
    }


    public override void ReloadOrdnance()
    {
        base.ReloadOrdnance();

        
        if (bombs != null) foreach (Bomb b in bombs) if (b != null && b.transform.root == transform.root) Destroy(b.gameObject);

        bombs = new Bomb[launchPositions.Length];

        for (int i = 0; i < launchPositions.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(launchPositions[i]);
            bombs[i] = Instantiate(bombRef, pos, transform.rotation, transform);
            bombs[i].Initialize(data, true);
        }
        fireIndex = launchPositions.Length - 1;
    }

    public override bool Launch(float delayFuse)
    {
        if (fireIndex < 0) return false;
        if (bombBay && aircraft.bombBay.state < 1f) return false;

        bombs[fireIndex].Drop(5f, bombBay);

        base.Launch(delayFuse);

        return true;
    }
    public void CopyPositions()
    {
        launchPositions = new Vector3[externalGroup.order.Length];
        for(int i = 0; i < externalGroup.order.Length; i++)
        {
            launchPositions[i] = transform.InverseTransformPoint(externalGroup.order[i].transform.position);
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BombLoad))]
public class BombLoadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        BombLoad bomb = (BombLoad)target;
        if (GUILayout.Button("Copy External Group"))
        {
            bomb.CopyPositions();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(bomb);
            EditorSceneManager.MarkSceneDirty(bomb.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif