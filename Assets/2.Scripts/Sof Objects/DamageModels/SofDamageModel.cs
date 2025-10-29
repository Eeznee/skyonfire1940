using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SofObject))]
public abstract class SofDamageModel : MonoBehaviour
{
    public SofObject sofObject { get; private set; }
    public Rigidbody rb => sofObject.rb;

    public virtual float RaycastDistanceExtension => 0f;


    protected virtual void Start()
    {
        sofObject = GetComponent<SofObject>();
    }


    public abstract void Explosion(Vector3 center, float tnt);
    public abstract HitResult ProjectileRaycast(Vector3 position, Vector3 velocity, ProjectileChart chart);
}


