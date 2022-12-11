using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Airfoil : Airframe
{
    public SimuFoil[] subdivisions = new SimuFoil[4];

    //1.Main Settings
    public float right = 1f;
    public AirfoilPreset airfoil;
    public AirfoilSection section;
    public float oswald = 0.75f;
    public AirfoilSkin skin;
    public Mesh skinMesh;
    public Airfoil parent;
    public Airfoil child;
    public Airfoil root;

    //Shape
    public Transform shapeTr;
    public float angle = 0f;
    public float tipWidth = 100.0f;
    public float totalArea;
    private Vector3 rootTipLocal;

    //Input
    public float controlSpeed = 5f;

    //2.Control Surface
    public ControlSurface controlSurface;
    public Flap[] flaps;
    public Slat slat;
    public float flapsTotalArea;
    public float flapsTotalMass;

    private TrailRenderer tipTrail;

    public void RootInitialize(Airfoil rootAirfoil)
    {
        root = rootAirfoil;
        airfoil = root.airfoil;
        section = root.section;
        controlSurface = root.controlSurface;
        flaps = root.flaps;
    }
    public override float StructureIntegrity()
    {
        if (skin) return base.StructureIntegrity() * skin.StructureIntegrity();
        return base.StructureIntegrity();
    }
    public override float MaxSpeed()
    {
        if (!aircraft) return base.MaxSpeed();
        return aircraft.maxSpeed * FlightModel.OverSpeedCoeff(parent, child);
    }
    public override float RecalculateArea()
    {
        return Mathf.Abs(shapeTr.localScale.x) * shapeTr.localScale.z * 0.5f * (1f + tipWidth / 100f);
    }
    private void GetReferences()
    {
        shapeTr = FlightModel.AirfoilShapeTransform(transform, shapeTr);
        child = transform.GetChild(0).GetComponent<Airfoil>();
        parent = transform.parent.GetComponent<Airfoil>();
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        GetReferences();

        if (!parent) //Root
        {
            Airfoil[] foils = (child ? child.transform : transform.GetChild(0)).GetComponentsInChildren<Airfoil>();
            foreach (Airfoil foil in foils) foil.RootInitialize(this);
            CalculateAerofoilStructure();
            root = this;
            totalArea = area;
            if (child)
            {
                foreach (Airfoil foil in foils)
                    totalArea += foil.area;
                foreach (Airfoil foil in foils)
                    foil.totalArea = totalArea;
            }
        }
        if (firstTime)
        {
            vital = child;
            maxG = aircraft.maxG * FlightModel.OverGCoeff(parent, child);

            if (skinMesh)
            {
                skin = new GameObject(name + " Skin").AddComponent<AirfoilSkin>();
                skin.transform.SetParent(transform);
                skin.transform.SetPositionAndRotation(transform.position, transform.rotation);
                MeshCollider meshCo = skin.gameObject.AddComponent<MeshCollider>();
                meshCo.sharedMesh = skinMesh;
                meshCo.isTrigger = false;
                meshCo.convex = true;
                skin.material = aircraft.materials.Material(skin);
                skin.gameObject.layer = 9;
                skin.Initialize(data, true);
            }

            if (!child) //Tip
            {
                Vector3 tipPos = shapeTr.TransformPoint(rootTipLocal * 0.55f);
                if (airfoil.tipTrail && controlSurface)
                {
                    tipTrail = Instantiate(airfoil.tipTrail, tipPos, transform.rotation, transform);
                    tipTrail.emitting = false;
                }
                if (aircraft) aircraft.wingSpan = Mathf.Max(transform.root.InverseTransformPoint(tipPos).x * 2f, aircraft.wingSpan);
            }
        }
    }

    public void CalculateAerofoilStructure()
    {
        GetReferences();

        right = Mathv.SignNoZero(shapeTr.root.InverseTransformPoint(shapeTr.position).x);
        float xScale = (right == 0f ? 1f : right) * Mathf.Abs(shapeTr.localScale.x);
        shapeTr.localScale = new Vector3(xScale, 1f, shapeTr.localScale.z);

        rootTipLocal = ((Vector3.right * Mathf.Abs(shapeTr.localScale.x)) + (Vector3.forward * Mathf.Abs(shapeTr.localScale.x) / Mathf.Tan((90 - angle) * Mathf.Deg2Rad))).normalized;

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
        Vector3 rootPos = -Vector3.right * shapeTr.localScale.x * 0.5f;
        area = Mathf.Abs(shapeTr.localScale.x) * shapeTr.localScale.z * 0.5f * (1f + tipWidth / 100f);

        int subs = subdivisions.Length;
        float subX = Mathf.Abs(shapeTr.localScale.x) / subs;

        for (int i = 0; i < subs; i++)
        {
            float pos = shapeTr.localScale.x * (i + 0.5f) / subs;
            Vector3 subPos = rootPos + Vector3.right * pos + Vector3.forward * Mathf.Abs(pos) / Mathf.Tan((90f - angle) * Mathf.Deg2Rad);

            float chord = Mathf.Lerp(shapeTr.localScale.z, shapeTr.localScale.z * tipWidth / 100f, (i + 0.5f) / (subs + 1f));
            float area = subX * chord;  //Z axis
            subdivisions[i].Init(shapeTr, airfoil,section, subPos, area, subX);
            subdivisions[i].AutoSubSurfaces();
        }
        if (child) child.CalculateAerofoilStructure();
    }
    void FixedUpdate()
    {
        Vector3 rootTip = shapeTr.TransformDirection(rootTipLocal);
        //FOR EACH SUBDIVISION
        float alpha = 0f;
        for (int i = 0; i < subdivisions.Length; i++)
            alpha = subdivisions[i].ApplyForces(rootTip);

        Floating();
        ForcesStress(true, true);

        if (tipTrail && aircraft)
        {
            bool emitting = data.ias > 20f && alpha * Mathf.Min(data.ias / 50f, 1f) > airfoil.maxAngle * 0.8f && complex.lod.LOD() <= 2;
            if (tipTrail.emitting != emitting) tipTrail.emitting = emitting;
        }
    }




#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!shapeTr) return;
        GetReferences();
        if (!parent) CalculateAerofoilStructure();

        Vector3 rootLiftPos = shapeTr.position - (shapeTr.right * (shapeTr.localScale.x * 0.5f));
        Vector3 tipLiftPos = rootLiftPos + (shapeTr.right * shapeTr.localScale.x) + (shapeTr.forward * Mathf.Abs(shapeTr.localScale.x) / Mathf.Tan((90 - angle) * Mathf.Deg2Rad));
        Vector3 leadingBot = rootLiftPos + (shapeTr.forward * shapeTr.localScale.z * (1f - Quadrangle.liftLine));
        Vector3 trailingBot = rootLiftPos - (shapeTr.forward * shapeTr.localScale.z * Quadrangle.liftLine);
        Vector3 leadingTop = tipLiftPos + (shapeTr.forward * shapeTr.localScale.z * (1f - Quadrangle.liftLine) * tipWidth / 100f);
        Vector3 trailingTop = tipLiftPos - (shapeTr.forward * shapeTr.localScale.z * Quadrangle.liftLine * tipWidth / 100f);

        int subs = subdivisions.Length;
        for (int i = 0; i < subs; i++)
        {
            SimuFoil sub = subdivisions[i];
            Vector3 csLeadingTop = Vector3.Lerp(leadingBot, leadingTop, (float)(i + 1) / subs);
            Vector3 csLeadingBot = Vector3.Lerp(leadingBot, leadingTop, (float)i / subs);
            Vector3 csTrailingTop = Vector3.Lerp(trailingBot, trailingTop, (float)(i + 1) / subs);
            Vector3 csTrailingBot = Vector3.Lerp(trailingBot, trailingTop, (float)i / subs);

            Color c = Color.white;
            if (sub.control) c = Color.blue;
            else if (sub.flap) c = Color.magenta;
            c.a = 0.06f;
            Features.DrawControlHandles(csLeadingBot, csLeadingTop, csTrailingTop, csTrailingBot, c, sub.slat ? Color.green : Color.yellow);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rootLiftPos, tipLiftPos);
    }
