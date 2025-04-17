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


    const int fragmentsAmountRef = 30;
    const float fragmentsMassRef = 0.115f;
    const float totalAreaRef = 4500f;
    const float fragmentsVelocity = 1000f;
    const float fragmentsRangeRef = 30f;

    private int AmountFragments(float fragMass)
    {
        return Mathf.RoundToInt(fragmentsAmountRef * Mathf.Pow(fragMass / fragmentsMassRef, 0.225f));
    }
    private float FragmentDiameter(int fragments, float fragMass)
    {
        float totalArea = totalAreaRef * fragMass / fragmentsMassRef;
        return Mathf.Sqrt(totalArea / fragments / Mathf.PI);
    }
    const float maxAbsoluteRange = 1000f;
    private float FragmentRange(float individualMass)
    {
        float individualMassRef =  fragmentsMassRef / fragmentsAmountRef;
        float range = Mathf.Pow(individualMass / individualMassRef, 5f / 8f) * fragmentsRangeRef;
        return Mathf.Min(range,maxAbsoluteRange);
    }
    public void Detonate(Vector3 pos, float totalMass, Transform tr)
    {
        ExplosionFX instance = Object.Instantiate(fx, pos, Quaternion.identity,tr);
        instance.Explode(mass * explosive.tntMultiplier);

        bool water = pos.y < 2f;
        BlastDamage(pos, water);
        if (water) return;
        ProjectFragments(pos,totalMass);
    }
    private void BlastDamage(Vector3 pos, bool water)
    {
        float tntEquivalent = mass * explosive.tntMultiplier;
        if (water) tntEquivalent *= 0.25f;
        foreach (SofObject obj in GameManager.sofObjects.ToArray())
            if (obj) obj.Explosion(pos, tntEquivalent);
    }
    private void ProjectFragments(Vector3 pos, float totalMass)
    {
        float fragmentsMass = totalMass - mass;
        int fragments = AmountFragments(fragmentsMass);
        float individualMass = fragmentsMass / fragments;
        float diameter = FragmentDiameter(fragments, fragmentsMass);
        float range = FragmentRange(individualMass);
        float penetration = Ballistics.ApproximatePenetration(individualMass, fragmentsVelocity, diameter);
        int bubbleLayerMask = LayerMask.GetMask("Bubble");

        Ballistics.ProjectileChart chart = new Ballistics.ProjectileChart(individualMass, penetration, fragmentsVelocity, diameter, 0f);
        for (int i = 0; i < fragments; i++)
        {
            Vector3 vel = Random.onUnitSphere * fragmentsVelocity;
            RaycastHit hit;
            if (Physics.Raycast(pos, vel, out hit, range, bubbleLayerMask))
            {
                ObjectBubble bubble = hit.collider.GetComponent<ObjectBubble>();
                if (!bubble) continue;
                bubble.EnableColliders(false);
                Ballistics.HitResult result = Ballistics.RaycastDamage(hit.point, vel, bubble.bubble.radius * 2f, chart);

                if(result.summary != Ballistics.HitSummary.NoHit)
                    StaticReferences.Instance.fragmentsHits.AircraftHit(false, result.firstHit);
            }
        }
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ExplosiveFiller))]
    public class ExplosiveFillerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

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

            EditorGUI.LabelField(labelRect, new GUIContent("Mass kg"));
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("mass"),GUIContent.none);
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("explosive"), GUIContent.none);
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("fx"), GUIContent.none);

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