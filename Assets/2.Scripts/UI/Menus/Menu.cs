using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Menu : MonoBehaviour
{
	public GameObject mainMenu;
	public ConfirmOverlay confirmOverlay;
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
        Action onConfirm = Application.Quit;
		confirmOverlay.SetupAndOpen(mainMenu, onConfirm, null, "Exiting Game", "Are you sure you want to exit the game ?", "Yes, Exit", "Cancel");
	}
}
