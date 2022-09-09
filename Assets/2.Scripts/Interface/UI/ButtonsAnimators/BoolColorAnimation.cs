using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoolColorAnimation : MonoBehaviour {

	public Image m_Image;

	public Color trueColor;
	public Color falseColor;


	public void SwitchColor(bool State){

		if (State == true) {

			m_Image.color = trueColor;

		} else {

			m_Image.color = falseColor;
		}

	}


}
