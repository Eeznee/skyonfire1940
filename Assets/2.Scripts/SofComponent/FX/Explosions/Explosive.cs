using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Explosive", menuName = "Weapons/Explosive")]
public class Explosive : ScriptableObject
{
	public float tntMultiplier = 1f;
	public float fireMultiplier = 1f;
}