using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ViewModeButton : MonoBehaviour
{
    public Image image;
    public Sprite toExternal;
    public Sprite toCockpit;

    int viewMode = -1;

    void Update()
    {
        if (viewMode != SofCamera.viewMode)
        {
            viewMode = SofCamera.viewMode;
            image.sprite = viewMode == 0 ? toCockpit : toExternal;
        }
    }
}
