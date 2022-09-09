using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//

public class Weather : MonoBehaviour
{
    public GameManager gm;
    public Light sun;
    public float startHour;

    public float multiplier = 1f;
    public float currentTime = 0;
    public int currentHour;
    public int currentMinute;
    public int currentSecond;

    public float sunAngle;
    public float sunClimateAngle = 107;
    public float sunMaximumIntensity = 1.3f;

    public float localTemperature;
    public bool winter = false;

    private float counter = 0f;
    const float updateDelta = 10f;
    const float fullDaySeconds = 86400;
    //
    void Awake()
    {
        winter = PlayerPrefs.GetInt("Winter", 0) == 1;
        currentTime = PlayerPrefs.GetFloat("Hour", 10f) * 3600;
        gm = GameManager.gm;
        UpdateWeather();
    }
    //
    void Update()
    {

        counter += Time.deltaTime;
        if (counter > updateDelta)
        {
            UpdateWeather();
            counter = 0f;
        }
        currentTime = (currentTime + Time.deltaTime * multiplier) % fullDaySeconds;
        float t = currentTime;
        currentSecond = (int)t % 60;
        t = (t - currentSecond) / 60;
        currentMinute = (int)t % 60;
        currentHour = (int)(t - currentMinute) / 60;
    }
    //
    void UpdateWeather()
    {
        float t = currentTime / fullDaySeconds;
        /*
		sun.transform.eulerAngles = new Vector3(t * 360 - 90, sunClimateAngle, 180);
		sunAngle = sun.transform.localRotation.eulerAngles.x;

		float intensityMultiplier = 1;
		if (t <= 0.23f || t >= 0.75f) {
			intensityMultiplier = 0;
		} else if (t <= 0.25f) {
			intensityMultiplier = Mathf.Clamp01 ((t - 0.23f) * (1 / 0.02f));
		}
		else if (t >= 0.73f) {
			intensityMultiplier = Mathf.Clamp01 (1 - ((t - 0.73f) * (1 / 0.02f)));
		}

		sun.intensity = sunMaximumIntensity * intensityMultiplier;
		*/
        localTemperature = gm.mapData.temperature.Evaluate(24f * t);
        localTemperature -= winter ? gm.mapData.winterOffset : 0f;
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(Weather))]
public class WeatherEditor : Editor
{
    Color backgroundColor;
    //
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //
        Weather weather = (Weather)target;
        //
        serializedObject.Update();

        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Time Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        weather.startHour = EditorGUILayout.FloatField("Start Hour", weather.startHour);
        weather.multiplier = EditorGUILayout.FloatField("Time multiplier", weather.multiplier);

        GUILayout.Space(20f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Solar Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        weather.sun = EditorGUILayout.ObjectField("Sun", weather.sun, typeof(Light), true) as Light;
        GUILayout.Space(8f);
        weather.sunMaximumIntensity = EditorGUILayout.FloatField("Maximum Intensity", weather.sunMaximumIntensity);
        //
        GUILayout.Space(20f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Weather Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        weather.winter = EditorGUILayout.Toggle("Winter", weather.winter);
        GUILayout.Space(6f);    //
                                //
        if (GUI.changed)
        {
            EditorUtility.SetDirty(weather);
            EditorSceneManager.MarkSceneDirty(weather.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif