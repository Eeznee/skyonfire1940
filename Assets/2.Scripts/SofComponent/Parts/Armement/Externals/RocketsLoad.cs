using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RocketsLoad : OrdnanceLoad
{
    public float dispersion = 0.5f;
    public float velocity = 525f;
    public Projectile rocketRef;
    private Projectile[] rockets;

    public override float SingleMass => rocketRef.p.mass;
    protected override void Clear()
    {
        base.Clear();
        if (rockets == null) return;

        foreach (Projectile r in rockets)
        {
            if (r == null || r.transform.parent == null) continue;
            Destroy(r.gameObject);
        }
    }
    public override void Rearm()
    {
        base.Rearm();

        rockets = new Projectile[launchPositions.Length];

        for (int i = 0; i < launchPositions.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(launchPositions[i]);
            rockets[i] = Instantiate(rocketRef, pos, transform.rotation, transform);
            rockets[i].Setup(rockets[i].p);
            rockets[i].enabled = false;
        }
    }
    public override bool Launch(float delayFuse)
    {
        if (fireIndex >= launchPositions.Length || !aircraft) return false;

        Projectile r = rockets[fireIndex];
        r.transform.parent = null;
        r.enabled = true;

        r.transform.rotation = Ballistics.Spread(transform.rotation, dispersion);
        r.InitializeTrajectory(r.transform.forward * r.p.baseVelocity, r.transform.forward, complex.bubble.bubble,0f);
        r.GetComponentInChildren<ParticleSystem>().Play();

        complex.ShiftMass(-SingleMass);

        base.Launch(delayFuse);

        return true;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(RocketsLoad))]
public class RocketsLoadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        RocketsLoad rockets = (RocketsLoad)target;
        if (GUILayout.Button("Interpolate"))
        {
            rockets.InterpolatePositions();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(rockets);
            EditorSceneManager.MarkSceneDirty(rockets.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif