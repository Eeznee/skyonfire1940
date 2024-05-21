using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaper : MonoBehaviour
{
    private ShapedAirframe airframe;
    public float angle;
    public float tipWidth;

    private void Start()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Debug.Log(name + "shapin");
    }


}
