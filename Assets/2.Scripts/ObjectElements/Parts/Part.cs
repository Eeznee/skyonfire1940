using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class Part : ObjectElement       //Parts are Object Elements with mass
{
    public float emptyMass = 0f;

    public virtual float Mass()
    {
        return emptyMass;
    }
    public virtual float EmptyMass()
    {
        return emptyMass;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }
    public void Detach()
    {
        //Destroy all line renderers
        foreach (LineRenderer rope in GetComponentsInChildren<LineRenderer>()) rope.enabled = false;

        //Completely separate this part from the aircraft
        if (!complex) return;
        data = tr.root.GetComponent<ObjectData>();
        Rigidbody previousRb = data.rb;
        Mass detachedMass = new Mass(GetComponentsInChildren<Part>(), false);
        data.ShiftMass(new Mass(-detachedMass.mass,detachedMass.center)); 
        if (data.GetMass() <= 0f) Debug.LogError("Mass below zero", gameObject);
        GameObject obj = new GameObject(name + " Ripped Off");
        obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        Rigidbody objRb = obj.AddComponent<Rigidbody>();
        transform.parent = obj.transform;
        ObjectData objData = obj.AddComponent<ObjectData>();

        objData.Initialize(false);
        objRb.velocity = previousRb.velocity;
        Destroy(obj, 30f);
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(Part))]
public class PartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Part part = (Part)target;

        part.emptyMass = EditorGUILayout.FloatField("Empty Mass", part.emptyMass);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(part);
            EditorSceneManager.MarkAllScenesDirty();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
