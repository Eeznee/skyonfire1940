using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRange : MonoBehaviour
{
    public float maxRange;
    public GameObject[] concerned;

    bool active;
    float maxRangeComp;
    void Start()
    {
        active = true;
        maxRangeComp = maxRange * maxRange;
    }

    void Update()
    {
        if (!Camera.main) return;
        float dis = (transform.position- Camera.main.transform.position).sqrMagnitude;
        bool inRange = dis < maxRangeComp;
        if (inRange != active)
        {
            active = inRange;
            for(int i = 0; i < concerned.Length; i++)
            {
                concerned[i].SetActive(active);
            }
        }
    }
}
