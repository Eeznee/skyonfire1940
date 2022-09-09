using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    public static class JInternalMaterials
    {
        private static Material copyTextureMaterial;
        public static Material CopyTextureMaterial
        {
            get
            {
                if (copyTextureMaterial == null)
                {
                    copyTextureMaterial = new Material(JJupiterSettings.Instance.InternalShaders.CopyTextureShader);
                }
                return copyTextureMaterial;
            }
        }

        private static Material solidColorMaterial;
        public static Material SolidColorMaterial
        {
            get
            {
                if (solidColorMaterial == null)
                {
                    solidColorMaterial = new Material(JJupiterSettings.Instance.InternalShaders.SolidColorShader);
                }
                return solidColorMaterial;
            }
        }

        private static Material unlitTextureMaterial;
        public static Material UnlitTextureMaterial
        {
            get
            {
                if (unlitTextureMaterial == null)
                {
                    unlitTextureMaterial = new Material(Shader.Find("Unlit/Texture"));
                }
                return unlitTextureMaterial;
            }
        }
    }
}
