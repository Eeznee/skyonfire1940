using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class CustomPicture : MonoBehaviour
{
    Renderer rend;

    private void OnEnable()
    {
        rend = GetComponent<Renderer>();

        UpdatePicture();
        SofSettingsSO.OnUpdateSettings += UpdatePicture;
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= UpdatePicture;
    }

    public void UpdatePicture()
    {
        string textureName = PlayerPrefs.GetString("custom_pictureLastSelected", "Default");
        rend.enabled = textureName != "Default";

        if (rend.enabled)
        {
            Texture texture = TextureTool.Load("custom_picture", textureName);
            rend.sharedMaterial.mainTexture = texture;
            transform.localScale = new Vector3(1f, texture.texelSize.x / texture.texelSize.y, 1f);
        }
    }
}
