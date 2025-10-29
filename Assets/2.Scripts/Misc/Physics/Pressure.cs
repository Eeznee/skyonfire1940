using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct Pressure
{
    public enum PressureUnit
    {
        [InspectorName("kiloPascal")]
        kPa,
        [InspectorName("bar | ata")]
        bar,
        [InspectorName("Inch of mercury | Hg")]
        Hg,
        [InspectorName("lb per in² | psi")]
        psi,
        [InspectorName("relative lb per in² | +psi")]
        psiDelta,
        [InspectorName("mm of mercury | mmHg")]
        mmHg
    }
    public PressureUnit unitUsed;

    [SerializeField] private float kPa;
    [SerializeField] private float bar;
    [SerializeField] private float hg;
    [SerializeField] private float psi;
    [SerializeField] private float psiDelta;
    [SerializeField] private float mmHg;

    public string UnitString
    {
        get
        {
            switch (unitUsed)
            {
                case PressureUnit.kPa: return "kPa";
                case PressureUnit.bar: return "ata";
                case PressureUnit.Hg: return "Hg";
                case PressureUnit.mmHg: return "mmHg";
                case PressureUnit.psiDelta: return "+psi";
                case PressureUnit.psi: return "psi";
                default: return "pa";
            }
        }
    }
    public float ConvertPAUsingUnit(float pa)
    {
        switch (unitUsed)
        {
            case PressureUnit.kPa: return pa * 0.001f;
            case PressureUnit.bar: return pa /  Aerodynamics.SeaLvlPressure;
            case PressureUnit.Hg: return pa * paToHg;
            case PressureUnit.mmHg: return pa * paToMmHg;
            case PressureUnit.psiDelta: return (pa - Aerodynamics.SeaLvlPressure) * paToPsi;
            case PressureUnit.psi: return pa * paToPsi;
            default: return pa;
        }
    }
    public string CompleteValueWithUnit(float pressure)
    {
        switch (unitUsed)
        {
            case PressureUnit.kPa: return pressure.ToString("0.0") + " kPa";
            case PressureUnit.bar: return pressure.ToString("0.00") + " ata";
            case PressureUnit.Hg: return pressure.ToString("0.0") + " inHg";
            case PressureUnit.mmHg: return pressure.ToString("0") + " mmHg";
            case PressureUnit.psiDelta: return "+" + pressure.ToString("0.00") + " psi";
            case PressureUnit.psi: return pressure.ToString("0.00") + "psi";
            default: return pressure.ToString("0") + " pa";
        }
    }

    public float PressurePa
    {
        get
        {
            switch (unitUsed)
            {
                case PressureUnit.kPa: return kPa * 1000f;
                case PressureUnit.bar: return bar * Aerodynamics.SeaLvlPressure;
                case PressureUnit.Hg: return hg * hgToPa;
                case PressureUnit.mmHg: return mmHg * mmHgToPa;
                case PressureUnit.psiDelta: return psiDelta * psiToPa + Aerodynamics.SeaLvlPressure;
                case PressureUnit.psi: return psi * psiToPa;
                default: return 0f;
            }
        }
    }
    public float PressureKpa => PressurePa * 0.001f;
    public float PressureBar => PressurePa / Aerodynamics.SeaLvlPressure;
    public float PressureHg => PressurePa;
    public float PressurePsi => PressurePa * paToPsi;
    public float PressurePsiDelta => (PressurePa - Aerodynamics.SeaLvlPressure) * paToPsi;
    public float PressureMmHg => PressurePa * paToMmHg;

    public const float paToPsi = 0.000145038f;
    public const float psiToPa = 6894.76f;
    public const float paToMmHg = 0.00750062f;

    public const float paToHg = 0.0002953f;
    public const float hgToPa = 3386.39f;
    public const float mmHgToPa = 133.322f;

    public Pressure(PressureUnit _unitUsed)
    {
        unitUsed = _unitUsed;
        kPa = Aerodynamics.SeaLvlPressure * 0.001f;
        bar = 1f;
        hg = Aerodynamics.SeaLvlPressure * paToHg;
        psi = Aerodynamics.SeaLvlPressure * paToPsi;
        psiDelta = 0f;
        mmHg = Aerodynamics.SeaLvlPressure * paToMmHg;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Pressure))]
public class PressureDrawer : PropertyDrawer
{
    public SerializedProperty CorrectPropertyByUnit(SerializedProperty property, Pressure.PressureUnit unit)
    {
        switch (unit)
        {
            case Pressure.PressureUnit.kPa:
                return property.FindPropertyRelative("kPa");
            case Pressure.PressureUnit.bar:
                return property.FindPropertyRelative("bar");
            case Pressure.PressureUnit.Hg:
                return property.FindPropertyRelative("hg");
            case Pressure.PressureUnit.psi:
                return property.FindPropertyRelative("psi");
            case Pressure.PressureUnit.psiDelta:
                return property.FindPropertyRelative("psiDelta");
            case Pressure.PressureUnit.mmHg:
                return property.FindPropertyRelative("mmHg");
            default: return null;
        }
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        SerializedProperty unitUsed = property.FindPropertyRelative("unitUsed");
        Pressure.PressureUnit unit = (Pressure.PressureUnit)unitUsed.enumValueIndex;
        SerializedProperty value = CorrectPropertyByUnit(property, unit);



        float width = position.width / 2f;
        float height = position.height;
        var valueRect = new Rect(position.x, position.y, width - 2, height);
        var unitRect = new Rect(position.x + width, position.y, width, height);

        EditorGUI.PropertyField(valueRect, value, GUIContent.none);
        EditorGUI.PropertyField(unitRect, unitUsed, GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
#endif