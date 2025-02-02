using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Explosive", menuName = "SOF/Materials/Explosive")]
public class Explosive : ScriptableObject
{
	public float tntMultiplier = 1f;
	public float fireMultiplier = 1f;
}