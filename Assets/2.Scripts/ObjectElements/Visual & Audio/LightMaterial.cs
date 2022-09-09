using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMaterial : ObjectElement
{
    [System.Serializable]
    public class MaterialSwitcher
    {
        public Material lightMat;
        public Material defaultMat;
    }

    public MaterialSwitcher[] materialSwitchers;
    public MaterialSwitcher[] overriders;
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        bool light = QualitySettings.GetQualityLevel() == 0;
        if (firstTime)
        {
            MeshRenderer[] renderers = sofObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer r in renderers)
            {
                foreach(MaterialSwitcher ov in overriders)
                {
                    if (r.sharedMaterial == ov.lightMat) r.sharedMaterial = ov.defaultMat;
                }
                foreach (MaterialSwitcher ms in materialSwitchers)
                {
                        if (ms.lightMat == r.sharedMaterial && !light) r.sharedMaterial = ms.defaultMat;
                    if (ms.defaultMat == r.sharedMaterial && light) r.sharedMaterial = ms.lightMat;
                }
            }
        }
    }
}
