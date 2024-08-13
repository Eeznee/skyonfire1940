using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BoundedAirframe : SofAirframe
{
    public override float MaxHp => area * ModulesHPData.frameHpPerSq;

    public Bounds bounds { get; private set; }

    public HydraulicSystem hydraulics;
    public bool customRipSpeed = false;
    public bool drag = false;
    public float maxSpeed = 100f;
    public float maxDrag = 0.1f;

    public override float MaxSpd()
    {
        if (customRipSpeed)
            return hydraulics ? Mathf.Lerp(base.MaxSpd(), maxSpeed, hydraulics.state) : maxSpeed;
        else
            return base.MaxSpd();
    }

    protected override AirfoilSurface CreateFoilSurface() { return new AirfoilSurface(this, CreateQuad(), foil); }

    protected Quad CreateQuadBounds(bool flat)
    {
        Vector3 forward = bounds.size.z * Vector3.forward * 0.5f;
        Vector3 side = (flat ? bounds.size.x * Vector3.right : bounds.size.y * Vector3.up) * 0.5f;

        Vector3 lt = bounds.center + forward + side;
        Vector3 lb = bounds.center + forward - side;
        Vector3 tt = bounds.center - forward + side;
        Vector3 tb = bounds.center - forward - side;

        return new Quad(transform, lt, lb, tt, tb);
    }
    protected override Quad CreateQuad()
    {
        return CreateQuadBounds(bounds.size.x > bounds.size.y);
    }
    private void RecalculateBounds()
    {
        MeshCollider mesh = GetComponent<MeshCollider>();
        if (mesh && mesh.sharedMesh) { bounds = mesh.sharedMesh.bounds; return; }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box) { bounds = new Bounds(box.center, box.size); return; }

        MeshFilter visualMesh = GetComponent<MeshFilter>();
        if (visualMesh && visualMesh.sharedMesh) { bounds = visualMesh.sharedMesh.bounds; return; }

        bounds = new Bounds(Vector3.zero, Vector3.zero);
    }
    public override void UpdateAerofoil()
    {
        RecalculateBounds();
        base.UpdateAerofoil();
    }
    protected override void FixedUpdate()
    {
        if (aircraft) ExcessDrag();
        base.FixedUpdate();
    }
    const float maxCd = 1.5f;
    public Vector2 SimplifiedCoefficients(float alpha)
    {
        float thickness = Mathf.Min(bounds.size.y, bounds.size.x);
        float minCd = maxCd * thickness / bounds.size.z;
        float maxCl = 1f;
        return Aerodynamics.SimpleCoefficients(alpha,maxCl,minCd,maxCd);
    }
    public void ExcessDrag()
    {
        if (drag)
        {
            float cd = hydraulics ? maxDrag * hydraulics.state : maxDrag;
            if (cd > 0f)
            {
                Vector3 velocity = rb.velocity;
                Vector3 drag = Aerodynamics.Drag(velocity, data.tas.Get, data.density.Get, 1f, cd, 1f);
                rb.AddForceAtPosition(drag, transform.position, ForceMode.Force);
            }
        }
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        foilSurface.quad.Draw(new Color(), Color.yellow, false);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoundedAirframe)), CanEditMultipleObjects]
public class BoundedAirframeEditor : AirframeEditor
{
    SerializedProperty ripSpeed;
    SerializedProperty maxSpeed;

    SerializedProperty drag;
    SerializedProperty maxDrag;

    SerializedProperty hydraulics;
    protected override void OnEnable()
    {
        base.OnEnable();
        ripSpeed = serializedObject.FindProperty("customRipSpeed");
        maxSpeed = serializedObject.FindProperty("maxSpeed");
        drag = serializedObject.FindProperty("drag");
        maxDrag = serializedObject.FindProperty("maxDrag");

        hydraulics = serializedObject.FindProperty("hydraulics");
    }
    static bool showDragModel = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        BoundedAirframe frame = (BoundedAirframe)target;

        showDragModel = EditorGUILayout.Foldout(showDragModel, "Simple Drag", true, EditorStyles.foldoutHeader);
        if (showDragModel)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(ripSpeed);
            if (frame.customRipSpeed)
                EditorGUILayout.PropertyField(maxSpeed, new GUIContent(frame.hydraulics ? "Extended Max Speed" : "Max Speed"));

            EditorGUILayout.PropertyField(drag);
            if (frame.drag)
                EditorGUILayout.PropertyField(maxDrag);

            if (frame.customRipSpeed || frame.drag)
                EditorGUILayout.PropertyField(hydraulics);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
