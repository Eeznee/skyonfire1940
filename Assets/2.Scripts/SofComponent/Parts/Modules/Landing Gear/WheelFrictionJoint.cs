using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelFrictionJoint : MonoBehaviour
{
    CustomWheel wheel;
    ConfigurableJoint joint;
    Rigidbody rb;

    public static WheelFrictionJoint CreateJoint(CustomWheel wheel)
    {
        WheelFrictionJoint joint = new GameObject(wheel.name + " Friction Joint").AddComponent<WheelFrictionJoint>();
        joint.Initialize(wheel);

        return joint;
    }

    public void Initialize(CustomWheel _wheel)
    {
        wheel = _wheel;

        rb = gameObject.AddComponent<Rigidbody>();
        joint = gameObject.AddComponent<ConfigurableJoint>();
        transform.position = wheel.tr.position + Vector3.down * wheel.radius + rb.velocity * Time.fixedDeltaTime;

        wheel = _wheel;

        rb.useGravity = false;
        rb.isKinematic = true;

        joint.connectedBody = wheel.rb;
        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
    }

    private void FixedUpdate()
    {
        rb.position = wheel.tr.position + Vector3.down * wheel.radius + rb.velocity * Time.fixedDeltaTime;
    }
}
