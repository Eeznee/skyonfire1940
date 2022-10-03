using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[System.Serializable]
public class Quadrangle
{
    public Quadrangle(Vector3 lt, Vector3 lb,Vector3 tt,Vector3 tb)
    {
        leadingTop = lt;
        leadingBot = lb;
        trailingTop = tt;
        trailingBot = tb;
        area = Mathv.SquareArea(leadingBot, leadingTop, trailingTop, trailingBot);
    }
    public const float liftLine = 0.75f;
    public Vector3 leadingTop;
    public Vector3 leadingBot;
    public Vector3 trailingTop;
    public Vector3 trailingBot;
    public float area;

    public Vector3 BotLiftPos { get { return Vector3.Lerp(trailingBot, leadingBot, liftLine); } }
    public Vector3 TopLiftPos { get { return Vector3.Lerp(trailingTop, leadingTop, liftLine); } }
    public Vector3 AerodynamicCenter { get { return (BotLiftPos + TopLiftPos) * 0.5f; } }
}

[System.Serializable]
public class MiniAirfoil
{
    public AirfoilPreset airfoil;

    //Data
    public Transform tr;
    public Quadrangle mainQuad;
    public Vector3 rootLiftPos = Vector3.zero;
    public Vector3 tipLiftPos = Vector3.zero;
    public void Init(Transform transform)
    {
        tr = FlightModel.AirfoilShapeTransform(transform, tr);
        rootLiftPos = tr.position - (tr.right * (tr.localScale.x * 0.5f));
        tipLiftPos = rootLiftPos + (tr.right * tr.localScale.x);
        Vector3 lt = tipLiftPos + (tr.forward * tr.localScale.z * (1f - Quadrangle.liftLine));
        Vector3 lb = rootLiftPos + (tr.forward * tr.localScale.z * (1f - Quadrangle.liftLine));
        Vector3 tt = tipLiftPos - (tr.forward * tr.localScale.z * Quadrangle.liftLine);
        Vector3 tb = rootLiftPos - (tr.forward * tr.localScale.z * Quadrangle.liftLine);
        mainQuad = new Quadrangle(lt, lb, tt, tb);
    }
    public void ApplyForces(Part part)
    {
        Vector3 velocity = part.rb.GetPointVelocity(tr.position);
        float alpha = Vector3.SignedAngle(tr.forward, velocity.normalized, tr.right);//  * Mathv.SignNoZero(tr.root.InverseTransformPoint(tr.position).x);

        float density = (tr.position.y < 0f) ? 40f : part.data.airDensity;

        float cl = airfoil ? airfoil.Cl(alpha) : SimplifiedAirfoil.ClSymmetric(alpha);
        float cd = airfoil ? airfoil.Cd(alpha) : SimplifiedAirfoil.CdSymmetric(alpha);
        Vector3 liftForce = Aerodynamics.ComputeLift(velocity, part.data.tas,tr.right, density, mainQuad.area, cl, part.structureDamage);
        Vector3 dragForce = Aerodynamics.ComputeDrag(velocity, part.data.tas,density, mainQuad.area, cd, part.structureDamage);
        //Apply forces
        part.rb.AddForceAtPosition(liftForce + dragForce, tr.position, ForceMode.Force);
    }
}
[System.Serializable]
public class SimuFoil
{
    private Transform tr;
    private AirfoilPreset airfoil;
    private Airframe frame;
    private Airfoil wing;

    [HideInInspector] public ControlSurface control;
    [HideInInspector] public Flap flap;
    [HideInInspector] public Slat slat;

    [HideInInspector] public Vector3 pos;
    [HideInInspector] public float area;
    [HideInInspector] public float span;
    [HideInInspector] public float length;
    [HideInInspector] public float controlSqrt;
    private float totalArea;

    public float PercentageSubsurface(MiniAirfoil subFoil)
    {
        Vector3 subPos = tr.InverseTransformDirection(subFoil.tr.position - tr.position);
        float minSub = subPos.x - Mathf.Abs(subFoil.tr.localScale.x) / 2f;
        float maxSub = subPos.x + Mathf.Abs(subFoil.tr.localScale.x) / 2f;
        float min = pos.x - span / 2f;
        float max = pos.x + span / 2f;
        minSub = Mathf.Max(minSub, min);
        maxSub = Mathf.Min(maxSub, max);
        return Mathf.Clamp01((maxSub - minSub) / (max - min));
    }
    public void Init(Transform _tr, AirfoilPreset _foil, Vector3 _pos, float _area, float _span)
    {
        tr = _tr;
        frame = tr.parent.GetComponent<Airframe>();
        airfoil = _foil;

        pos = _pos;
        area = _area;
        span = _span;
        length = area / span;

        controlSqrt = control ? Mathf.Sqrt(control.miniFoil.tr.localScale.z / tr.localScale.z) : 0f;

        wing = frame.GetComponent<Airfoil>();
        totalArea = wing ? wing.totalArea : frame.area;
        if (wing && wing.skin) frame = wing.skin;
    }

    public void AutoSubSurfaces()
    {
        slat = null; flap = null; control = null;
        foreach (Slat s in tr.parent.GetComponentsInChildren<Slat>()) if (PercentageSubsurface(s.miniFoil) > 0.5f) slat = s;
        foreach (Flap f in tr.parent.GetComponentsInChildren<Flap>()) if (PercentageSubsurface(f.miniFoil) > 0.5f) flap = f;
        foreach (ControlSurface c in tr.parent.GetComponentsInChildren<ControlSurface>()) if (PercentageSubsurface(c.miniFoil) > 0.5f) control = c;
    }

    public float ApplyForces(Vector3 rootTip)
    {
        if (controlSqrt == 0f && control) controlSqrt = Mathf.Sqrt(control.miniFoil.tr.localScale.z / length);
        float flapsInput = frame.aircraft ? frame.aircraft.flapsInput : 0f;

        Vector3 center = Airfoil.TransformPointUnscaled(tr, pos);
        Vector3 velocity = frame.rb.GetPointVelocity(center);
        float alpha =  Vector3.SignedAngle(tr.forward, velocity, tr.right);

        //Coefficient of Lift
        if (slat && !slat.ripped) alpha -= slat.extend * slat.aoaEffect * Mathf.InverseLerp(15f, 15f + slat.aoaEffect * 2f, alpha);                 //Slat Effect
        float cl = (flap && !flap.ripped) ? airfoil.Cl(alpha, flapsInput) : airfoil.Cl(alpha);
        if (control && !control.ripped) cl += controlSqrt * airfoil.gradient * control.sinControlAngle;                                      //Surface control effect

        //Coefficient of Drag
        float cd = (flap && !flap.ripped) ? airfoil.Cd(alpha, flapsInput) : airfoil.Cd(alpha);
        float wingSpan = frame.aircraft ? frame.aircraft.wingSpan : 5f;
        if (wing) cd += cl * cl * totalArea * 2f / (wingSpan * wingSpan * Mathf.PI * wing.oswald);                                   //Induced Drag
        cd *= frame.data.groundEffect;

        Vector3 force = Aerodynamics.ComputeLift(velocity, frame.data.tas, rootTip, frame.data.airDensity, area, cl, frame.structureDamage);
        force += Aerodynamics.ComputeDrag(velocity, frame.data.tas, frame.data.airDensity, area, cd, frame.structureDamage);
        if (center.y > 0f) frame.rb.AddForceAtPosition(force, center);

        return alpha;
    }
}

public class SimplifiedAirfoil
{
    public static float ClSymmetric(float aoa)
    {
        if (Mathf.Abs(aoa) < 20f) return aoa / 20f;
        else return Mathf.Sign(aoa) * Mathf.InverseLerp(20f, 180f, Mathf.Abs(aoa));
    }
    public static float CdSymmetric(float aoa)
    {
        return 1.01f - Mathf.Cos(aoa * Mathf.PI / 90f);
    }
}

[CreateAssetMenu(fileName = "New Airfoil Preset", menuName = "Aircraft/Airfoil Preset")]
public class AirfoilPreset : ScriptableObject
{
    public AnimationCurve liftCurve = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve dragCurve = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve liftCurveFlaps = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve dragCurveFlaps = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public TextAsset performancePlot;

    public enum AirfoilType
    {
        Asymetric,
        Symetric
    }
    public AirfoilType type = AirfoilType.Asymetric;
    public float clZero = 0f;
    public float clMax = 1.2f;
    public float clMin = -1.2f;
    public float maxAngle = 18f;
    public float minAngle = -18f;
    public float cdMin = 0.01f;
    public float flapClZero = 0.5f;
    public float flapClMax = 3f;
    public float flapClMin = 0f;
    public float flapMaxAngle = 25f;
    public float flapMinAngle = -10f;
    public float flapCdMin = 0.1f;

    public TrailRenderer tipTrail;

    public float gradient { get { return (clMax - clZero) / (maxAngle*Mathf.Deg2Rad); } }

    public float Cl(float aoa) { return liftCurve.Evaluate(aoa); }
    public float Cl(float aoa, float flapFactor) { return Mathf.Lerp(liftCurve.Evaluate(aoa), liftCurveFlaps.Evaluate(aoa), flapFactor); }
    public float Cd(float aoa) { return dragCurve.Evaluate(aoa); }
    public float Cd(float aoa, float flapFactor) { return Mathf.Lerp(dragCurve.Evaluate(aoa), dragCurveFlaps.Evaluate(aoa), flapFactor); }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AirfoilPreset))]
public class AirfoilPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AirfoilPreset foil = (AirfoilPreset)target;
        //
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        foil.type = (AirfoilPreset.AirfoilType)EditorGUILayout.EnumPopup("Airfoil Type", foil.type);

        foil.liftCurve = EditorGUILayout.CurveField("Lift Curve", foil.liftCurve);
        foil.dragCurve = EditorGUILayout.CurveField("Drag Curve", foil.dragCurve);

        if (foil.type == AirfoilPreset.AirfoilType.Asymetric)
        {
            foil.clZero = EditorGUILayout.FloatField("Cl at AoA 0", foil.clZero);
            foil.clMax = EditorGUILayout.FloatField("Maximum Cl", foil.clMax);
            foil.maxAngle = EditorGUILayout.FloatField("At angle", foil.maxAngle);
            foil.clMin = EditorGUILayout.FloatField("Minimum Cl", foil.clMin);
            foil.minAngle = EditorGUILayout.FloatField("At angle", foil.minAngle);
            foil.cdMin = EditorGUILayout.FloatField("Cd Minimum", foil.cdMin);

            foil.liftCurveFlaps = EditorGUILayout.CurveField("Lift Curve Flaps", foil.liftCurveFlaps);
            foil.dragCurveFlaps = EditorGUILayout.CurveField("Drag Curve Flaps", foil.dragCurveFlaps);
            foil.flapClZero = EditorGUILayout.FloatField("Cl at AoA 0 , Flaps", foil.flapClZero);
            foil.flapClMax = EditorGUILayout.FloatField("Maximum Cl , Flaps", foil.flapClMax);
            foil.flapMaxAngle = EditorGUILayout.FloatField("At angle , Flaps", foil.flapMaxAngle);
            foil.flapClMin = EditorGUILayout.FloatField("Minimum Cl , Flaps", foil.flapClMin);
            foil.flapMinAngle = EditorGUILayout.FloatField("At angle , Flaps", foil.flapMinAngle);
            foil.flapCdMin = EditorGUILayout.FloatField("Cd Minimum , Flaps", foil.flapCdMin);
            foil.tipTrail = EditorGUILayout.ObjectField("Wingtip Trail", foil.tipTrail, typeof(TrailRenderer),false) as TrailRenderer;
        }
        else
        {
            foil.clZero = 0f;
            foil.clMax = EditorGUILayout.FloatField("Maximum/Minimum Cl", foil.clMax);
            foil.maxAngle = EditorGUILayout.FloatField("At angle", foil.maxAngle);
            foil.minAngle = -foil.maxAngle;
            foil.clMin = -foil.clMax;
            foil.cdMin = EditorGUILayout.FloatField("Cd Minimum", foil.cdMin);
        }

        

        if (GUILayout.Button("Apply Simplified Physics"))
        {
            foil.liftCurve = AnimationCurve.Linear(-180.0f, 0f, 180, 0f);
            foil.liftCurve.AddKey(Mathf.Lerp(foil.minAngle, -180f, 0.25f), foil.clMin / 2f);
            foil.liftCurve.AddKey(foil.minAngle, foil.clMin);
            foil.liftCurve.AddKey(0f, foil.clZero);
            foil.liftCurve.AddKey(foil.maxAngle, foil.clMax);
            foil.liftCurve.AddKey(Mathf.Lerp(foil.maxAngle, 180f, 0.25f), foil.clMax / 2f);
            foil.liftCurve.SmoothTangents(2, 0.9f);
            foil.liftCurve.SmoothTangents(4, -0.9f);

            foil.liftCurveFlaps = AnimationCurve.Linear(-180.0f, 0f, 180, 0f);
            foil.liftCurveFlaps.AddKey(Mathf.Lerp(foil.flapMinAngle, -180f, 0.25f), foil.flapClMin / 2f);
            foil.liftCurveFlaps.AddKey(foil.flapMinAngle, foil.flapClMin);
            foil.liftCurveFlaps.AddKey(0f, foil.flapClZero);
            foil.liftCurveFlaps.AddKey(foil.flapMaxAngle, foil.flapClMax);
            foil.liftCurveFlaps.AddKey(Mathf.Lerp(foil.flapMaxAngle, 180f, 0.25f), foil.flapClMax / 2f);
            foil.liftCurveFlaps.SmoothTangents(2, 0.9f);
            foil.liftCurveFlaps.SmoothTangents(4, -0.9f);

            foil.dragCurve = AnimationCurve.Linear(-180.0f, 0.02f, 180, 0.02f);
            foil.dragCurve.AddKey(-90f, 1f);
            foil.dragCurve.AddKey(90f, 1f);
            float zeroLiftPoint = foil.minAngle * foil.clZero / (foil.clZero - foil.clMin);
            foil.dragCurve.AddKey(zeroLiftPoint, foil.cdMin);
            foil.dragCurve.AddKey(foil.minAngle, foil.cdMin * (1f + 0.2f * (Mathf.Abs(zeroLiftPoint) - foil.minAngle)));
            foil.dragCurve.AddKey(foil.maxAngle, foil.cdMin * (1f + 0.2f * (Mathf.Abs(zeroLiftPoint) + foil.maxAngle)));
            //foil.dragCurve.SmoothTangents(2, -0.5f);
            foil.dragCurve.SmoothTangents(3, 0f);
            //foil.dragCurve.SmoothTangents(4, 0.5f);



            foil.dragCurveFlaps = AnimationCurve.Linear(-180.0f, 0.02f, 180, 0.02f);
            foil.dragCurveFlaps.AddKey(-90f, 1f);
            foil.dragCurveFlaps.AddKey(foil.flapMinAngle, foil.flapCdMin * 1.5f);
            foil.dragCurveFlaps.AddKey((foil.flapMaxAngle + foil.flapMinAngle) / 2f, foil.flapCdMin);
            foil.dragCurveFlaps.AddKey(foil.flapMaxAngle, foil.flapCdMin * 1.5f);
            foil.dragCurveFlaps.AddKey(90f, 1f);
            foil.dragCurveFlaps.SmoothTangents(2, -0.5f);
            foil.dragCurveFlaps.SmoothTangents(4, 0.5f);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(foil);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