#endif

}
#if UNITY_EDITOR
[CustomEditor(typeof(Airfoil))]
public class AerofoilReEditor : Editor
{
    int toolbarindex = 0;
    Color backgroundColor;

    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        Airfoil airfoil = (Airfoil)target;

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Airfoil Configuration", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;

        airfoil.shapeTr = FlightModel.AirfoilShapeTransform(airfoil.transform, airfoil.shapeTr);
        airfoil.CalculateAerofoilStructure();
        airfoil.skinMesh = EditorGUILayout.ObjectField("Airfoil Skin", airfoil.skinMesh, typeof(Mesh), true) as Mesh;
        if (!airfoil.parent)
        {
            airfoil.airfoil = EditorGUILayout.ObjectField("Airfoil", airfoil.airfoil, typeof(AirfoilPreset), false) as AirfoilPreset;
            airfoil.section = EditorGUILayout.ObjectField("Airfoil Section", airfoil.section, typeof(AirfoilSection), false) as AirfoilSection;
            airfoil.oswald = EditorGUILayout.Slider("Oswald Coef", airfoil.oswald, 0f, 1f);
            EditorGUILayout.LabelField("Wing Section Area", airfoil.totalArea.ToString("0.00") + " m2");
        }
        airfoil.tipWidth = EditorGUILayout.Slider("Tip Width", airfoil.tipWidth, 5f, 100f);
        airfoil.angle = EditorGUILayout.Slider("Angle", airfoil.angle, -50f, 20f);
        EditorGUILayout.LabelField("Airfoil Section Area", airfoil.area.ToString("0.00") + " m2");

        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Damage Model", MessageType.None); //Damage and forces model
        GUI.color = backgroundColor;
        airfoil.material = EditorGUILayout.ObjectField("Material", airfoil.material, typeof(PartMaterial), false) as PartMaterial;
        SerializedProperty ripOnRip = serializedObject.FindProperty("ripOnRip");
        EditorGUILayout.PropertyField(ripOnRip, true);
        airfoil.emptyMass = EditorGUILayout.FloatField("Mass", airfoil.emptyMass);

