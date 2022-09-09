using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    public struct JCubemapRendererArgs
    {
        public Cubemap Cubemap { get; set; }

        public Vector3 CameraPosition { get; set; }
        public float CameraNearPlane { get; set; }
        public float CameraFarPlane { get; set; }
        public CameraClearFlags CameraClearFlag { get; set; }
        public Color CameraBackgroundColor { get; set; }
        public int Resolution { get; set; }
        public CubemapFace Face { get; set; }
    }
}
