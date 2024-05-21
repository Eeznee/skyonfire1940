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
    [HideInInspector] public Animator animator;

    [HideInInspector] public SofObject sofObject;
    [HideInInspector] public SofComplex complex;
    [HideInInspector] public SofAircraft aircraft;

    [HideInInspector] public ObjectAudio avm;
    [HideInInspector] public ObjectData data;

    [HideInInspector] public Vector3 localPos;

    private bool initialized = false;

    public void SetReferences() {
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
        if (_complex == null) Debug.LogError("This Component is not attached to a SofComplex", this);
        sofObject = _complex;
        complex = _complex;
        aircraft = sofObject.aircraft;

        tr = transform;
        rb = complex.rb;
        avm = complex.avm;
        data = complex.data;
        animator = aircraft ? aircraft.animator : null;

        localPos = complex.transform.InverseTransformPoint(tr.position);
    }
    public virtual void AttachNewComplex(SofComplex newComplex)
    {

    }
    public virtual void Initialize(SofComplex _complex)
    {
        initialized = true;
        gameObject.layer = DefaultLayer();
        Rearm();
    }
    public void SetInstanciatedComponent(SofComplex _complex)
    {
        SetReferences(_complex);
        AttachNewComplex(_complex);
        if (!initialized) Initialize(_complex);
        complex.RegisterComponent(this);
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
    }
    public virtual void Rearm()
    {

    }
}
public static class SofComponentExtension
{
    public static T AddSofComponent<T>(this GameObject mono, SofComplex complex) where T : SofComponent
    {
        T sofComponent = mono.gameObject.AddComponent<T>();
        sofComponent.SetInstanciatedComponent(complex);
        return sofComponent;
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
