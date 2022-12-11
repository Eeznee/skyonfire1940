using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofObject : MonoBehaviour
{
    //References
    public bool warOnly;
    public ObjectData data;
    public AVM avm;
    public CrewMember[] crew = new CrewMember[1];

    public Vector3 viewPoint = new Vector3(0, 3f, -15f);

    public bool destroyed = false;
    public bool burning = false;

    private void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        avm = GetComponentInChildren<AVM>();
        data = GetComponent<ObjectData>() ? GetComponent<ObjectData>() : gameObject.AddComponent<ObjectData>();
        data.Initialize(true);
        if (warOnly && !GameManager.war) { Destroy(gameObject); return; }
        GameManager.sofObjects.Add(this);
    }
    public virtual void Explosion(Vector3 center, float tnt)
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofObject))]
public class SofObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofObject sofObj = (SofObject)target;

        sofObj.viewPoint = EditorGUILayout.Vector3Field("External Camera ViewPoint", sofObj.viewPoint);
        SerializedProperty crew = serializedObject.FindProperty("crew");
        EditorGUILayout.PropertyField(crew, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(sofObj);
            EditorSceneManager.MarkSceneDirty(sofObj.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif