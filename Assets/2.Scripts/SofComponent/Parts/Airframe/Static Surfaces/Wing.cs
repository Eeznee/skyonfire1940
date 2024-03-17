using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Wing : ShapedAirframe
{
    public bool split = false;
    public float splitFraction = 0.5f;
    public Aero splitAero;

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
    public void RootInitialize(Wing rootWing)
    {
        root = rootWing;
        foil = root.foil;
        oswald = root.oswald;
        totalArea = root.totalArea;
    }
    public override void SetReferences(SofComplex _complex)
    {
        child = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Wing>() : null;
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
        base.SetReferences(_complex);

        if (!parent)
        {
            Wing[] wings = GetComponentsInChildren<Wing>();
            root = this;
            totalArea = 0f;
            foreach (Wing wing in wings) totalArea += wing.area;
            foreach (Wing wing in wings) wing.RootInitialize(this); //Do not merge the loops !
        }
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);
    }
    protected override Aero CreateAero()
    {
        return new ComplexAero(this, CreateQuad(), foil);
    }
    public override void CalculateAerofoilStructure()
    {
        base.CalculateAerofoilStructure();
        if (split)
        {
            Quad[] splitQuad = aero.quad.Split(splitFraction);
            aero = new ComplexAero(this, splitQuad[0], foil);
            splitAero = new ComplexAero(this, splitQuad[1], foil);
        }
    }
    protected override void FixedUpdate()
    {
        alpha = aero.ApplyForces();
        if (split) alpha = splitAero.ApplyForces();
    }

#if UNITY_EDITOR
    protected override void Draw()
    {
        Color faceColor = Color.white;
        if (aero.control) faceColor = Color.blue;
        else if (aero.flap) faceColor = Color.magenta;
        faceColor.a = 0.06f;
        aero.quad.Draw(faceColor, aero.slat ? Color.green : Color.yellow, true);
        if (split)
        {
            if (splitAero.control) faceColor = Color.blue;
            else if (splitAero.flap) faceColor = Color.magenta;
            faceColor.a = 0.06f;
            splitAero.quad.Draw(faceColor, aero.slat ? Color.green : Color.yellow, true);
        }
    }
    private void OnValidate()
    {
        CalculateAerofoilStructure();
        SnapToParent();
    }

    public void SnapToParent()
    {
        if (!parent) return;

        shapeTr.localScale = new Vector3(shapeTr.localScale.x, 1f, parent.shapeTr.localScale.z * parent.tipWidth / 100f);
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(parent.shapeTr.localRotation.eulerAngles.x, parent.shapeTr.localRotation.eulerAngles.y, shapeTr.localRotation.eulerAngles.z);
        shapeTr.localRotation = rot;
        Vector3 pos = parent.shapeTr.position;
        pos += parent.shapeTr.right * parent.shapeTr.localScale.x * 0.5f;                                                             //Offset for parent airfoil scale
        pos += shapeTr.localScale.x * shapeTr.right * 0.5f;                                                                           //Offset for airfoil scale
        pos += parent.shapeTr.forward * Mathf.Abs(parent.shapeTr.localScale.x) / Mathf.Tan((90f - parent.angle) * Mathf.Deg2Rad);     //Offset for wing angle
        shapeTr.position = pos;
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

        base.OnInspectorGUI();
    }
}
#endif