        GUILayout.Space(20f);
        SerializedProperty subdivisions = serializedObject.FindProperty("subdivisions");
        EditorGUILayout.PropertyField(subdivisions, true);

        if (!airfoil.parent)
        {
            GUILayout.Space(20f);
            GUI.color = Color.cyan;
            toolbarindex = GUILayout.Toolbar(toolbarindex, new string[] { "Main Surface", "Flap", "Slat" });

            switch (toolbarindex)
            {
                //CONTROL SURFACE
                case 0:
                    airfoil.controlSurface = airfoil.GetComponentInChildren<ControlSurface>();
                    if (airfoil.controlSurface)
                    {
                        ControlSurface cs = airfoil.controlSurface;
                        EditorGUILayout.HelpBox("Control Surface", MessageType.None);
                        GUI.color = backgroundColor;
                        cs.emptyMass = EditorGUILayout.FloatField("Mass", cs.emptyMass);
                        airfoil.controlSpeed = EditorGUILayout.FloatField("Control speed", airfoil.controlSpeed);
                        cs.maxDeflection = Mathf.Abs(EditorGUILayout.FloatField("Positive Limit", cs.maxDeflection));
                        cs.minDeflection = Mathf.Abs(EditorGUILayout.FloatField("Negative Limit", -cs.minDeflection));
                        cs.effectiveSpeed = EditorGUILayout.FloatField("Eff Speed Km/h", Mathf.Round(cs.effectiveSpeed * 36f) / 10f) / 3.6f;
                        EditorGUILayout.LabelField("Control Surface Area", cs.miniFoil.mainQuad.area.ToString("0.00") + " m2");
                    }
                    break;

                //FLAPS
                case 1:
                    EditorGUILayout.HelpBox("Flaps", MessageType.None);
                    GUI.color = backgroundColor;
                    airfoil.flaps = airfoil.GetComponentsInChildren<Flap>();
                    airfoil.flapsTotalArea = 0f;
                    airfoil.flapsTotalMass = EditorGUILayout.FloatField("Flaps total mass", airfoil.flapsTotalMass);
                    if (airfoil.flaps.Length > 0f && airfoil.flaps[0])
                    {
                        foreach (Flap flap in airfoil.flaps)
                            if (flap) airfoil.flapsTotalArea += flap.area;

                        Flap refFlap = airfoil.flaps[0];
                        refFlap.extendedRipSpeed = EditorGUILayout.FloatField("Extended Rip Km/h", Mathf.Round(refFlap.extendedRipSpeed * 36f) / 10f) / 3.6f;
                        foreach (Flap flap in airfoil.flaps)
                            if (flap)
                            {
                                flap.extendedRipSpeed = refFlap.extendedRipSpeed;
                                flap.emptyMass = airfoil.flapsTotalMass * flap.area / airfoil.flapsTotalArea;
                            }
                    }
                    EditorGUILayout.LabelField("Total Flaps Area", Mathf.Round(airfoil.flapsTotalArea * 10f) / 10f + " m²");
                    break;

                //SLATS
                case 2:
                    EditorGUILayout.HelpBox("Slats", MessageType.None);
                    GUI.color = backgroundColor;
                    airfoil.slat = airfoil.GetComponentInChildren<Slat>();
                    if (airfoil.slat == null) return;
                    airfoil.slat.emptyMass = EditorGUILayout.FloatField("Mass", airfoil.slat.emptyMass);
                    airfoil.slat.distance = EditorGUILayout.FloatField("Extend Distance", airfoil.slat.distance);
                    airfoil.slat.aoaEffect = EditorGUILayout.FloatField("Effect on Angle Of Attack", airfoil.slat.aoaEffect);
                    airfoil.slat.extendedSpeed = EditorGUILayout.FloatField("Extended Speed", airfoil.slat.extendedSpeed);
                    airfoil.slat.lockedSpeed = EditorGUILayout.FloatField("Locked Speed", airfoil.slat.lockedSpeed);
                    airfoil.slat.straightLockedSpeed = EditorGUILayout.FloatField("Straight Locked Speed", airfoil.slat.straightLockedSpeed);
                    break;
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(airfoil);
            EditorSceneManager.MarkSceneDirty(airfoil.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
