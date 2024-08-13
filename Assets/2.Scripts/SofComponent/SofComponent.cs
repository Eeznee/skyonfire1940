using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public abstract class SofComponent : MonoBehaviour  //Objects elements are the building blocks of Sof Objects
{
    public virtual int DefaultLayer() { return sofObject.DefaultLayer(); }

    public Transform tr { get; private set; }
    public Rigidbody rb { get; private set; }
     public Animator animator { get; private set; }

    public SofObject sofObject { get; private set; }
    public SofComplex complex { get; private set; }
    public SofAircraft aircraft { get; private set; }

    public ObjectData data { get; private set; }

    public Vector3 localPos { get; private set; }

    private bool initialized = false;

    public void SetReferences()
    {
        if (Application.isEditor)
        {
            complex = transform.root.GetComponent<SofComplex>();
            if (complex == null) { Debug.LogError("This Sof Component is not attached to Sof Complex", this); return; }
            complex.EditorInitialization();
            return;
        }
        if (complex == null) return;
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
        data = complex.data;
        animator = aircraft ? aircraft.animator : null;

        localPos = complex.transform.InverseTransformPoint(tr.position);
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
    SerializedProperty mass;

    protected virtual void OnEnable()
    {
        mass = serializedObject.FindProperty("mass");

        SofComponent component = (SofComponent)target;
        component.SetReferences();
    }

    protected virtual string BasicName() { return "Component"; }

    protected virtual void BasicFoldout()
    {
        SofComponent component = (SofComponent)target;
        IMassComponent massComponent = component as IMassComponent;
        if (massComponent != null)
        {
            if (mass != null) EditorGUILayout.PropertyField(mass);
            else
            {
                EditorGUILayout.LabelField("Empty Mass", massComponent.EmptyMass.ToString("0.0") + " kg");
                EditorGUILayout.LabelField("Loaded Mass", massComponent.LoadedMass.ToString("0.0") + " kg");
            }
        }
    }

    static bool showBasic = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showBasic = EditorGUILayout.Foldout(showBasic, BasicName(), true, EditorStyles.foldoutHeader);
        if (showBasic)
        {
            EditorGUI.indentLevel++;
            BasicFoldout();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
