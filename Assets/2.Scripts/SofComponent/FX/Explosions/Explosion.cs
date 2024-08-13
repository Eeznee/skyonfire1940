using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public struct ExplosiveFiller
{
	public float mass;
	public Explosive explosive;
	public ExplosionFX fx;


    const int fragmentsAmountRef = 20;
    const float fragmentsMassRef = 0.115f;
    const float totalAreaRef = 4500f;
    const float fragmentsVelocity = 1000f;
    const float fragmentsRangeRef = 30f;

    private int AmountFragments(float fragMass)
    {
        return Mathf.RoundToInt(fragmentsAmountRef * Mathf.Pow(fragMass / fragmentsMassRef, 0.225f));
    }
    private float Diameter(int fragments, float fragMass)
    {
        float totalArea = totalAreaRef * fragMass / fragmentsMassRef;
        return Mathf.Sqrt(totalArea / fragments / Mathf.PI);
    }
    private float Range(float individualMass)
    {
        float individualMassRef =  fragmentsMassRef / fragmentsAmountRef;
        return Mathf.Pow(individualMass / individualMassRef , 5f / 8f) * fragmentsRangeRef;
    }
    public void Detonate(Vector3 pos, float totalMass, Transform tr)
    {
        //FX
        ExplosionFX instance = Object.Instantiate(fx, pos, Quaternion.identity,tr);
        instance.Explode(mass * explosive.tntMultiplier);

        //Blast Damage
        foreach (SofObject obj in GameManager.sofObjects.ToArray())
            if (obj) obj.Explosion(pos, mass * explosive.tntMultiplier);

        //Fragmentation Damage
        float fragmentsMass = totalMass - mass;
        int fragments = AmountFragments(fragmentsMass);
        float individualMass = fragmentsMass / fragments;
        float diameter = Diameter(fragments,fragmentsMass);
        float range = Range(individualMass);
        float penetration = Ballistics.ApproximatePenetration(individualMass, fragmentsVelocity, diameter);
        for (int i = 0; i < fragments; i++) RaycastFragment(pos, individualMass, diameter, range,penetration);
    }
    private void RaycastFragment(Vector3 pos,float mass, float diam, float range, float pen) 
    {
        Vector3 vel = Random.onUnitSphere * fragmentsVelocity;

        RaycastHit[] hits = Ballistics.RaycastAndSort(pos, vel, range, LayerMask.GetMask("SofComplex"));
        if (hits.Length == 0) return;

        float sqrVelocity = fragmentsVelocity * fragmentsVelocity;
        foreach (RaycastHit h in hits)
        {
            SofModule module = h.collider.GetComponent<SofModule>();
            if (module == null) continue;
            float penetrationPower = pen * sqrVelocity / (fragmentsVelocity * fragmentsVelocity);
            float alpha = Vector3.Angle(-h.normal, vel);
            float armor = Random.Range(0.8f, 1.2f) * module.Armor.surfaceArmor / Mathf.Cos(alpha * Mathf.Deg2Rad);
            if (penetrationPower > armor)//If penetration occurs
            {
                //part.Damage(mass * sqrVelocity / 2000f, diam, 0f);
                module.ProjectileDamage(diam * diam / 5f, diam, 0f);
                armor += Random.Range(0.8f, 1.2f) * module.Armor.fullPenArmor;
                sqrVelocity *= 1f - armor / penetrationPower;
                if (sqrVelocity <= 0f) return;
            }
            else return;
        }
    }

    /* Old fragmentation
    float realDis = Mathf.Max(1f, Mathf.Sqrt(sqrDis));
    bool shrapnelhit = Random.value * 10f < shrapnel / realDis;
    if (shrapnelhit)
    {
        float shrapnelDamage = Mathf.Lerp(Mathf.Sqrt(shrapnel) / 5f, 0f, sqrDis / tnt) * Random.Range(0.5f, 2f);
        Damage(shrapnelDamage, 10f, 0f);
    }
    */
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ExplosiveFiller))]
    public class ExplosiveFillerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            float width = position.width / 6f;
            float height = position.height;
            var labelRect = new Rect(position.x, position.y, width - 3, height);
            var amountRect = new Rect(position.x + position.width * 1f / 6f, position.y, width - 3, height);
            var unitRect = new Rect(position.x + position.width*2f/6f, position.y, width * 2f - 3, height);
            var nameRect = new Rect(position.x + position.width*4f/6f, position.y, width * 2f - 3, height);
            //EditorGUILayout.PropertyField(property.FindPropertyRelative("baseVelocity"));

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(labelRect, new GUIContent("Mass kg"));
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("mass"),GUIContent.none);
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("explosive"), GUIContent.none);
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("fx"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
#endif
}

public class Explosion : MonoBehaviour {

	[Header("Customizable Options")]
	public float despawnTime = 10.0f;

	[Header("Audio")]
	public AudioClip[] explosionSounds;
	public AudioSource audioSource;


	static float lastExplosion;

	private void Start () {
		Destroy(gameObject, despawnTime);

		if (Time.time - lastExplosion < 0.02f) return;
		audioSource.clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
		audioSource.PlayDelayed((transform.position - Camera.main.transform.position).magnitude / (343f * Time.timeScale));
		audioSource.outputAudioMixerGroup = GameManager.gm.listener.persistent;
		lastExplosion = Time.time;
	}
}