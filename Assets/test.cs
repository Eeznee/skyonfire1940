using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public CrewMember target;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Player.SetCrew(target);
        }
    }
}
