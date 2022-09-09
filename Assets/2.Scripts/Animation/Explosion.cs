using UnityEngine;
using System.Collections;

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
	public static void ExplosionDamage(Vector3 pos, float tntMass, float totalMass)
    {
		foreach (SofObject obj in GameManager.sofObjects.ToArray())
		{
			obj.Explosion(pos, tntMass, totalMass);
		}
	}
}