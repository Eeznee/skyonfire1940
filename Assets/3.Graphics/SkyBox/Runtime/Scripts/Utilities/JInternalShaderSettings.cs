using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    [System.Serializable]
    public struct JInternalShaderSettings
    {
        [SerializeField]
        private Shader skyShader;
        public Shader SkyShader
        {
            get
            {
                return skyShader;
            }
            set
            {
                skyShader = value;
            }
        }

        [SerializeField]
        private Shader copyTextureShader;
        public Shader CopyTextureShader
        {
            get
            {
                return copyTextureShader;
            }
            set
            {
                copyTextureShader = value;
            }
        }

        [SerializeField]
        private Shader solidColorShader;
        public Shader SolidColorShader
        {
            get
            {
                return solidColorShader;
            }
            set
            {
                solidColorShader = value;
            }
        }
    }
}
