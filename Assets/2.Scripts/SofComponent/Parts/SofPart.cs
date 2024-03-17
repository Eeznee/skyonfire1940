using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SofPart : SofComponent       //Parts are Object Elements with mass
{
    public float emptyMass = 0f;
    protected Vector3 localPos;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        localPos = complex.transform.InverseTransformPoint(tr.position);
    }
    public virtual float Mass()
    {
        return emptyMass;
    }
    public virtual float EmptyMass()
    {
        return emptyMass;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofPart))]
public class PartEditor : Editor
{
    SerializedProperty emptyMass;
    protected virtual void OnEnable()
    {
        emptyMass = serializedObject.FindProperty("emptyMass");
    }
    protected bool ShowMass() { return true; }
    protected virtual string BasicName() { return "Part"; }

    static bool showBasic = true;

    protected virtual void BasicFoldout()
    {
        if (ShowMass()) EditorGUILayout.PropertyField(emptyMass);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showBasic = EditorGUILayout.Foldout(showBasic, BasicName(), true, EditorStyles.foldoutHeader);
        if (showBasic)
        {
            EditorGUI.indentLevel++;
            BasicFoldout();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
