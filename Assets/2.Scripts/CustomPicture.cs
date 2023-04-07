using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class CustomPicture : MonoBehaviour
{
    Renderer rend;
    private void Start()
    {
        rend = GetComponent<Renderer>();
        string textureName = PlayerPrefs.GetString("custom_pictureLastSelected", "Disabled");
        if (textureName == "Disabled")
        {
            Destroy(gameObject);
            return;
        }
        Texture texture = TextureTool.Load("custom_picture",textureName);
        rend.sharedMaterial.mainTexture = texture;
        transform.localScale = new Vector3(1f, texture.texelSize.x/texture.texelSize.y, 1f);
    }
}
