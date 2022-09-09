using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectElement))]
public class DragSurface : MonoBehaviour
{
    Part part;
    public float cd = 0f;
    public float animationFactor = 1f;
    void Start()
    {
        part = GetComponent<Part>();
    }

    void FixedUpdate()
    {
        Debug.LogError("Enleve Le Script !!!");
        //Drag
        Vector3 velocity = part.rb.velocity;
        Vector3 drag = Aerodynamics.ComputeDrag(velocity,part.data.tas, part.data.airDensity, 1f, cd * animationFactor,1f);
        part.rb.AddForceAtPosition(drag, transform.position, ForceMode.Force);
    }
}
