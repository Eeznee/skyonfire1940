using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Wing : ShapedAirframe
{
    public bool split = false;
    public float splitFraction = 0.5f;
    public AirfoilSurface splitFoilSurface;

    public float oswald = 0.75f;
    public WingSkin skin;
    public Mesh skinMesh;
    public Wing parent;
    public Wing child;
    public Wing root;
    public float totalArea;

    public float alpha;

    public override float AreaCd() { return area * foil.airfoilSim.minCd; }

    public override float MaxSpd()
    {
        float coeff = 1f;
        if (!parent) coeff += 0.1f;
        if (child) coeff += 0.1f;
        return aircraft.maxSpeed * coeff;
    }
    public override float MaxG()
    {
        float coeff = 1f;
        if (parent) coeff += 0.15f;
        if (!child) coeff += 0.15f;
        return aircraft.maxG * coeff;
    }
    public override float StructureIntegrity()
    {
        if (skin) return base.StructureIntegrity() * skin.StructureIntegrity();
        return base.StructureIntegrity();
    }
    public override void SetReferences(SofComplex _complex)
    {
        child = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Wing>() : null;
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
        base.SetReferences(_complex);
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);
    }
    public void CopyRootValues(Wing rootWing)
    {
        root = rootWing;
        foil = root.foil;
        oswald = root.oswald;
        totalArea = root.totalArea;
    }
    protected override AirfoilSurface CreateFoilSurface()
    {
        return new ComplexAirfoilSurface(this, CreateQuad(), foil);
    }
    public override void UpdateAerofoil()
    {
        if (!parent)
        {
            Wing[] wings = GetComponentsInChildren<Wing>();
            root = this;
            totalArea = 0f;
            foreach (Wing wing in wings) totalArea += wing.area;
            foreach (Wing wing in wings) wing.CopyRootValues(this); //Do not merge the loops !
        }

        base.UpdateAerofoil();
        if (split)
        {
            Quad[] splitQuad = foilSurface.quad.Split(splitFraction);
            foilSurface = new ComplexAirfoilSurface(this, splitQuad[0], foil);
            splitFoilSurface = new ComplexAirfoilSurface(this, splitQuad[1], foil);
        }
    }
    protected override void FixedUpdate()
    {
        alpha = foilSurface.ApplyForces();
        if (split) splitFoilSurface.ApplyForces();
    }

#if UNITY_EDITOR
    private Color AirfoilSurfaceColor(AirfoilSurface surface)
    {
        if (surface.control) return aileronColor;
        if (surface.flap) return flapColor;
        return Vector4.zero;
    }
    public override void Draw()
    {
        foilSurface.quad.Draw(AirfoilSurfaceColor(foilSurface), foilSurface.slat ? Color.green : bordersColor, true);
        if (split)
            splitFoilSurface.quad.Draw(AirfoilSurfaceColor(splitFoilSurface), splitFoilSurface.slat ? Color.green : bordersColor, true);
    }
    public void RecursiveSnap()
    {
        if (parent)
        {
            bool snapAffectedShape = shape.SnapTo(parent);
            if (snapAffectedShape)
                UpdateAerofoil();
        }

        if (child) child.RecursiveSnap();
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Wing)), CanEditMultipleObjects]
public class WingEditor : ShapedAirframeEditor
{
    SerializedProperty foil;
    SerializedProperty oswald;
    SerializedProperty split;
    SerializedProperty splitFraction;
    SerializedProperty skinMesh;

    protected override void OnEnable()
    {
        base.OnEnable();
        foil = serializedObject.FindProperty("foil");
        oswald = serializedObject.FindProperty("oswald");
        split = serializedObject.FindProperty("split");
        splitFraction = serializedObject.FindProperty("splitFraction");
        skinMesh = serializedObject.FindProperty("skinMesh");
    }
    private float lastTime;
    protected override void OnSceneGUI()
    {
        base.OnSceneGUI();

        //This is required because for some reason OnSceneGUI is called multiple times per frame which leads to terrible performances
        float newTime = Time.time;
        if (newTime == lastTime) return;
        lastTime = newTime;

        Wing wing = (Wing)target;

        wing.root.RecursiveSnap();
        base.OnSceneGUI();
    }
    protected override void BasicFoldout()
    {
        EditorGUILayout.PropertyField(skinMesh, new GUIContent("Skin Collider"));
        base.BasicFoldout();
        Wing wing = (Wing)target;
        EditorGUILayout.LabelField("Full Wing Area", wing.totalArea.ToString("0.00") + " m2");
    }
    protected override void ShapeFoldout()
    {
        Wing wing = (Wing)target;

        base.ShapeFoldout();

        EditorGUILayout.PropertyField(split);
        if (wing.split) EditorGUILayout.Slider(splitFraction, 0f, 1f);
    }
    static bool showFoil = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Wing wing = (Wing)target;

        if (!wing.parent)
        {
            showFoil = EditorGUILayout.Foldout(showFoil, "Wing", true, EditorStyles.foldoutHeader);
            if (showFoil)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(foil);
                EditorGUILayout.Slider(oswald, 0.3f, 1f);

                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
#endif
