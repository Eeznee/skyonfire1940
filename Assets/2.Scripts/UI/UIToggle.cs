using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour
{
	public GameObject first;
	public GameObject second;

	bool state = true;

    private void Start()
    {
		state = true;
		first.SetActive(true);
		second.SetActive(false);
    }

    public void Toggle(){

		Toggle(!state);
	}
	public void Toggle(bool newState)
    {
		if (newState == state) return;
		state = newState;
		first.SetActive(state);
		second.SetActive(!state);
	}
}
