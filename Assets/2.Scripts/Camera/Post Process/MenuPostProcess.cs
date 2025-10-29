using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class MenuPostProcess : MonoBehaviour
{
    public VolumeProfile profile;

    public GameObject mainMenuGameObject;
    public float defaultFocusDistance = 2.5f;
    public float blurredFocusDistance = 0.1f;

    private DepthOfField depthOfField;
    private float blurValue;

    private void Start()
    {
        profile.TryGet(out depthOfField);
        blurValue = defaultFocusDistance;
        depthOfField.focusDistance.value = blurValue;
    }

    private void Update()
    {
        float targetBlurValue = mainMenuGameObject.activeSelf ? defaultFocusDistance : blurredFocusDistance;

        if (blurValue == targetBlurValue) return;

        blurValue = Mathf.MoveTowards(blurValue, targetBlurValue, Time.unscaledDeltaTime * 5f);
        depthOfField.focusDistance.value = blurValue;
    }
}
