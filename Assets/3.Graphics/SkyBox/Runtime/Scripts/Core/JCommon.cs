using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;
using DateTime = System.DateTime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Jupiter
{
    public static class JCommon
    {
        public static string SUPPORT_EMAIL = "support@pinwheel.studio";
        public static string BUSINESS_EMAIL = "hello@pinwheel.studio";
        public static string YOUTUBE_CHANNEL = "https://www.youtube.com/channel/UCebwuk5CfIe5kolBI9nuBTg";
        public static string ONLINE_MANUAL = "https://docs.google.com/document/d/1Wf4CDlD96c6tna1ee0fpquWkdSrGDIO-eKuQDxjJedY/edit?usp=sharing";
        public static string FORUM = "https://forum.unity.com/threads/pre-released-jupiter-procedural-sky-builtin-lwrp-urp.799635/";
        public static string DISCORD = "https://discord.gg/HXNnFpS";


        public const int PREVIEW_TEXTURE_SIZE = 512;
        public const int TEXTURE_SIZE_MIN = 1;
        public const int TEXTURE_SIZE_MAX = 8192;

        public static JRenderPipelineType CurrentRenderPipeline
        {
            get
            {
                string pipelineName = Shader.globalRenderPipeline;
                if (pipelineName.Equals("UniversalPipeline,LightweightPipeline"))
                {
                    return JRenderPipelineType.Universal;
                }
                else if (pipelineName.Equals("LightweightPipeline"))
                {
                    return JRenderPipelineType.Lightweight;
                }
                else
                {
                    return JRenderPipelineType.Builtin;
                }
            }
        }

        private static Vector2[] fullRectUvPoints;
        public static Vector2[] FullRectUvPoints
        {
            get
            {
                if (fullRectUvPoints == null)
                {
                    fullRectUvPoints = new Vector2[]
                    {
                        Vector2.zero,
                        Vector2.up,
                        Vector2.one,
                        Vector2.right
                    };
                }
                return fullRectUvPoints;
            }
        }

        private static Mesh emptyMesh;
        public static Mesh EmptyMesh
        {
            get
            {
                if (emptyMesh == null)
                {
                    emptyMesh = new Mesh();
                }
                return emptyMesh;
            }
        }

        private static Material[] emptyMaterials;
        public static Material[] EmptyMaterials
        {
            get
            {
                if (emptyMaterials==null)
                {
                    emptyMaterials = new Material[0];
                }
                return emptyMaterials;
            }
        }

        public static Rect UnitRect
        {
            get
            {
                return new Rect(0, 0, 1, 1);
            }
        }

        public static string GetUniqueID()
        {
            string s = GetTimeTick().ToString();
            return Reverse(s);
        }

        public static long GetTimeTick()
        {
            DateTime time = DateTime.Now;
            return time.Ticks;
        }

        public static string Reverse(string s)
        {
            char[] chars = s.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }

        public static void SetDirty(Object o)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(o);
#endif
        }

        public static void AddObjectToAsset(Object objectToAdd, Object asset)
        {
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(objectToAdd, asset);
#endif
        }

        public static Texture2D CreateTexture(int resolution, Color fill, TextureFormat format = TextureFormat.ARGB32)
        {
            Texture2D t = new Texture2D(resolution, resolution, format, false);
            Color[] colors = new Color[resolution * resolution];
            JUtilities.Fill(colors, fill);
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static void CopyToRT(Texture t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            Graphics.Blit(t, rt);
            RenderTexture.active = null;
        }

        public static void CopyFromRT(Texture2D t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            t.Apply();
            RenderTexture.active = null;
        }

        public static void CopyTexture(Texture2D src, Texture2D des)
        {
            RenderTexture rt = new RenderTexture(des.width, des.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            CopyToRT(src, rt);
            CopyFromRT(des, rt);
            rt.Release();
            JUtilities.DestroyObject(rt);
        }

        public static Texture2D CloneTexture(Texture2D t)
        {
            RenderTexture rt = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32);
            CopyToRT(t, rt);
            Texture2D result = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
            result.filterMode = t.filterMode;
            result.wrapMode = t.wrapMode;
            CopyFromRT(result, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
            return result;
        }

        public static void FillTexture(Texture2D t, Color c)
        {
            Color[] colors = new Color[t.width * t.height];
            JUtilities.Fill(colors, c);
            t.SetPixels(colors);
            t.Apply();
        }

        public static void FillTexture(RenderTexture rt, Color c)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            CopyToRT(tex, rt);
            JUtilities.DestroyObject(tex);
        }

        public static Texture2D CloneAndResizeTexture(Texture2D t, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            CopyToRT(t, rt);
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.filterMode = t.filterMode;
            result.wrapMode = t.wrapMode;
            CopyFromRT(result, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
            return result;
        }

        public static RenderTexture CopyToRT(Texture src, int startX, int startY, int width, int height, Color defaultColor)
        {
            int endX = startX + width - 1;
            int endY = startY + height - 1;
            Vector2 startUV = new Vector2(
                JUtilities.InverseLerpUnclamped(0, src.width - 1, startX),
                JUtilities.InverseLerpUnclamped(0, src.height - 1, startY));
            Vector2 endUV = new Vector2(
                JUtilities.InverseLerpUnclamped(0, src.width - 1, endX),
                JUtilities.InverseLerpUnclamped(0, src.height - 1, endY));
            Material mat = JInternalMaterials.CopyTextureMaterial;
            mat.SetTexture("_MainTex", src);
            mat.SetVector("_StartUV", startUV);
            mat.SetVector("_EndUV", endUV);
            mat.SetColor("_DefaultColor", defaultColor);
            mat.SetPass(0);
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture.active = rt;
            Graphics.Blit(src, mat);
            RenderTexture.active = null;

            return rt;
        }

        public static void DrawTexture(RenderTexture rt, Texture texture, Rect uvRect, Material mat, int pass = 0)
        {
            if (mat == null)
                mat = JInternalMaterials.UnlitTextureMaterial;
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetTexture("_MainTex", texture);
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(uvRect.min.x, uvRect.min.y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(uvRect.min.x, uvRect.max.y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(uvRect.max.x, uvRect.max.y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(uvRect.max.x, uvRect.min.y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawTriangle(RenderTexture rt, Vector2 v0, Vector2 v1, Vector2 v2, Color c)
        {
            Material mat = JInternalMaterials.SolidColorMaterial;
            mat.SetColor("_Color", c);
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);
            GL.Vertex3(v0.x, v0.y, 0);
            GL.Vertex3(v1.x, v1.y, 0);
            GL.Vertex3(v2.x, v2.y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawQuad(RenderTexture rt, Vector2[] quadCorners, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static List<System.Type> GetAllLoadedTypes()
        {
            List<System.Type> loadedTypes = new List<System.Type>();
            List<string> typeName = new List<string>();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (t.IsVisible && !t.IsGenericType)
                    {
                        typeName.Add(t.Name);
                        loadedTypes.Add(t);
                    }
                }
            }
            return loadedTypes;
        }

        public static IEnumerable<Rect> CompareHeightMap(int gridSize, Color[] oldValues, Color[] newValues)
        {
            if (oldValues.LongLength != newValues.LongLength)
            {
                return new Rect[1] { new Rect(0, 0, 1, 1) };
            }
            Rect[] rects = new Rect[gridSize * gridSize];
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    rects[JUtilities.To1DIndex(x, z, gridSize)] = GetUvRange(gridSize, x, z);
                }
            }

            HashSet<Rect> dirtyRects = new HashSet<Rect>();

            int index = 0;
            int resolution = Mathf.RoundToInt(Mathf.Sqrt(newValues.LongLength));
            for (int rectIndex = 0; rectIndex < rects.Length; ++rectIndex)
            {
                Rect r = rects[rectIndex];
                int startX = (int)Mathf.Lerp(0, resolution - 1, r.min.x);
                int startY = (int)Mathf.Lerp(0, resolution - 1, r.min.y);
                int endX = (int)Mathf.Lerp(0, resolution - 1, r.max.x);
                int endY = (int)Mathf.Lerp(0, resolution - 1, r.max.y);
                for (int x = startX; x <= endX; ++x)
                {
                    for (int y = startY; y <= endY; ++y)
                    {
                        index = JUtilities.To1DIndex(x, y, resolution);
                        if (oldValues[index].r == newValues[index].r &&
                            oldValues[index].g == newValues[index].g &&
                            oldValues[index].b == newValues[index].b &&
                            oldValues[index].a == newValues[index].a)
                            continue;
                        dirtyRects.Add(r);

                        Rect hRect = new Rect();
                        hRect.size = new Vector2(r.width * 1.2f, r.height);
                        hRect.center = r.center;
                        dirtyRects.Add(hRect);

                        Rect vRect = new Rect();
                        vRect.size = new Vector2(r.width, r.height * 1.2f);
                        vRect.center = r.center;
                        dirtyRects.Add(vRect);
                        break;
                    }
                    if (dirtyRects.Contains(r))
                        break;
                }
            }

            return dirtyRects;
        }

        public static Rect GetUvRange(int gridSize, int x, int z)
        {
            Vector2 position = new Vector2(x * 1.0f / gridSize, z * 1.0f / gridSize);
            Vector2 size = Vector2.one / gridSize;
            return new Rect(position, size);
        }

        public static Texture2D CreateTextureFromCurve(AnimationCurve curve, int width, int height)
        {
            Texture2D t = new Texture2D(width, height, TextureFormat.ARGB32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            Color[] colors = new Color[width * height];
            for (int x = 0; x < width; ++x)
            {
                float f = Mathf.InverseLerp(0, width - 1, x);
                float value = curve.Evaluate(f);
                Color c = new Color(value, value, value, value);
                for (int y = 0; y < height; ++y)
                {
                    colors[JUtilities.To1DIndex(x, y, width)] = c;
                }
            }
            t.filterMode = FilterMode.Bilinear;
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Vector3[] GetBrushQuadCorners(Vector3 center, float radius, float rotation)
        {
            Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, rotation, 0));
            Vector3[] corners = new Vector3[]
            {
                center + matrix.MultiplyPoint(new Vector3(-1,0,-1)*radius),
                center + matrix.MultiplyPoint(new Vector3(-1,0,1)*radius),
                center + matrix.MultiplyPoint(new Vector3(1,0,1)*radius),
                center + matrix.MultiplyPoint(new Vector3(1,0,-1)*radius)
            };
            return corners;
        }

        //public static void RegisterBeginRender(Camera.CameraCallback callback)
        //{
        //    Camera.onPreCull += callback;
        //}

        //public static void RegisterBeginRenderSRP(System.Action<Camera> callback)
        //{
        //    RenderPipelineManager.beginCameraRendering += callback;
        //}

        //public static void UnregisterBeginRender(Camera.CameraCallback callback)
        //{
        //    Camera.onPreCull -= callback;
        //}

        //public static void UnregisterBeginRenderSRP(System.Action<Camera> callback)
        //{
        //    RenderPipeline.beginCameraRendering -= callback;
        //}

        //public static void RegisterEndRender(Camera.CameraCallback callback)
        //{
        //    Camera.onPostRender += callback;
        //}

        public static void ClearRT(RenderTexture rt)
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
        }

        public static void SetMaterialKeywordActive(Material mat, string keyword, bool active)
        {
            if (active)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }

        public static void Editor_ProgressBar(string title, string detail, float percent)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar(title, detail, percent);
#endif
        }

        public static void Editor_CancelableProgressBar(string title, string detail, float percent)
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayCancelableProgressBar(title, detail, percent))
            {
                throw new JProgressCancelledException();
            }
#endif
        }

        public static void Editor_ClearProgressBar()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        public static Camera CreateCamera()
        {
            GameObject g = new GameObject();
            Camera cam = g.AddComponent<Camera>();
            return cam;
        }
    }
}
