using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Propeller))]
public class PropellerVisual : SofComponent
{
    public MeshRenderer brokenProp;
    public MeshRenderer blurredProp;

    public override int DefaultLayer()
    {
        return 2;
    }
    private Propeller propeller;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock blurredBlock;

    public override void SetReferences(SofModular _modular)
    {
        if (aircraft) aircraft.OnUpdateLOD1 -= UpdatePropellerVisual;
        base.SetReferences(_modular);

        propeller = GetComponent<Propeller>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        if (!blurredProp || !brokenProp) return;
        blurredBlock = new MaterialPropertyBlock();
        blurredProp.gameObject.layer = 1;
        meshRenderer.enabled = blurredProp.enabled = true;
        brokenProp.enabled = false;
        transform.Rotate(Vector3.forward * Random.value * 360f);

        propeller.OnRip += OnPropellerRip;
        aircraft.OnUpdateLOD1 += UpdatePropellerVisual;
    }

    const float INVERT90 = 1f / 90F;
    const float meshDiseppearAtRadPerSec = 40f;
    public void UpdatePropellerVisual()
    {
        if (!meshRenderer || !blurredProp) return;

        Vector3 cameraDir = transform.position - SofCamera.tr.position;
        float angle = 1f - Mathf.Abs(Vector3.Angle(cameraDir, transform.root.forward) * INVERT90 - 1f);

        blurredProp.GetPropertyBlock(blurredBlock);
        blurredBlock.SetFloat("_Rpm", propeller.RadPerSec * 30f * Mathv.invPI);
        blurredBlock.SetFloat("_CameraAngle", Mathv.SmoothStart(angle, 5));
        blurredProp.SetPropertyBlock(blurredBlock);

        blurredProp.enabled = true;
        float timeScale = Time.timeScale == 0f ? 1f : Time.timeScale;
        meshRenderer.enabled = propeller.RadPerSec * timeScale < meshDiseppearAtRadPerSec && !propeller.ripped;
    }

    private void OnPropellerRip(SofModule module)
    {
        meshRenderer.enabled = blurredProp.enabled = false;
        brokenProp.enabled = true;
    }
}
