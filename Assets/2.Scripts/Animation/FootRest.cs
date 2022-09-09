using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootRest : MonoBehaviour
{
    public void SetFootPose(Animator crew,AvatarIKGoal foot)
    {
        crew.SetIKPositionWeight(foot, 1f);
        crew.SetIKRotationWeight(foot, 1f);
        crew.SetIKPosition(foot, transform.position);
        crew.SetIKRotation(foot, transform.rotation);
    }
}
