using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RocketsLoad : OrdnanceLoad
{
    public override float Mass()
    {
        return base.Mass() + rocketRef.p.mass * Mathf.Max(fireIndex + 1, 0);
    }
    public float dispersion = 0.5f;
    public float velocity = 525f;

    public Projectile rocketRef;
    private Projectile[] rockets;

    public override float SingleMass()
    {
        return rocketRef.p.mass;
    }
    public override void ReloadOrdnance()
    {
        base.ReloadOrdnance();

        if (rockets != null) foreach (Projectile r in rockets) if (r != null) Destroy(r.gameObject);

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
        if (fireIndex < 0) return false;

        Projectile r = rockets[fireIndex];
        r.transform.parent = null;
        r.enabled = true;

        r.transform.rotation = Ballistics.Spread(transform.rotation, dispersion);
        r.InitializeTrajectory(r.transform.forward * r.p.baseVelocity, r.transform.forward, complex.bubble);
        //data.mass -= properties.mass;
        r.GetComponentInChildren<ParticleSystem>().Play();

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