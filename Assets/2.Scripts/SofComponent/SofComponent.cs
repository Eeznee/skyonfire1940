using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofComponent : MonoBehaviour  //Objects elements are the building blocks of Sof Objects
{
    public virtual int DefaultLayer() { return sofObject.DefaultLayer(); }

    [HideInInspector] public Transform tr;
    [HideInInspector] public Rigidbody rb;

    [HideInInspector] public SofObject sofObject;
    [HideInInspector] public SofComplex complex;
    [HideInInspector] public SofAircraft aircraft;

    [HideInInspector] public ObjectAudio avm;
    [HideInInspector] public ObjectData data;

    private bool initialized = false;
    private SofComplex registeredTo = null;

    public virtual void SetReferences() {
        if (complex == null) return;
        if (Application.isEditor)
        {
            complex = transform.root.GetComponent<SofComplex>();
            if (complex == null) { Debug.LogError("This Sof Component is not attached to Sof Complex", this); return; }
            complex.SetReferences();
            return;
        }
        SetReferences(complex);
    }
    public virtual void SetReferences(SofComplex _complex)
    {
        sofObject = _complex;
        if (sofObject == null) Debug.LogError("This Component has no Sof Object", this);
        complex = _complex;
        aircraft = sofObject.aircraft;

        gameObject.layer = DefaultLayer();

        tr = transform;
        rb = complex.rb;

        avm = complex.avm;
        data = complex.data;
    }

    public virtual void Initialize(SofComplex _complex)
    {
        initialized = true;
        Rearm();
    }
    public void InitializeComponent(SofComplex _complex)
    {
        SetReferences(_complex);
        if (_complex != registeredTo)
        {
            complex.RegisterComponent(this);
            registeredTo = complex;
        }
        if (!initialized) Initialize(_complex);
    }
    public void SetReferencesAndRegister(SofComplex _complex)
    {
        SetReferences(_complex);
        complex.RegisterComponent(this);
        registeredTo = complex;
    }
    public void Detach()
    {
        SofComplex oldComplex = complex;

        Vector3 velocity = sofObject.rb.velocity;
        GameObject detached = new GameObject(name + " Ripped Off");
        detached.AddComponent<Rigidbody>().velocity = velocity;
        detached.transform.SetPositionAndRotation(transform.position, transform.rotation);
        transform.parent = detached.transform;

        SofDebris debris = detached.AddComponent<SofDebris>();

        oldComplex.OnPartDetach(debris);
    }
    protected void OnDestroy()
    {
        if (!complex) return;
        complex.RemoveComponent(this);
        registeredTo = null;
    }
    public virtual void Rearm()
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofComponent))]
public class SofComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofComponent sofComponent = (SofComponent)target;


        if (GUI.changed)
        {
            EditorUtility.SetDirty(sofComponent);
            EditorSceneManager.MarkSceneDirty(sofComponent.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
