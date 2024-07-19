using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SofPart : SofComponent
{
    [SerializeField] private float mass;

    public void SetCustomMass(float newMass)
    {
        if (NoCustomMass) Debug.LogError("This SofPart does not use the custom mass",this);
        mass = newMass;
    }
    public float GetCustomMass()
    {
        if (NoCustomMass) Debug.LogError("This SofPart does not use the custom mass",this);
        return mass;
    }

    public float Mass => EmptyMass + AdditionalMass;
    public virtual float AdditionalMass => 0f;
    public virtual float EmptyMass => mass;
    public virtual bool NoCustomMass => false;
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofPart))]
public class PartEditor : Editor
{
    SerializedProperty mass;
    protected virtual void OnEnable()
    {
        mass = serializedObject.FindProperty("mass");
        SofPart part = (SofPart)target;

        part.SetReferences();
    }
    protected virtual string BasicName() { return "Part"; }

    static bool showBasic = true;

    protected virtual void BasicFoldout()
    {
        SofPart part = (SofPart)target;
        if (part.NoCustomMass)
        {
            EditorGUILayout.LabelField("Empty Mass", part.EmptyMass.ToString("0.0") + " kg");
            EditorGUILayout.LabelField("Loaded Mass", part.Mass.ToString("0.0") + " kg");
        } else EditorGUILayout.PropertyField(mass);
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
