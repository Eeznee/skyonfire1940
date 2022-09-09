using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    public static class JCubemapRenderer
    {
        public static bool Render(JCubemapRendererArgs args)
        {
            GameObject go = new GameObject("~CubemapRendererCamera");
            go.transform.position = args.CameraPosition;

            Camera cam = go.AddComponent<Camera>();
            cam.clearFlags = args.CameraClearFlag;
            cam.nearClipPlane = args.CameraNearPlane;
            cam.farClipPlane = args.CameraFarPlane;
            cam.backgroundColor = args.CameraBackgroundColor;

            bool result = cam.RenderToCubemap(args.Cubemap, (int)args.Face);
            JUtilities.DestroyGameobject(go);

            return result;
        }
    }
}
