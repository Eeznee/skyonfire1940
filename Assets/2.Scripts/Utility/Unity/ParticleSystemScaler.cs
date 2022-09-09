using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemScaler : MonoBehaviour
{
    public static void ScaleEffect(float input, Transform particle)
    {
        particle.localScale = Vector3.one * input;
        foreach (Transform sub in particle)
        {
            sub.localScale = particle.localScale;
        }
    }
}
