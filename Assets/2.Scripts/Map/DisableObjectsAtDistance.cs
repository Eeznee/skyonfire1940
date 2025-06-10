using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DisableObjectsAtDistance : MonoBehaviour
{
    public bool affectsOnlyChildren = true;
    public GameObject[] gameObjectsToDisable;

    public bool matchCameraFrustrum = true;
    public float maxDistanceFromPlayer = 10000f;
    private float MaxDistanceSqr => M.Pow(matchCameraFrustrum ? RangeByAltitude.currentRange : maxDistanceFromPlayer, 2);

    private bool isEnabled;
    private void Start()
    {
        isEnabled = true;
        if (affectsOnlyChildren)
        {
            gameObjectsToDisable = new GameObject[transform.childCount];
            for (int i = 0; i < gameObjectsToDisable.Length; i++)
            {
                gameObjectsToDisable[i] = transform.GetChild(i).gameObject;
            }
        }
        StartCoroutine(RepeatUpdateDisableState());
        UpdateDisableState();

    }

    IEnumerator RepeatUpdateDisableState()
    {
        yield return new WaitForSecondsRealtime(Random.Range(0f, 1.5f));

        while (true)
        {
            UpdateDisableState();
            yield return new WaitForSecondsRealtime(Random.Range(0f, 5f));
        }
    }
    private void OnEnable()
    {
        Player.OnCrewChange += UpdateDisableState;
        SofCamera.OnSwitchCamEvent += UpdateDisableState;
        CameraEditor.OnSubcamSettingsChange += UpdateDisableState;
    }
    private void OnDisable()
    {
        Player.OnCrewChange -= UpdateDisableState;
        SofCamera.OnSwitchCamEvent -= UpdateDisableState;
        CameraEditor.OnSubcamSettingsChange -= UpdateDisableState;
    }
    void UpdateDisableState()
    {
        float distanceSqr = (SofCamera.tr.position - transform.position).sqrMagnitude;
        distanceSqr = Mathf.Min(distanceSqr, (SofCamera.subCam.Target().tr.position - transform.position).sqrMagnitude);
        if (Player.tr) distanceSqr = Mathf.Min(distanceSqr, (Player.tr.position - transform.position).sqrMagnitude);


        bool objectsEnabled = distanceSqr < MaxDistanceSqr;

        if (objectsEnabled != isEnabled)
        {
            isEnabled = objectsEnabled;

            foreach (GameObject obj in gameObjectsToDisable)
            {
                if (obj == null) continue;
                obj.SetActive(isEnabled);
            }
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(DisableObjectsAtDistance))]
public class DisableObjectsAtDistanceEditor : Editor
{
    SerializedProperty affectsOnlyChildren;
    SerializedProperty gameObjectsToDisable;
    SerializedProperty matchCameraFrustrum;
    SerializedProperty maxDistanceFromPlayer;

    protected void OnEnable()
    {
        affectsOnlyChildren = serializedObject.FindProperty("affectsOnlyChildren");
        gameObjectsToDisable = serializedObject.FindProperty("gameObjectsToDisable");
        matchCameraFrustrum = serializedObject.FindProperty("matchCameraFrustrum");
        maxDistanceFromPlayer = serializedObject.FindProperty("maxDistanceFromPlayer");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DisableObjectsAtDistance script = (DisableObjectsAtDistance)target;

        EditorGUILayout.PropertyField(affectsOnlyChildren);
        if (!script.affectsOnlyChildren)
        {
            EditorGUILayout.PropertyField(gameObjectsToDisable);
        }
        EditorGUILayout.PropertyField(matchCameraFrustrum);
        if (!script.matchCameraFrustrum)
        {
            EditorGUILayout.PropertyField(maxDistanceFromPlayer);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif