using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
abstract public class Turret : ObjectElement
{
    //General Parameters ---------------------------------------------------------------------------------------------------------------
    public bool preventSelfDamage;
    public bool animatedConstrains = false;

    //Input ------------------------------
    public int controlMode = 0; //0 = full automatic, 1 = special axis manual, 2 = all manual
    protected Vector3 targetDirection;
    protected Vector2 basicAxis;
    protected float specialAxis;
    public bool selfDamaging = false;

    private Vector3 localTarget = Vector3.zero;

    public float animationFactor = 0f;

    protected Gun[] guns;

    public Vector3 FiringDirection() { return guns[0].transform.forward; }
    public bool IsFiring()
    {
        bool firing = false;
        foreach (Gun gun in guns) firing |= gun.Firing();
        return firing;
    }

    public virtual float TargetAvailability(Vector3 pos)
    {
        return 1f;
    }
    public void SetFuze(float dis)
    {
        foreach (Gun g in guns) g.fuzeDistance = dis;
    }
    public float MuzzleVelocity() { return guns[0].gunPreset.ammunition.defaultMuzzleVel; }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        guns = GetComponentsInChildren<Gun>();
    }
    //Turret possible behaviours
    protected virtual void BasicAxisTarget()
    {

    }
    protected virtual void SpecialAxisTarget()
    {

    }
    protected virtual void BasicAxisInput()
    {

    }
    protected virtual void SpecialAxisInput()
    {

    }

    // Turret inputs
    public void SetDirectionAuto(Vector3 worldDir)
    {
        controlMode = 0;
        localTarget = transform.parent.InverseTransformDirection(worldDir);
    }
    public void SetDirectionSemi(Vector3 worldDir, float special)
    {
        controlMode = 1;
        localTarget = transform.parent.InverseTransformDirection(worldDir);
        specialAxis = special;
    }
    public void SetDirectionSemi(Vector3 worldDir)
    {
        SetDirectionSemi(worldDir, specialAxis);
    }
    public void SetManual(Vector2 basic, float special)
    {
        controlMode = 2;
        basicAxis = basic;
        specialAxis = special;
    }
    public void SetManual(Vector2 basic)
    {
        SetManual(basic, specialAxis);
    }
    public void SetSpecial(float special)
    {
        specialAxis = special;
    }

    public void Operate(bool fire,bool automaticPrevention)
    {
        targetDirection = transform.parent.TransformDirection(localTarget).normalized;

        if (controlMode <= 1) BasicAxisTarget();
        else BasicAxisInput();
        if (controlMode == 0) SpecialAxisTarget();
        else SpecialAxisInput();

        if (!fire) return;

        //Self Damaging
        if (preventSelfDamage || automaticPrevention)
        {
            for (int j = 0; j < guns.Length; j++)
                if (Physics.Raycast(guns[j].transform.position, guns[j].transform.forward, 10f, LayerMask.GetMask("SofObject"))) return;
        }


        //Fire the guns
        foreach (Gun g in guns) g.Trigger();
    }
    public bool Firing() { return guns[0].Firing(); }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Turret))]
public class TurretEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Turret turret = (Turret)target;
        turret.preventSelfDamage = EditorGUILayout.Toggle("Prevent self damage", turret.preventSelfDamage);
        turret.animatedConstrains = EditorGUILayout.Toggle("Animated Constrains", turret.animatedConstrains);
        GUI.color = GUI.backgroundColor;
        if (GUI.changed)
        {
            EditorUtility.SetDirty(turret);
            EditorSceneManager.MarkSceneDirty(turret.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif


