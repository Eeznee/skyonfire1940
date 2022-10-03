using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Jupiter;
using UnityEditor;
using System.IO;

namespace Pinwheel.Jupiter
{
    public class JCubemapCreatorWindow : EditorWindow
    {
        public Vector3 CameraPosition { get; set; }
        public float CameraNearPlane { get; set; }
        public float CameraFarPlane { get; set; }
        public CameraClearFlags CameraClearFlag { get; set; }
        public Color CameraBackgroundColor { get; set; }
        public int Resolution { get; set; }
        public bool ExportFaceTextures { get; set; }
        public string Directory { get; set; }
        
        public static readonly int[] ResolutionValues = new int[] { 16, 32, 64, 128, 256, 512, 1024, 2048 };
        public static readonly string[] ResolutionLabels = new string[] { "16", "32", "64", "128", "256", "512", "1024", "2048" };

        public const string PREF_PREFIX = "cubemap-creator";
        public const string CAM_POS_X = "cam-pos-x";
        public const string CAM_POS_Y = "cam-pos-y";
        public const string CAM_POS_Z = "cam-pos-z";
        public const string CAM_NEAR_PLANE = "cam-near-plane";
        public const string CAM_FAR_PLANE = "cam-far-plane";
        public const string CAM_CLEAR_FLAG = "cam-clear-flag";
        public const string CAM_BG_COLOR = "cam-background-color";
        public const string RESOLUTION = "resolution";
        public const string EXPORT_FACE_TEXTURES = "export-face-textures";
        public const string DIRECTORY = "directory";

        public static void ShowWindow()
        {
            JCubemapCreatorWindow window = EditorWindow.CreateInstance<JCubemapCreatorWindow>();
            window.titleContent = new GUIContent("Cubemap Creator");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            float x = EditorPrefs.GetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_X), 0);
            float y = EditorPrefs.GetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_Y), 0);
            float z = EditorPrefs.GetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_Z), 0);
            CameraPosition = new Vector3(x, y, z);
            CameraNearPlane = EditorPrefs.GetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_NEAR_PLANE), 0);
            CameraFarPlane = EditorPrefs.GetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_FAR_PLANE), 1000);
            CameraClearFlag = (CameraClearFlags)EditorPrefs.GetInt(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_CLEAR_FLAG), 0);
            string htmlColor = EditorPrefs.GetString(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_BG_COLOR), "FFFFFF");
            Color c = Color.white;
            ColorUtility.TryParseHtmlString("#" + htmlColor, out c);
            CameraBackgroundColor = c;
            Resolution = EditorPrefs.GetInt(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, RESOLUTION), 512);
            ExportFaceTextures = EditorPrefs.GetBool(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_FACE_TEXTURES), false);
            Directory = EditorPrefs.GetString(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY), "Assets/");
        }

        private void OnDisable()
        {
            EditorPrefs.SetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_X), CameraPosition.x);
            EditorPrefs.SetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_Y), CameraPosition.y);
            EditorPrefs.SetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_POS_Z), CameraPosition.z);
            EditorPrefs.SetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_NEAR_PLANE), CameraNearPlane);
            EditorPrefs.SetFloat(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_FAR_PLANE), CameraFarPlane);
            EditorPrefs.SetInt(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_CLEAR_FLAG), (int)CameraClearFlag);
            EditorPrefs.SetString(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CAM_BG_COLOR), ColorUtility.ToHtmlStringRGB(CameraBackgroundColor));
            EditorPrefs.SetInt(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, RESOLUTION), Resolution);
            EditorPrefs.SetBool(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_FACE_TEXTURES), ExportFaceTextures);
            EditorPrefs.SetString(JEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY), Directory);
        }

        private void OnGUI()
        {
            DrawExportGUI();
        }

        private void DrawExportGUI()
        {
            string label = "Export";
            string id = "export" + GetInstanceID();

            JEditorCommon.Foldout(label, true, id, () =>
            {
                CameraPosition = JEditorCommon.InlineVector3Field("Position", CameraPosition);
                CameraNearPlane = EditorGUILayout.FloatField("Near Plane", CameraNearPlane);
                CameraFarPlane = EditorGUILayout.FloatField("Far Plane", CameraFarPlane);
                CameraClearFlag = (CameraClearFlags)EditorGUILayout.EnumPopup("Clear Flags", CameraClearFlag);
                if (CameraClearFlag == CameraClearFlags.Color)
                {
                    CameraBackgroundColor = EditorGUILayout.ColorField("Background Color", CameraBackgroundColor);
                }

                Resolution = EditorGUILayout.IntPopup("Resolution", Resolution, ResolutionLabels, ResolutionValues);
                ExportFaceTextures = EditorGUILayout.Toggle("Export Face Textures", ExportFaceTextures);

                string dir = Directory;
                JEditorCommon.BrowseFolder("Directory", ref dir);
                Directory = dir;

                GUI.enabled = !string.IsNullOrEmpty(Directory);
                if (GUILayout.Button("Export"))
                {
                    Export();
                }
                GUI.enabled = true;
            });
        }

        private void Export()
        {
            JUtilities.EnsureDirectoryExists(Directory);

            Cubemap cube = new Cubemap(Resolution, TextureFormat.ARGB32, false);
            JCubemapRendererArgs args = new JCubemapRendererArgs()
            {
                CameraPosition = this.CameraPosition,
                CameraNearPlane = this.CameraNearPlane,
                CameraFarPlane = this.CameraFarPlane,
                CameraClearFlag = this.CameraClearFlag,
                CameraBackgroundColor = this.CameraBackgroundColor,
                Resolution = this.Resolution,
                Cubemap = cube,
                Face = (CubemapFace)63
            };
            JCubemapRenderer.Render(args);

            string fileName = Path.Combine(Directory, "Cubemap-" + JCommon.GetUniqueID() + ".cubemap");
            AssetDatabase.CreateAsset(cube, fileName);

            if (ExportFaceTextures)
            {
                ExportFace(cube, CubemapFace.PositiveX);
                ExportFace(cube, CubemapFace.NegativeX);
                ExportFace(cube, CubemapFace.PositiveY);
                ExportFace(cube, CubemapFace.NegativeY);
                ExportFace(cube, CubemapFace.PositiveZ);
                ExportFace(cube, CubemapFace.NegativeZ);
            }

            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(cube);
            Selection.activeObject = cube;
        }

        private void ExportFace(Cubemap cube, CubemapFace face)
        {
            Texture2D tex = new Texture2D(cube.width, cube.height);
            Color[] data = cube.GetPixels(face);
            Color[] flipData = new Color[data.Length];
            for (int y = 0; y < Resolution; ++y)
            {
                for (int x = 0; x < Resolution; ++x)
                {
                    flipData[JUtilities.To1DIndex(x, y, Resolution)] = data[JUtilities.To1DIndex(Resolution - 1 - x, Resolution - 1 - y, Resolution)];
                }
            }
            tex.SetPixels(flipData);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string fileName = Path.Combine(Directory, cube.name + "-" + face.ToString() + ".png");
            File.WriteAllBytes(fileName, bytes);
            JUtilities.DestroyObject(tex);
        }
    }
}
