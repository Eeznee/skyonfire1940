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
    private void GetReferences()
    {
        shapeTr = FlightModel.AirfoilShapeTransform(transform, shapeTr);
        child = transform.GetChild(0).GetComponent<Wing>();
        parent = transform.parent ? transform.parent.GetComponent<Wing>() : null;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        GetReferences();
        if (firstTime)
        {
            if (skinMesh) skin = WingSkin.CreateSkin(this, skinMesh);

            if (!parent) //Root
            {
                Wing[] wings = (child ? child.transform : transform.GetChild(0)).GetComponentsInChildren<Wing>();
                CalculateAerofoilStructure();
                root = this;
                totalArea = area;
                foreach (Wing wing in wings) totalArea += wing.area;
                foreach (Wing wing in wings) wing.RootInitialize(this); //Do not merge the loops !
            }
            if (!child) //Tip
            {
                Vector3 tipPos = (split ? splitAero : aero).quad.TopAeroPos(true) + tr.right * 0.1f;
                aircraft.wingSpan = Mathf.Max(transform.root.InverseTransformPoint(tipPos).x * 2f, aircraft.wingSpan);
            }
        }
    }
    public void SnapToParent()
    {
        GetReferences();
        if (parent) //Snap to parent
        {
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
        aero.quad.Draw(faceColor, aero.slat ? Color.green : Color.yellow,true);
        if (split)
        {
            if (splitAero.control) faceColor = Color.blue;
            else if (splitAero.flap) faceColor = Color.magenta;
            faceColor.a = 0.06f;
            splitAero.quad.Draw(faceColor, aero.slat ? Color.green : Color.yellow,true);
        }
    }
    private void OnDrawGizmos()
    {
        SnapToParent();
        CalculateAerofoilStructure();
        Draw();
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Wing))]
public class WingEditor : ShapedAirframeEditor
{
    public override void OnInspectorGUI()
    {
        Wing wing = (Wing)target;
        wing.SnapToParent();
        base.OnInspectorGUI();
        Color backgroundColor = GUI.backgroundColor;

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Wing Configuration", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;

        wing.split = EditorGUILayout.Toggle("Split", wing.split);
        if (wing.split) wing.splitFraction = EditorGUILayout.Slider("Split Fraction", wing.splitFraction, 0f, 1f);

        wing.skinMesh = EditorGUILayout.ObjectField("Skin Mesh", wing.skinMesh, typeof(Mesh), true) as Mesh;
        if (!wing.parent)
        {
            wing.foil = EditorGUILayout.ObjectField("Airfoil", wing.foil, typeof(Airfoil), false) as Airfoil;
            wing.oswald = EditorGUILayout.Slider("Oswald Coef", wing.oswald, 0f, 1f);
            EditorGUILayout.LabelField("Full Wing Area", wing.totalArea.ToString("0.00") + " m2");
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(wing);
            EditorSceneManager.MarkSceneDirty(wing.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
