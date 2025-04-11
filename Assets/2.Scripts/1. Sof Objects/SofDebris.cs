using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofDebris : SofComplex
{
    private void Awake()
    {
        GameInitialization();
    }
    private void Start()
    {
        
    }

    protected override void GameInitialization()
    {
        base.GameInitialization();

        foreach (LineRenderer rope in GetComponentsInChildren<LineRenderer>()) rope.enabled = false;
        if (!GetComponentInChildren<Bomb>()) Destroy(gameObject, 25f);

        if(airframes.Count == 0)
        {
            RandomAerodynamics randomAero = gameObject.AddSofComponent<RandomAerodynamics>(this);
            float area = Mathf.Sqrt(GetMass()) * 0.2f;
            float maxOffset = 0.5f;
            randomAero.SetValues(area,maxOffset);
        }
    }
    public override int DefaultLayer()
    {
        return 8;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        WaterPhysics();
    }

}
