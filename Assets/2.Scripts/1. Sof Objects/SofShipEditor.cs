using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(SofShip))]
public class SofShipEditor : SofModularEditor
{
    SerializedProperty width;
    SerializedProperty heightAboveWater;
    SerializedProperty seperateSections;
    SerializedProperty midBowPortion;
    SerializedProperty midSternPortion;

    SerializedProperty armorPlatingmm;
    SerializedProperty projectileHp;
    SerializedProperty maxTntKgCharge;

    SerializedProperty sinkingTime;
    SerializedProperty equilibrumRoll;
    SerializedProperty equilibrumPitch;
    SerializedProperty spring;
    SerializedProperty damper;

    SerializedProperty ammoCanDetonate;
    SerializedProperty ammoDetonationChance;
    SerializedProperty ammoDetonationInstantChance;
    SerializedProperty ammoDetonationCharge;

    SerializedProperty canCatchFire;
    SerializedProperty chanceToCatchFire;
    SerializedProperty fireFX;

    protected override void OnEnable()
    {
        width = serializedObject.FindProperty("width");
        heightAboveWater = serializedObject.FindProperty("heightAboveWater");
        midBowPortion = serializedObject.FindProperty("midBowPortion");
        midSternPortion = serializedObject.FindProperty("midSternPortion");
        seperateSections = serializedObject.FindProperty("seperateSections");

        armorPlatingmm = serializedObject.FindProperty("armorPlatingmm");
        projectileHp = serializedObject.FindProperty("projectileHp");
        maxTntKgCharge = serializedObject.FindProperty("maxTntKgCharge");

        sinkingTime = serializedObject.FindProperty("sinkingTime");
        equilibrumRoll = serializedObject.FindProperty("equilibrumRoll");
        equilibrumPitch = serializedObject.FindProperty("equilibrumPitch");
        spring = serializedObject.FindProperty("spring");
        damper = serializedObject.FindProperty("damper");

        ammoCanDetonate = serializedObject.FindProperty("ammoCanDetonate");
        ammoDetonationChance = serializedObject.FindProperty("ammoDetonationChance");
        ammoDetonationInstantChance = serializedObject.FindProperty("ammoDetonationInstantChance");
        ammoDetonationCharge = serializedObject.FindProperty("ammoDetonationCharge");

        canCatchFire = serializedObject.FindProperty("canCatchFire");
        chanceToCatchFire = serializedObject.FindProperty("chanceToCatchFire");
        fireFX = serializedObject.FindProperty("fireFX");

        base.OnEnable();
    }

    static bool showDimensions = true;
    static bool showDamageModel = true;
    static bool showBuoyancy = true;
    static bool showDestructionEvents = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SofShip ship = (SofShip)target;

        showDimensions = EditorGUILayout.Foldout(showDimensions, "Ship Dimensions", true, EditorStyles.foldoutHeader);
        if (showDimensions)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(width);
            EditorGUILayout.PropertyField(heightAboveWater);

            
            EditorGUILayout.PropertyField(seperateSections);
            if (ship.seperateSections)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(midBowPortion);
                EditorGUILayout.PropertyField(midSternPortion);
                if (ship.midBowPortion < ship.midSternPortion) ship.midBowPortion = ship.midSternPortion;
                EditorGUILayout.MinMaxSlider(new GUIContent("Mid Section Borders"),ref ship.midSternPortion, ref ship.midBowPortion, 0f, 1f);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
        showDamageModel = EditorGUILayout.Foldout(showDamageModel, "Buoyancy", true, EditorStyles.foldoutHeader);
        if (showBuoyancy)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(sinkingTime);
            EditorGUILayout.Slider(equilibrumPitch, -90f, 90f);
            EditorGUILayout.Slider(equilibrumRoll, -180f, 180f);
            EditorGUILayout.PropertyField(spring);
            EditorGUILayout.PropertyField(damper);

            EditorGUI.indentLevel--;
        }

        showDamageModel = EditorGUILayout.Foldout(showDamageModel, "Damage Model", true, EditorStyles.foldoutHeader);
        if (showDamageModel)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("Reminder that projectile HP is proportional to kinetic energy. 30 cal => 3 hp. 20 mm => 80 hp. 40 mm => 1600 hp", MessageType.Info);
            EditorGUILayout.PropertyField(projectileHp,new GUIContent("Projectile HP"));
            EditorGUILayout.PropertyField(maxTntKgCharge, new GUIContent("Max TNT charge kg"));
            EditorGUILayout.PropertyField(armorPlatingmm, new GUIContent("Armor Plating mm"));

            EditorGUI.indentLevel--;
        }

        showDestructionEvents = EditorGUILayout.Foldout(showDestructionEvents, "Destruction Events", true, EditorStyles.foldoutHeader);
        if (showDestructionEvents)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(ammoCanDetonate, new GUIContent("Ammo Can Detonate"));
            if (ship.ammoCanDetonate)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(ammoDetonationChance, 0f, 1f);
                EditorGUILayout.Slider(ammoDetonationInstantChance, 0f, 1f);
                EditorGUILayout.PropertyField(ammoDetonationCharge, new GUIContent("Explosive Charge"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(canCatchFire, new GUIContent("Can Catch Fire"));
            if (ship.canCatchFire)
            {
                EditorGUILayout.Slider(chanceToCatchFire, 0f, 1f);
                EditorGUILayout.PropertyField(fireFX);
            }


            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif