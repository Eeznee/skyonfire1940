using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class LiquidTank : Module
{
    public class LiquidCircuit
    {
        public LiquidCircuit(Module _module, LiquidTank _tank, float _escapeSpeed)
        {
            holesArea = 0f;
            module = _module;
            mainTank = _tank;
            escapeSpeed = _escapeSpeed;
            leak = Instantiate(mainTank.leakEffect, module.transform).GetComponent<ParticleSystem>();
        }
        public Module module;
        public LiquidTank mainTank;

        public float holesArea;
        public float escapeSpeed;
        protected ParticleSystem leak;

        public void Damage(float caliber)
        {
            holesArea += Mathv.SmoothStart(caliber / 2000f, 2) * Mathf.PI;
        }
        public void Leaking()
        {
            float fill = mainTank.currentAmount / mainTank.capacity;
            float leakRate = holesArea * escapeSpeed * 1000f;
            mainTank.Consume(leakRate * Time.deltaTime);
            if (mainTank.burning || fill <= 0f || holesArea <= 0f)
            {
                if (leak.isPlaying) leak.Stop();
            }
            else if (!leak.isPlaying)
            {
                leak.Play();
            }
        }
    }

    //Tank
    public float capacity;
    public float currentAmount;
    public LiquidCircuit circuit;

    //Leak
    public float escapeSpeed = 2f;
    public GameObject leakEffect;

    private float massLost;
    const float massLostThreshold = 0.4f;

    public float fill { get { return currentAmount / capacity; } }
    public override float Mass() { return currentAmount;  }
    public override float EmptyMass() { return 0f; }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            massLost = 0f;
            currentAmount = capacity;
            maxHp = material.hpPerSq * Mathf.Pow(capacity, 2f / 3f);
            circuit = new LiquidCircuit(this, this, escapeSpeed);
            emptyMass = 0f;
        }
    }
    private void Update()
    {
        if (Integrity != 1f)
            circuit.Leaking();
    }
    public void Consume(float amount)
    {
        if (!aircraft || currentAmount <= 0f) return;
        currentAmount -= amount;
        if (currentAmount < 0f)
        {
            currentAmount = 0f;
            amount += currentAmount;
        }
        massLost += amount;
        if (massLost > massLostThreshold)
        {
            data.ShiftMass(-massLost);
            massLost = 0f;
        }
    }
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        base.KineticDamage(damage, caliber, fireCoeff);
        circuit.Damage(caliber);
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(LiquidTank))]
public class LiquidTankEditor : Editor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //
        LiquidTank tank = (LiquidTank)target;
        //
        serializedObject.Update();

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Tank Configuration", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.HelpBox("Hp scales with capacity x hp per sq", MessageType.Info);
        tank.currentAmount = tank.capacity = EditorGUILayout.FloatField("Capacity kg", tank.capacity);
        EditorGUILayout.LabelField("Capacity in Gallons : ", (tank.capacity / 4.55f).ToString("0"));
        tank.escapeSpeed = EditorGUILayout.FloatField("Leak Speed m/s", tank.escapeSpeed);
        tank.data = EditorGUILayout.ObjectField("Data", tank.data, typeof(ObjectData),true) as ObjectData;
        EditorGUILayout.LabelField("30 cal empty time : ", (tank.capacity / (Mathf.Pow(7.62f / 2000f, 2) * tank.escapeSpeed * 1000f * Mathf.PI)).ToString("0") + " sec");

        tank.material = EditorGUILayout.ObjectField("Material", tank.material, typeof(ModuleMaterial), false) as ModuleMaterial;
        tank.leakEffect = EditorGUILayout.ObjectField("Leak Effect", tank.leakEffect, typeof(GameObject), false) as GameObject;


        if (GUI.changed)
        {
            EditorUtility.SetDirty(tank);
            EditorSceneManager.MarkSceneDirty(tank.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
