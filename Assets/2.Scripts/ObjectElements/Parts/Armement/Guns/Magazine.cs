using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Magazine : Module
{
    public GunPreset gunPreset;
    public int capacity = 100;
    [HideInInspector] public int ammo;
    public Vector3 ejectVector = new Vector3(0f,0.2f,0f);
    public int[] markers;
    public GameObject[] markersGameObjects;

    [HideInInspector] public HandGrip grip;
    [HideInInspector] public Gun attachedGun;
    [HideInInspector] public MagazineStock attachedStock;

    public override float Mass()
    {
        return gunPreset.ammunition.FullMass * ammo;
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            grip = GetComponentInChildren<HandGrip>();
            ammo = capacity;
        }
    }
    public bool EjectRound()
    {
        if (ammo <= 0) return false;
        ammo--;
        if (markers != null)
        {
            for (int i = 0; i < markers.Length; i++)
            {
                if (ammo < markers[i]) markersGameObjects[i].SetActive(false);
            }
        }
        return true;
    }

    public Vector3 MagTravelPos(Vector3 startPos, Vector3 endPos,float animTime)
    {
        float distance = (startPos - endPos).magnitude;
        float t = animTime * animTime;
        if (animTime > 0.2f) t = Mathf.Lerp(0.04f,1f,(animTime-0.2f)/0.8f);
        Vector3 travelOffset = (endPos - (startPos + ejectVector)) * t;
        return travelOffset + startPos + ejectVector * Mathf.Clamp01(animTime*distance * 3f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Magazine))]
public class MagazineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        Magazine mag = (Magazine)target;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(mag);
            EditorSceneManager.MarkAllScenesDirty();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
