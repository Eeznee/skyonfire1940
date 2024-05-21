using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SofPart : SofComponent       //Parts are Object Elements with mass
{
    public float mass;

    public float Mass()
    {
        return EmptyMass() + AdditionalMass();
    }
    public virtual float AdditionalMass()
    {
        return 0f;
    }
    public virtual float EmptyMass()
    {
        return mass;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofPart))]
public class PartEditor : Editor
{
    SerializedProperty mass;
    protected virtual void OnEnable()
    {
        mass = serializedObject.FindProperty("mass");
    }
    protected bool ShowMass() { return true; }
    protected virtual string BasicName() { return "Part"; }

    static bool showBasic = true;

    protected virtual void BasicFoldout()
    {
        if (ShowMass()) EditorGUILayout.PropertyField(mass);
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
