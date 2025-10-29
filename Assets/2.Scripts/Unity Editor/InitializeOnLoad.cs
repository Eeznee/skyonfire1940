
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;

[InitializeOnLoad]
public class InitializeOnLoad
{
    static InitializeOnLoad()
    {
        StaticReferences.Instance.defaultAircrafts.UpdateCards();
    }
}
#endif