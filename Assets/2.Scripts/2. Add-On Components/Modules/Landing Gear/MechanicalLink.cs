using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MechanicalLink : SofComponent
{
    [System.Serializable]
    public struct Link
    {
        public Transform tr;
        public Vector3 jointPivot;
        [HideInInspector] public float length;

        public Vector3 PivotPos => tr.TransformPoint(jointPivot);
        public Vector3 PivotDir => tr.parent.TransformDirection(jointPivot);
        public Vector3 pos => tr.position;

        public void RotateToIntersection(Vector3 intersection, Vector3 normal)
        {
            tr.localRotation = Quaternion.identity;
            Vector3 fromC2ToIntersection = intersection - tr.position;
            float angle = Vector3.SignedAngle(fromC2ToIntersection, PivotDir, normal);
            tr.rotation = Quaternion.AngleAxis(-angle, normal) * tr.parent.rotation;
        }
    }
    public override void SetReferences(SofModular _modular)
    {
       if(aircraft) aircraft.OnUpdateLOD0 -= MechanicalAnimation;
        base.SetReferences(_modular);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        PrecomputeValues();

        aircraft.OnUpdateLOD0 += MechanicalAnimation;
    }

#if UNITY_EDITOR
    private void Update()
    {
        PrecomputeValues();
        MechanicalAnimation();
    }
#endif

    public virtual void PrecomputeValues()
    {

    }
    public abstract void MechanicalAnimation();
}
