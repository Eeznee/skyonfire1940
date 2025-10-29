using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Runway : MonoBehaviour
{
    public bool bothWays = false;
    public float spawnArea = 0.1f;


    public void Spawn(int amount, bool opposite)
    {

    }
#if UNITY_EDITOR

    public void OnDrawGizmos()
    {
        Color c = Color.red;
        c.a = 0.06f;

        Vector3 rightSide = transform.position + transform.right * transform.localScale.x / 2f;
        Vector3 leftSide = rightSide - transform.right * transform.localScale.x;

        Vector3 upRight = rightSide + transform.forward * transform.localScale.z / 2f;
        Vector3 downRight = upRight - transform.forward * transform.localScale.z;
        Vector3 upLeft = leftSide + transform.forward * transform.localScale.z / 2f;
        Vector3 downLeft = upLeft - transform.forward * transform.localScale.z;

        DrawControlHandles(upRight, downRight, downLeft, upLeft, c, Color.magenta);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.Lerp(downRight, upRight,spawnArea), Vector3.Lerp(downLeft, upLeft, spawnArea));

        if (!bothWays) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.Lerp(downRight, upRight, 1f-spawnArea), Vector3.Lerp(downLeft, upLeft, 1f-spawnArea));
    }
    void DrawControlHandles(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color face, Color outline)
    {
        Vector3[] v = new Vector3[4];
        v[0] = A;
        v[1] = B;
        v[2] = C;
        v[3] = D;
        Handles.DrawSolidRectangleWithOutline(v, face, outline);
    }

#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(Runway))]
public class RunWayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Runway runway = (Runway)target;
        EditorGUILayout.LabelField("Use transform scale");
        runway.spawnArea = EditorGUILayout.Slider("Spawn Area Coeff", runway.spawnArea, 0.01f, 0.5f);
        runway.bothWays = EditorGUILayout.Toggle("Use Both Ways", runway.bothWays);
    }
}
#endif