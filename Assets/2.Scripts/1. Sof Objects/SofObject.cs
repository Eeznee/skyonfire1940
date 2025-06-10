using UnityEngine;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofObject : MonoBehaviour
{
    public virtual int DefaultLayer() { return 0; }
    //References
    public Transform tr { get; private set; }
    public Rigidbody rb { get; protected set; }

    public SofModular modular { get; private set; }
    public SofComplex complex { get; private set; }
    public SofAircraft aircraft { get; private set; }
    public SofDebris debris { get; private set; }
    public SimpleDamageModel simpleDamage { get; private set; }
    public SofDamageModel damageModel { get; protected set; }

    public bool warOnly;

    public bool destroyed = false;
    public bool burning = false;

    public virtual void SetReferences()
    {
        tr = transform;
        simpleDamage = GetComponent<SimpleDamageModel>();
        modular = GetComponent<SofModular>();
        complex = GetComponent<SofComplex>();
        debris = GetComponent<SofDebris>();
        aircraft = GetComponent<SofAircraft>();
        damageModel = GetComponent<SofDamageModel>();

        SetRigidbody();

        if (Application.isPlaying)
        {
            gameObject.layer = modular ? (simpleDamage ? 0 : 9) : 0;
        }
    }
    protected virtual void SetRigidbody()
    {
        if (aircraft)
            rb = this.GetCreateComponent<Rigidbody>();
        else
            rb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : tr.root.GetComponent<Rigidbody>();

        if (rb && rb.transform == transform)
        {
            rb.angularDrag = 0f;
            rb.drag = 0f;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Extrapolate;
        }

    }
    protected virtual void GameInitialization()
    {
        SetReferences();
    }
    protected virtual void OnEnable()
    {
        GameManager.sofObjects.Add(this);
    }
    protected virtual void OnDisable()
    {
        GameManager.sofObjects.Remove(this);
    }
    private void Start()
    {
        if (warOnly && !GameManager.war) Destroy(gameObject);
        GameInitialization();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofObject))]
public class SofObjectEditor : Editor
{
    SerializedProperty warOnly;
    protected virtual void OnEnable()
    {
        warOnly = serializedObject.FindProperty("warOnly");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofObject sofObj = (SofObject)target;

        if (!sofObj.aircraft) EditorGUILayout.PropertyField(warOnly);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif