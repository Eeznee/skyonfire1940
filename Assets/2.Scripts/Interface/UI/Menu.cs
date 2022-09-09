using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public PPSlider[] ppSliders;
    public PPToggle[] ppToggles;
    public AudioSlider[] audioSliders;
	//load a scene
	public void LoadScene (string scene)
	{
		SceneManager.LoadScene (scene);
    }

	public void OpenUrl (string Url)
	{
		Application.OpenURL (Url);
	}

	//leave the game
	public void Exit ()
	{
		Application.Quit ();
	}

	void Start ()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Load every player prefs values
        foreach(PPSlider pps in ppSliders)
        {
            pps.Start();
        }
        //Load every player prefs values
        foreach (AudioSlider audioSlider in audioSliders)
        {
            audioSlider.Start();
        }
    }
}
