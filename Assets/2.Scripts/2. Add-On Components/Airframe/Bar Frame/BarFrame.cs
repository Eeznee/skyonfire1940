using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BarFrame : SofFrame
{
    public virtual float HpPerMeter => 45f;
    public override float MaxHp => HpPerMeter * length + minHp;

    const float minHp = 15f;
    public float length { get; private set; }


    const float kgPerMeter = 10f;
    public override float ApproximateMass()
    {
        return length * kgPerMeter;
    }
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);

        Bounds bounds = GetBounds();
        length = BoundsToLength(bounds);
    }
    private float BoundsToLength(Bounds bounds)
    {
        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BarFrame)), CanEditMultipleObjects]
public class BarFrameEditor : FrameEditor
{
    protected override string BasicName()
    {
        return "Bar Frame";
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        BarFrame frame = (BarFrame)target;
        EditorGUILayout.LabelField("Length", frame.length.ToString("0.0") + " m");
    }
}
#endif