using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BoundedAirframe : AirframeBase
{
    public Bounds bounds;

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
    protected void RecalculateBounds()
    {
        MeshCollider mesh = GetComponent<MeshCollider>();
        if (mesh) { bounds = mesh.sharedMesh.bounds; return; }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box) { bounds = new Bounds(box.center,box.size); return; }

        MeshFilter visualMesh = GetComponent<MeshFilter>();
        if (visualMesh) bounds = visualMesh.sharedMesh.bounds;
    }
    protected float ApproximateArea()
    {
        float a = area;
        Vector3 size = bounds.size;
        if (size != Vector3.zero) a = size.z * Mathf.PI / Mathf.Sqrt(2f) * Mathf.Sqrt(size.x * size.x + size.y * size.y);
        return a;
    }
    protected override Quad CreateQuad()
    {
        return CreateQuadBounds(bounds.size.x > bounds.size.y);
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        if (firstTime)
            RecalculateBounds();
        base.Initialize(d, firstTime);
    }
    protected override void FixedUpdate()
    {
        if (aircraft) ExcessDrag();
        base.FixedUpdate();
    }
#if UNITY_EDITOR
    protected override void Draw()
    {
        RecalculateBounds();
        base.Draw();
    }
#endif
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
                Vector3 drag = Aerodynamics.ComputeDrag(velocity, data.tas.Get, data.density.Get, 1f, cd, 1f);
                rb.AddForceAtPosition(drag, transform.position, ForceMode.Force);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoundedAirframe))]
public class BoundedAirframeEditor : AirframeEditor
{
    public override void OnInspectorGUI()
    {
        Color backgroundColor = GUI.backgroundColor;

        base.OnInspectorGUI();

        BoundedAirframe frame = (BoundedAirframe)target;
        frame.CalculateAerofoilStructure();

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Simple Drag Model", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;

        frame.customRipSpeed = EditorGUILayout.Toggle("Custom Rip Speed", frame.customRipSpeed);
        if (frame.customRipSpeed)
            frame.maxSpeed = EditorGUILayout.FloatField(frame.hydraulics ? "Extended Max Speed" : "Max Speed", frame.maxSpeed * 3.6f) / 3.6f;
        frame.drag = EditorGUILayout.Toggle("Has Drag", frame.drag);
        if (frame.drag)
            frame.maxDrag = EditorGUILayout.FloatField(frame.hydraulics ? "Extended Max Drag" : "Max Drag", frame.maxDrag);

        if (frame.customRipSpeed || frame.drag)
            frame.hydraulics = EditorGUILayout.ObjectField("Linked Hydraulics", frame.hydraulics, typeof(HydraulicSystem), true) as HydraulicSystem;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
