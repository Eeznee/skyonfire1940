using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public abstract class SofComponent : MonoBehaviour  //Objects elements are the building blocks of Sof Objects
{
    public virtual int DefaultLayer() { return sofObject.DefaultLayer(); }

    public Transform tr { get; private set; }
    public Rigidbody rb { get; private set; }
    public Animator animator { get; protected set; }

    public SofObject sofObject { get; private set; }
    public SofModular sofModular { get; private set; }
    public SofComplex sofComplex { get; private set; }
    public SofAircraft aircraft { get; private set; }
    public bool HasAircraft { get; private set; }

    public Vector3 localPos { get; private set; }
    public Quaternion localRot { get; private set; }

    private bool initialized = false;


    public ObjectData data => sofModular.data;


    public void SetReferences()
    {
        if (Application.isEditor)
        {
            sofModular = transform.root.GetComponent<SofModular>();
            if (sofModular) sofModular.SetReferences();
        }
        if (sofModular == null) return;
        SetReferences(sofModular);
    }
    public virtual void SetReferences(SofModular _modular)
    {
        if (_modular == null) Debug.LogError("This Component is not attached to a SofComplex", this);
        sofObject = _modular;
        sofModular = _modular;
        sofComplex = sofObject.complex;
        aircraft = sofObject.aircraft;
        HasAircraft = aircraft != null;

        tr = transform;
        rb = sofModular.rb;
        animator = aircraft ? aircraft.animator : null;

        localPos = sofModular.transform.InverseTransformPoint(tr.position);
        localRot = Quaternion.Inverse(sofModular.transform.rotation) * tr.rotation;
    }
    public virtual void Initialize(SofModular _complex)
    {
        if (Application.isEditor && !Application.isPlaying) Debug.LogError("Initialize should never be called in editor");
        initialized = true;
        gameObject.layer = DefaultLayer();
        Rearm();
    }
    public void SetInstanciatedComponent(SofModular _complex)
    {
        _complex.AddInstantiatedComponent(this);

        SetReferences(_complex);
        if (!initialized) Initialize(_complex);
    }
    public void DetachAndCreateDebris()
    {
        SofModular oldComplex = sofModular;

        Transform debrisTr = new GameObject(name + " debris").transform;
        debrisTr.SetPositionAndRotation(tr.position,tr.rotation);
        transform.parent = debrisTr;
        debrisTr.position += rb.velocity * (Time.fixedTime - Time.time);

        Rigidbody detachedRb = debrisTr.gameObject.AddComponent<Rigidbody>();
        detachedRb.velocity = rb.velocity + Random.insideUnitSphere * 5f;

        debrisTr.gameObject.AddComponent<SofDebris>();
        oldComplex.RemoveComponentRoot(this);
    }
    public virtual void Rearm()
    {

    }
}
public static class SofComponentExtension
{
    public static T AddSofComponent<T>(this GameObject mono, SofModular complex) where T : SofComponent
    {
        T sofComponent = mono.gameObject.AddComponent<T>();
        if (!mono.gameObject.transform.IsChildOf(complex.transform))
        {
            sofComponent.transform.parent = complex.transform;
        }
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
        //base.OnInspectorGUI();
        SofComponent component = (SofComponent)target;
        component.SetReferences();

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
