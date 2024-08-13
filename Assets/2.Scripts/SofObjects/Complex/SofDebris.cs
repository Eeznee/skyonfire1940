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
    }
    public override int DefaultLayer()
    {
        return 8;
    }
    private void FixedUpdate()
    {
        WaterPhysics();
    }

}
