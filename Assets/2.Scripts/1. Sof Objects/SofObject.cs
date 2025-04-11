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

    public SofComplex complex { get; private set; }
    public SofAircraft aircraft { get; private set; }
    public SofDebris debris { get; private set; }
    public SofSimple simpleDamage { get; private set; }

    public bool warOnly;

    public bool destroyed = false;
    public bool burning = false;

    public virtual void SetReferences()
    {
        tr = transform;
        simpleDamage = GetComponent<SofSimple>();
        complex = GetComponent<SofComplex>();
        debris = GetComponent<SofDebris>();
        aircraft = GetComponent<SofAircraft>();

        SetRigidbody();

        if (Application.isPlaying)
        {
            gameObject.layer = complex ? (simpleDamage ? 0 : 9) : 0;
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
    public virtual void Explosion(Vector3 center, float tnt)
    {
        if (simpleDamage) simpleDamage.Explosion(center, tnt);
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


        if (!sofObj.aircraft) sofObj.warOnly = EditorGUILayout.Toggle("War Only", sofObj.warOnly);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif