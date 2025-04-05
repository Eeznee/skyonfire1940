using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Propeller))]
public class PropellerVisual : MonoBehaviour
{
    public MeshRenderer brokenProp;
    public MeshRenderer blurredProp;


    private Propeller propeller;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock blurredBlock;

    private void Start()
    {
        propeller = GetComponent<Propeller>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (!blurredProp || !brokenProp) return;
        blurredBlock = new MaterialPropertyBlock();
        blurredProp.gameObject.layer = 1;
        meshRenderer.enabled = blurredProp.enabled = true;
        brokenProp.enabled = false;
        transform.Rotate(Vector3.forward * Random.value * 360f);

        propeller.OnRip += OnPropellerRip;
    }

    const float INVERT90 = 1f / 90F;
    const float meshDiseppearAtRadPerSec = 40f;
    public void Update()
    {
        if (!meshRenderer || !blurredProp || !propeller.aircraft) return;

        Vector3 cameraDir = transform.position - SofCamera.tr.position;
        float angle = 1f - Mathf.Abs(Vector3.Angle(cameraDir, transform.root.forward) * INVERT90 - 1f);

        blurredProp.GetPropertyBlock(blurredBlock);
        blurredBlock.SetFloat("_Rpm", propeller.RadPerSec * 30f * Mathv.invPI);
        blurredBlock.SetFloat("_CameraAngle", Mathv.SmoothStart(angle, 5));
        blurredProp.SetPropertyBlock(blurredBlock);

        blurredProp.enabled = true;
        float timeScale = Time.timeScale == 0f ? 1f : Time.timeScale;
        meshRenderer.enabled = propeller.RadPerSec * timeScale < meshDiseppearAtRadPerSec && !propeller.ripped;

        transform.Rotate(-Vector3.forward * Time.deltaTime * propeller.RadPerSec * 57.3f);
    }

    private void OnPropellerRip(SofModule module)
    {
        meshRenderer.enabled = blurredProp.enabled = false;
        brokenProp.enabled = true;
    }
}
