using UnityEngine;

public class ObjectElement : MonoBehaviour
{
    //References
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public SofObject sofObject;
    [HideInInspector] public SofSimple simple;
    [HideInInspector] public SofComplex complex;
    [HideInInspector] public SofAircraft aircraft;
    [HideInInspector] public ObjectData data;
    [HideInInspector] public Transform tr;
    public virtual float Mass()
    {
        return 0f;
    }

    public virtual float EmptyMass()
    {
        return 0f;
    }
    public virtual void Initialize(ObjectData d,bool firstTime)
    {
        data = d;
        rb = data.rb;
        sofObject = data.sofObject;
        simple = data.simple;
        complex = data.complex;
        aircraft = data.aircraft;
        tr = transform;
    }
}

