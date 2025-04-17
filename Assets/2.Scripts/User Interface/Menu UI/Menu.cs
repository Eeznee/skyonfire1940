using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
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

        foreach(PPSlider pps in GetComponentsInChildren<PPSlider>())
        {
            pps.Start();
        }
        foreach (PPToggle ppt in GetComponentsInChildren<PPToggle>())
        {
            ppt.Start();
        }
        foreach (AudioSlider audioSlider in GetComponentsInChildren<AudioSlider>())
        {
            audioSlider.Start();
        }
    }
}
