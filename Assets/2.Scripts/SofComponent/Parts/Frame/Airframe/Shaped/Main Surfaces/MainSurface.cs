using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class MainSurface : ShapedAirframe
{
    public override float AreaCd() { return area * Airfoil.MinCD(); }



    public Airfoil airfoil;

    public ComplexAeroSurface aeroSurface;

    public virtual void CreateAeroSurface()
    {
        aeroSurface = new ComplexAeroSurface(this, quad);
    }
    public override Vector2 Coefficients(float angleOfAttack)
    {
        return aeroSurface.Coefficients(angleOfAttack);
    }
    public override void UpdateAerofoil()
    {
        base.UpdateAerofoil();
        CreateAeroSurface();
    }

    public override IAirfoil Airfoil
    {
        get
        {
            if (airfoil) return airfoil;
            Debug.Log(aircraft.name + "  " + name + " does not have an airfoil assigned");
            return new SimpleAirfoil(1f, 0.1f, 0f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainSurface)), CanEditMultipleObjects]
public class MainSurfaceEditor : ShapedAirframeEditor
{
    SerializedProperty airfoil;

    protected override void OnEnable()
    {
        base.OnEnable();
        airfoil = serializedObject.FindProperty("airfoil");;
    }

    protected virtual void AirfoilFoldout()
    {
        EditorGUILayout.PropertyField(airfoil);
    }
    static bool showAirfoil = true;


    protected virtual bool ShowAirfoilFoldout() { return true; }



    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (ShowAirfoilFoldout())
        {
            showAirfoil = EditorGUILayout.Foldout(showAirfoil, "Airfoil", true, EditorStyles.foldoutHeader);
            if (showAirfoil)
            {
                EditorGUI.indentLevel++;
                AirfoilFoldout();
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
#endif
