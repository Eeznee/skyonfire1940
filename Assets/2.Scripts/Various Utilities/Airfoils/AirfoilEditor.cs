using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Syrus.Plugins.ChartEditor;
#endif
public partial class Airfoil : ScriptableObject, IAirfoil
{
#if UNITY_EDITOR
    public Vector2[] liftPlot;
    public Vector2[] dragPlot;
    public Vector2[] liftDragPlot;


    public Vector2[] flapsLiftPlot;
    public Vector2[] flapsDragPlot;
    public Vector2[] flapsLiftDragPlot;

    public FlapsDesign testFlapsDesign;
    public float graphFlapsRatio;

    const int points = 120;
    public const float graphRange = 26f;

    private void GraphUpdate()
    {
        UpdateValues();

        liftPlot = new Vector2[points];
        dragPlot = new Vector2[points];
        flapsLiftDragPlot = new Vector2[points];
        liftDragPlot = new Vector2[points];

        flapsLiftPlot = new Vector2[points];
        flapsDragPlot = new Vector2[points];
        flapsLiftDragPlot = new Vector2[points];

        float increment = graphRange * 2 / points;

        for (int i = 0; i < points; i++)
        {
            float alpha = -graphRange + increment * i;

            Vector2 coeffs = Coefficients(alpha);

            liftPlot[i] = new Vector2(alpha, coeffs.y);
            dragPlot[i] = new Vector2(alpha, coeffs.x * 10f);
            liftDragPlot[i] = new Vector2(alpha, coeffs.y / coeffs.x * 0.05f);
            if (dragPlot[i].y > 2f) dragPlot[i].y = 2f;

            if (testFlapsDesign == null) continue;

            Vector2 flappedCoeffs = ((IAirfoil)this).Coefficients(alpha, testFlapsDesign, graphFlapsRatio);

            flapsLiftPlot[i] = new Vector2(alpha, flappedCoeffs.y);
            flapsDragPlot[i] = new Vector2(alpha, flappedCoeffs.x * 10f);
            flapsLiftDragPlot[i] = new Vector2(alpha, flappedCoeffs.y / flappedCoeffs.x * 0.05f);
            if (flapsDragPlot[i].y > 2f) flapsDragPlot[i].y = 2f;

        }
    }
    void OnValidate()
    {
        GraphUpdate();
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Airfoil))]
public class AirfoilEditor : Editor
{
    SerializedProperty symmetric;
    SerializedProperty zeroCl;
    SerializedProperty maxCl;
    SerializedProperty maxAlpha;
    SerializedProperty minAlpha;
    SerializedProperty minCd;
    SerializedProperty cdGrowth;
    protected void OnEnable()
    {
        symmetric = serializedObject.FindProperty("symmetric");
        zeroCl = serializedObject.FindProperty("zeroCl");
        maxCl = serializedObject.FindProperty("maxCl");
        maxAlpha = serializedObject.FindProperty("maxAlpha");
        minAlpha = serializedObject.FindProperty("minAlpha");
        minCd = serializedObject.FindProperty("minCd");
        cdGrowth = serializedObject.FindProperty("cdGrowth");
    }

    static bool showStats = false;
    public override void OnInspectorGUI()
    {
        Airfoil airfoil = (Airfoil)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(symmetric, new GUIContent("Symmetric"));
        EditorGUILayout.Slider(maxAlpha, 0f, 25f, new GUIContent("Peak Alpha º"));
        GUI.enabled = !symmetric.boolValue;
        EditorGUILayout.Slider(minAlpha, 0f, -25f, new GUIContent("Negative Peak Alpha º"));
        GUI.enabled = true;

        GUILayout.Space(20);

        EditorGUILayout.Slider(maxCl, 0f, 2f, new GUIContent("Lift Coefficient @ peak alpha"));
        GUI.enabled = !symmetric.boolValue;
        EditorGUILayout.Slider(zeroCl, -0.5f, 0.5f, new GUIContent("Lift Coefficient @ zero alpha"));
        GUI.enabled = true;

        GUILayout.Space(20);

        EditorGUILayout.Slider(minCd, 0f, 0.2f, new GUIContent("Drag Coefficient Minimum"));
        EditorGUILayout.Slider(cdGrowth, 0f, 2f, new GUIContent("Drag Coefficient Growth"));



        GUILayout.Space(20);

        showStats = EditorGUILayout.Foldout(showStats, "Stats & Graph", true, EditorStyles.foldoutHeader);
        if (showStats)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Airfoil Quality", "1");
            EditorGUILayout.LabelField("Airfoil Thickness", "1");


            EditorGUILayout.PropertyField(serializedObject.FindProperty("testFlapsDesign"), new GUIContent("Test Flaps Design"));

            if (airfoil.testFlapsDesign != null)
                EditorGUILayout.Slider(serializedObject.FindProperty("graphFlapsRatio"), 0f, 1f, new GUIContent("Flaps Level"));

            ShowGraph(airfoil);

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    static bool showLift = true;
    static bool showDrag =true;
    static bool showLiftDrag = true;

    void ShowGraph(Airfoil airfoil)
    {
        Color c = GUI.color;

        GUI.color = Color.cyan;
        showLift = EditorGUILayout.Toggle("Lift Coefficient",showLift);
        GUI.color = Color.red;
        showDrag = EditorGUILayout.Toggle("Drag Coefficient (x10)",showDrag);
        GUI.color = Color.green;
        showLiftDrag = EditorGUILayout.Toggle("Lift/Drag Coefficient (x0.05)",showLiftDrag);

        GUI.color = c;



        Color transparent = Color.white;
        transparent.a = 0f;
        Color lightGray = Color.gray;
        lightGray.a = 0.2f;

        GUIChartEditor.BeginChart(10f, 150f, 100f, 450f, transparent,
            //GUIChartEditorOptions.ShowLabels("0", Color.white, engine.GraphLabels()),
            GUIChartEditorOptions.ShowAltPeriod(),
            GUIChartEditorOptions.ShowAxes(Color.white),
            GUIChartEditorOptions.ShowGrid(5f, 0.25f, lightGray, Color.white, true),
            GUIChartEditorOptions.ChartBounds(-Airfoil.graphRange, Airfoil.graphRange, -2f, 2f),
            GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft));

        if(showLift) GUIChartEditor.PushLineChart(airfoil.liftPlot, Color.cyan);
        if (showDrag) GUIChartEditor.PushLineChart(airfoil.dragPlot, Color.red);
        if (showLiftDrag) GUIChartEditor.PushLineChart(airfoil.liftDragPlot, Color.green);

        bool showFlaps = airfoil.testFlapsDesign != null && airfoil.graphFlapsRatio != 0f;

        if (showLift && showFlaps) GUIChartEditor.PushLineChart(airfoil.flapsLiftPlot, new Color(0.5f, 0.8f, 0.8f));
        if (showDrag && showFlaps) GUIChartEditor.PushLineChart(airfoil.flapsDragPlot, new Color(0.8f,0.5f,0.5f));
        if (showLiftDrag && showFlaps) GUIChartEditor.PushLineChart(airfoil.flapsLiftDragPlot, new Color(0.5f, 0.8f, 0.5f));

        GUIChartEditor.PushLineChart(new Vector2[] {new Vector2(-Airfoil.graphRange, 2f), new Vector2(Airfoil.graphRange, 2f) }, Color.black);

        GUIChartEditor.EndChart();
    }
}
#endif