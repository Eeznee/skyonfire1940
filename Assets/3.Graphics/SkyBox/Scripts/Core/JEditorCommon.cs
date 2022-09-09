#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Type = System.Type;
using System.Reflection;
using Action = System.Action;
using UnityEditor.AnimatedValues;

namespace Pinwheel.Jupiter
{
    public static partial class JEditorCommon
    {
        public static string[] DOT_ANIM = new string[4] { "   ", "●  ", "●● ", "●●●" };
        public static string caretRight = "►";
        public static string caretDown = "▼";
        public static string contextIconText = "≡";
        public static string dot = "●";

        public static float standardHeight = EditorGUIUtility.singleLineHeight;
        public static float tinyHeight = EditorGUIUtility.singleLineHeight;

        public static float standardWidth = 100;
        public static float standardWidthExtent = 200;
        public static float tinyWidth = EditorGUIUtility.singleLineHeight;
        public static float indentSpace = 11;
        public static float objectSelectorDragDropHeight = 55;
        public static Vector2 selectionGridTileSizeSmall = new Vector2(50, 50);
        public static Vector2 selectionGridTileSizeMedium = new Vector2(75, 75);
        public static Vector2 selectionGridTileSizeWide = new Vector2(95, 17);

        public static Color oddItemColor = EditorGUIUtility.isProSkin ? new Color32(55, 55, 55, 255) : new Color32(190, 190, 190, 255);
        public static Color evenItemColor = EditorGUIUtility.isProSkin ? new Color32(50, 50, 50, 255) : new Color32(180, 180, 180, 255);
        public static Color selectedItemColor = EditorGUIUtility.isProSkin ? new Color32(62, 95, 150, 255) : new Color32(62, 125, 231, 255);
        public static Color criticalItemColor = new Color32(220, 40, 0, 255);
        public static Color lightSkinInspectorHeaderBackground = new Color32(221, 221, 221, 255);
        public static Color lightGrey = new Color32(176, 176, 176, 255);
        public static Color midGrey = new Color32(128, 128, 128, 255);
        public static Color darkGrey = new Color32(64, 64, 64, 255);
        public static Color assetPreviewGrey = new Color32(82, 82, 82, 255);
        public static Color linkColor = new Color(0, 0, 238, 255); //#0000EE

        public static Color boxBorderColor = EditorGUIUtility.isProSkin ? new Color32(36, 36, 36, 255) : new Color32(127, 127, 127, 255);
        public static Color boxHeaderBg = EditorGUIUtility.isProSkin ? new Color32(53, 53, 53, 255) : new Color32(182, 182, 182, 255);
        public static Color boxBodyBg = EditorGUIUtility.isProSkin ? new Color32(65, 65, 65, 255) : new Color32(200, 200, 200, 255);

        public static RectOffset boxOffset = new RectOffset(2, 2, 2, 2);

        private static GUIStyle centeredMiniLabel;
        public static GUIStyle CenteredMiniLabel
        {
            get
            {
                if (centeredMiniLabel == null)
                {
                    centeredMiniLabel = new GUIStyle(EditorStyles.miniLabel);
                    centeredMiniLabel.alignment = TextAnchor.MiddleCenter;
                    //centeredMiniLabel.fontSize = 8;
                }

                return centeredMiniLabel;
            }
        }

        private static GUIStyle wordWrapLeftLabel;
        public static GUIStyle WordWrapLeftLabel
        {
            get
            {
                if (wordWrapLeftLabel == null)
                {
                    wordWrapLeftLabel = new GUIStyle(EditorStyles.label);
                    wordWrapLeftLabel.alignment = TextAnchor.MiddleLeft;
                    wordWrapLeftLabel.wordWrap = true;
                }

                return wordWrapLeftLabel;
            }
        }

        private static GUIStyle wordWrapItalicLabel;
        public static GUIStyle WordWrapItalicLabel
        {
            get
            {
                if (wordWrapItalicLabel == null)
                {
                    wordWrapItalicLabel = new GUIStyle(EditorStyles.label);
                    wordWrapItalicLabel.alignment = TextAnchor.MiddleLeft;
                    wordWrapItalicLabel.wordWrap = true;
                    wordWrapItalicLabel.fontStyle = FontStyle.Italic;
                }

                return wordWrapItalicLabel;
            }
        }

        private static GUIStyle centeredLabel;
        public static GUIStyle CenteredLabel
        {
            get
            {
                if (centeredLabel == null)
                {
                    centeredLabel = new GUIStyle(EditorStyles.label);
                    centeredLabel.alignment = TextAnchor.MiddleCenter;
                    centeredLabel.wordWrap = true;
                }

                return centeredLabel;
            }
        }

        private static GUIStyle centeredBoldLabel;
        public static GUIStyle CenteredBoldLabel
        {
            get
            {
                if (centeredBoldLabel == null)
                {
                    centeredBoldLabel = new GUIStyle(EditorStyles.label);
                    centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                    centeredBoldLabel.wordWrap = true;
                    centeredBoldLabel.fontStyle = FontStyle.Bold;
                }

                return centeredBoldLabel;
            }
        }

        private static GUIStyle centeredWhiteLabel;
        public static GUIStyle CenteredWhiteLabel
        {
            get
            {
                if (centeredWhiteLabel == null)
                {
                    centeredWhiteLabel = new GUIStyle();
                    centeredWhiteLabel.alignment = TextAnchor.MiddleCenter;
                    centeredWhiteLabel.normal.textColor = Color.white;
                    centeredWhiteLabel.wordWrap = true;
                }

                return centeredWhiteLabel;
            }
        }

        private static GUIStyle linkLabel;
        public static GUIStyle LinkLabel
        {
            get
            {
                if (linkLabel == null)
                {
                    linkLabel = new GUIStyle(CenteredLabel);
                    linkLabel.normal.textColor = linkColor;
                }
                return linkLabel;
            }
        }

        public static GUIStyle BoldLabel
        {
            get
            {
                return EditorStyles.boldLabel;
            }
        }

        private static GUIStyle italicLabel;
        public static GUIStyle ItalicLabel
        {
            get
            {
                if (italicLabel == null)
                {
                    italicLabel = new GUIStyle(EditorStyles.label);
                    italicLabel.fontStyle = FontStyle.Italic;
                    italicLabel.alignment = TextAnchor.MiddleLeft;
                }
                return italicLabel;
            }
        }

        private static GUIStyle boldHeader;
        public static GUIStyle BoldHeader
        {
            get
            {
                if (boldHeader == null)
                {
                    boldHeader = new GUIStyle();
                    boldHeader.fontStyle = FontStyle.Bold;
                    boldHeader.alignment = TextAnchor.MiddleLeft;
                }
                return boldHeader;
            }
        }

        private static GUIStyle rightAlignedItalicLabel;
        public static GUIStyle RightAlignedItalicLabel
        {
            get
            {
                if (rightAlignedItalicLabel == null)
                {
                    rightAlignedItalicLabel = new GUIStyle(ItalicLabel);
                    rightAlignedItalicLabel.alignment = TextAnchor.MiddleRight;
                }
                return rightAlignedItalicLabel;
            }
        }

        private static GUIStyle rightAlignedItalicWhiteLabel;
        public static GUIStyle RightAlignedItalicWhiteLabel
        {
            get
            {
                if (rightAlignedItalicWhiteLabel == null)
                {
                    rightAlignedItalicWhiteLabel = new GUIStyle();
                    rightAlignedItalicWhiteLabel.fontStyle = FontStyle.Italic;
                    rightAlignedItalicWhiteLabel.fontSize = 12;
                    rightAlignedItalicWhiteLabel.alignment = TextAnchor.MiddleRight;
                    rightAlignedItalicWhiteLabel.normal.textColor = Color.white;
                }
                return rightAlignedItalicWhiteLabel;
            }
        }

        private static GUIStyle rightAlignedWhiteTinyLabel;
        public static GUIStyle RightAlignedWhiteTinyLabel
        {
            get
            {
                if (rightAlignedWhiteTinyLabel == null)
                {
                    rightAlignedWhiteTinyLabel = new GUIStyle(EditorStyles.whiteMiniLabel);
                    rightAlignedWhiteTinyLabel.alignment = TextAnchor.MiddleRight;
                    rightAlignedWhiteTinyLabel.normal.textColor = Color.white;
                }
                return rightAlignedWhiteTinyLabel;
            }
        }

        private static Texture2D sliderBackgroundTexture;
        public static Texture2D SliderBackgroundTexture
        {
            get
            {
                if (sliderBackgroundTexture == null)
                {
                    //sliderBackgroundTexture = EditorGUIUtility.Load("icons/sv_icon_dot0_sml.png") as Texture2D;
                    sliderBackgroundTexture = Texture2D.whiteTexture;
                }
                return sliderBackgroundTexture;
            }
        }

        private static Texture2D warningIcon;
        public static Texture2D WarningIcon
        {
            get
            {
                if (warningIcon == null)
                {
                    warningIcon = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
                }
                return warningIcon;
            }
        }

        private static Texture2D oddItemTexture1x1;
        public static Texture2D OddItemTexture1x1
        {
            get
            {
                if (oddItemTexture1x1 == null)
                {
                    oddItemTexture1x1 = new Texture2D(1, 1);
                    oddItemTexture1x1.SetPixels(new Color[] { oddItemColor });
                    oddItemTexture1x1.Apply();
                }
                return oddItemTexture1x1;
            }
        }

        private static Texture2D oddItemHoveredTexture1x1;
        public static Texture2D OddItemHoveredTexture1x1
        {
            get
            {
                if (oddItemHoveredTexture1x1 == null)
                {
                    oddItemHoveredTexture1x1 = new Texture2D(1, 1);
                    oddItemHoveredTexture1x1.SetPixels(new Color[] { oddItemColor * 1.2f });
                    oddItemHoveredTexture1x1.Apply();
                }
                return oddItemHoveredTexture1x1;
            }
        }

        private static Texture2D oddItemClickedTexture1x1;
        public static Texture2D OddItemClickedTexture1x1
        {
            get
            {
                if (oddItemClickedTexture1x1 == null)
                {
                    oddItemClickedTexture1x1 = new Texture2D(1, 1);
                    oddItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    oddItemClickedTexture1x1.Apply();
                }
                return oddItemClickedTexture1x1;
            }
        }

        private static Texture2D evenItemTexture1x1;
        public static Texture2D EvenItemTexture1x1
        {
            get
            {
                if (evenItemTexture1x1 == null)
                {
                    evenItemTexture1x1 = new Texture2D(1, 1);
                    evenItemTexture1x1.SetPixels(new Color[] { evenItemColor });
                    evenItemTexture1x1.Apply();
                }
                return evenItemTexture1x1;
            }
        }

        private static Texture2D evenItemHoveredTexture1x1;
        public static Texture2D EvenItemHoveredTexture1x1
        {
            get
            {
                if (evenItemHoveredTexture1x1 == null)
                {
                    evenItemHoveredTexture1x1 = new Texture2D(1, 1);
                    evenItemHoveredTexture1x1.SetPixels(new Color[] { evenItemColor * 1.2f });
                    evenItemHoveredTexture1x1.Apply();
                }
                return evenItemHoveredTexture1x1;
            }
        }

        private static Texture2D evenItemClickedTexture1x1;
        public static Texture2D EvenItemClickedTexture1x1
        {
            get
            {
                if (evenItemClickedTexture1x1 == null)
                {
                    evenItemClickedTexture1x1 = new Texture2D(1, 1);
                    evenItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    evenItemClickedTexture1x1.Apply();
                }
                return evenItemClickedTexture1x1;
            }
        }

        private static Texture2D selectedItemTexture1x1;
        public static Texture2D SelectedItemTexture1x1
        {
            get
            {
                if (selectedItemTexture1x1 == null)
                {
                    selectedItemTexture1x1 = new Texture2D(1, 1);
                    selectedItemTexture1x1.SetPixels(new Color[] { selectedItemColor });
                    selectedItemTexture1x1.Apply();
                }
                return selectedItemTexture1x1;
            }
        }

        private static Texture2D selectedItemHoveredTexture1x1;
        public static Texture2D SelectedItemHoveredTexture1x1
        {
            get
            {
                if (selectedItemHoveredTexture1x1 == null)
                {
                    selectedItemHoveredTexture1x1 = new Texture2D(1, 1);
                    selectedItemHoveredTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    selectedItemHoveredTexture1x1.Apply();
                }
                return selectedItemHoveredTexture1x1;
            }
        }

        private static Texture2D selectedItemClickedTexture1x1;
        public static Texture2D SelectedItemClickedTexture1x1
        {
            get
            {
                if (selectedItemClickedTexture1x1 == null)
                {
                    selectedItemClickedTexture1x1 = new Texture2D(1, 1);
                    selectedItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.4f });
                    selectedItemClickedTexture1x1.Apply();
                }
                return selectedItemClickedTexture1x1;
            }
        }

        private static GUIStyle oddFlatButton;
        public static GUIStyle OddFlatButton
        {
            get
            {
                if (oddFlatButton == null)
                {
                    oddFlatButton = new GUIStyle();
                    oddFlatButton.alignment = TextAnchor.MiddleCenter;
                    oddFlatButton.normal.background = OddItemTexture1x1;
                    oddFlatButton.hover.background = OddItemHoveredTexture1x1;
                    oddFlatButton.active.background = OddItemClickedTexture1x1;
                }
                return oddFlatButton;
            }
        }

        private static GUIStyle evenFlatButton;
        public static GUIStyle EvenFlatButton
        {
            get
            {
                if (evenFlatButton == null)
                {
                    evenFlatButton = new GUIStyle();
                    evenFlatButton.alignment = TextAnchor.MiddleCenter;
                    evenFlatButton.normal.background = EvenItemTexture1x1;
                    evenFlatButton.hover.background = EvenItemHoveredTexture1x1;
                    evenFlatButton.active.background = EvenItemClickedTexture1x1;
                }
                return evenFlatButton;
            }
        }

        private static GUIStyle selectedFlatButton;
        public static GUIStyle SelectedFlatButton
        {
            get
            {
                if (selectedFlatButton == null)
                {
                    selectedFlatButton = new GUIStyle();
                    selectedFlatButton.alignment = TextAnchor.MiddleCenter;
                    selectedFlatButton.normal.background = SelectedItemTexture1x1;
                    selectedFlatButton.hover.background = SelectedItemHoveredTexture1x1;
                    selectedFlatButton.active.background = SelectedItemClickedTexture1x1;
                }
                return selectedFlatButton;
            }
        }

        private static Texture2D criticalItemTexture1x1;
        public static Texture2D CriticalItemTexture1x1
        {
            get
            {
                if (criticalItemTexture1x1 == null)
                {
                    criticalItemTexture1x1 = new Texture2D(1, 1);
                    criticalItemTexture1x1.SetPixels(new Color[] { criticalItemColor });
                    criticalItemTexture1x1.Apply();
                }
                return criticalItemTexture1x1;
            }
        }

        private static Texture2D criticalItemHoveredTexture1x1;
        public static Texture2D CriticalItemHoveredTexture1x1
        {
            get
            {
                if (criticalItemHoveredTexture1x1 == null)
                {
                    criticalItemHoveredTexture1x1 = new Texture2D(1, 1);
                    criticalItemHoveredTexture1x1.SetPixels(new Color[] { criticalItemColor * 1.2f });
                    criticalItemHoveredTexture1x1.Apply();
                }
                return criticalItemHoveredTexture1x1;
            }
        }

        private static Texture2D criticalItemClickedTexture1x1;
        public static Texture2D CriticalItemClickedTexture1x1
        {
            get
            {
                if (criticalItemClickedTexture1x1 == null)
                {
                    criticalItemClickedTexture1x1 = new Texture2D(1, 1);
                    criticalItemClickedTexture1x1.SetPixels(new Color[] { criticalItemColor * 1.4f });
                    criticalItemClickedTexture1x1.Apply();
                }
                return criticalItemClickedTexture1x1;
            }
        }

        private static GUIStyle criticalFlatButton;
        public static GUIStyle CriticalFlatButton
        {
            get
            {
                if (criticalFlatButton == null)
                {
                    criticalFlatButton = new GUIStyle();
                    criticalFlatButton.alignment = TextAnchor.MiddleCenter;
                    criticalFlatButton.normal.background = CriticalItemTexture1x1;
                    criticalFlatButton.hover.background = CriticalItemHoveredTexture1x1;
                    criticalFlatButton.active.background = CriticalItemClickedTexture1x1;
                    criticalFlatButton.normal.textColor = Color.white;
                    criticalFlatButton.hover.textColor = Color.white;
                    criticalFlatButton.active.textColor = Color.white;
                    criticalFlatButton.fontStyle = FontStyle.Bold;
                }
                return criticalFlatButton;
            }
        }

        private static GUIStyle richTextLabel;
        public static GUIStyle RichTextLabel
        {
            get
            {
                if (richTextLabel == null)
                {
                    richTextLabel = new GUIStyle(EditorStyles.label);
                }
                richTextLabel.richText = true;
                richTextLabel.alignment = TextAnchor.MiddleLeft;
                richTextLabel.wordWrap = true;
                return richTextLabel;
            }
        }

        private static string projectName;
        public static string ProjectName
        {
            get
            {
                if (string.IsNullOrEmpty(projectName))
                {
                    string[] s = Application.dataPath.Split('/', '\\');
                    projectName = s[s.Length - 2];
                }
                return projectName;
            }

        }

        public static string GetProjectRelatedEditorPrefsKey(params string[] keyElements)
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder(ProjectName);
            for (int i = 0; i < keyElements.Length; ++i)
            {
                b.Append("-").Append(keyElements[i]);
            }
            return b.ToString();
        }

        /// <summary>
        /// Draw a tab view and return the currently selected tab
        /// </summary>
        /// <param name="current"></param>
        /// <param name="tabLabels"></param>
        /// <returns></returns>
        public static int Tabs(int current, params string[] tabLabels)
        {
            if (tabLabels.Length == 0)
                return current;
            int selectedTab = current;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < tabLabels.Length; ++i)
            {
                GUIStyle style =
                    i == 0 && tabLabels.Length > 1 ? EditorStyles.miniButtonLeft :
                    i == tabLabels.Length - 1 && tabLabels.Length > 1 ? EditorStyles.miniButtonRight :
                    i > 0 && i < tabLabels.Length - 1 && tabLabels.Length > 1 ? EditorStyles.miniButtonMid :
                    EditorStyles.miniButton;
                GUI.backgroundColor = i == current ? Color.gray : Color.white;
                if (GUILayout.Button("", style))
                {
                    selectedTab = i;
                }
                GUI.backgroundColor = Color.white;

                GUIStyle labelStyle = i == current ? CenteredWhiteLabel : CenteredLabel;
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), tabLabels[i], labelStyle);
            }
            EditorGUILayout.EndHorizontal();
            return selectedTab;
        }

        public static int Tabs(int current, params GUIContent[] tabContents)
        {
            if (tabContents.Length == 0)
                return current;
            float maxIconHeight = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < tabContents.Length; ++i)
            {
                if (tabContents[i].image != null && tabContents[i].image.height > maxIconHeight)
                    maxIconHeight = tabContents[i].image.height;
            }
            int selectedTab = current;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < tabContents.Length; ++i)
            {
                GUIStyle style =
                    i == 0 && tabContents.Length > 1 ? EditorStyles.miniButtonLeft :
                    i == tabContents.Length - 1 && tabContents.Length > 1 ? EditorStyles.miniButtonRight :
                    i > 0 && i < tabContents.Length - 1 && tabContents.Length > 1 ? EditorStyles.miniButtonMid :
                    EditorStyles.miniButton;
                GUI.backgroundColor = i == current ? Color.gray : Color.white;
                if (GUILayout.Button("", style, GUILayout.Height(maxIconHeight)))
                {
                    selectedTab = i;
                }
                GUI.backgroundColor = Color.white;

                GUIStyle labelStyle = i == current ? CenteredWhiteLabel : CenteredLabel;
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), tabContents[i], labelStyle);
            }
            EditorGUILayout.EndHorizontal();
            return selectedTab;
        }

        public static int Tabs(int current, float height, params GUIContent[] tabContents)
        {
            if (tabContents.Length == 0)
                return current;

            int selectedTab = current;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < tabContents.Length; ++i)
            {
                GUIStyle style =
                    i == 0 && tabContents.Length > 1 ? EditorStyles.miniButtonLeft :
                    i == tabContents.Length - 1 && tabContents.Length > 1 ? EditorStyles.miniButtonRight :
                    i > 0 && i < tabContents.Length - 1 && tabContents.Length > 1 ? EditorStyles.miniButtonMid :
                    EditorStyles.miniButton;
                GUI.backgroundColor = i == current ? Color.gray : Color.white;
                if (GUILayout.Button("", style, GUILayout.Height(height)))
                {
                    selectedTab = i;
                }
                GUI.backgroundColor = Color.white;

                GUIStyle labelStyle = i == current ? CenteredWhiteLabel : CenteredLabel;
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), tabContents[i], labelStyle);
            }
            EditorGUILayout.EndHorizontal();
            return selectedTab;
        }

        /// <summary>
        /// Add space between controls
        /// </summary>
        /// <param name="amount"></param>
        public static void Space(int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
            {
                EditorGUILayout.Space();
            }
        }

        public static void SpacePixel(int pixel)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(pixel));
        }

        /// <summary>
        /// Draw a horizontal line
        /// </summary>
        public static void Separator(bool useIndent = false)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(3));
            if (useIndent)
            {
                r = EditorGUI.IndentedRect(r);
            }
            Vector2 start = new Vector2(r.min.x - 2, (r.min.y + r.max.y) / 2);
            Vector2 end = new Vector2(r.max.x + 1, (r.min.y + r.max.y) / 2);
            Handles.BeginGUI();
            Handles.color = boxBorderColor;
            Handles.DrawLine(start, end);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a button which anchors to the right edge of the window
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool RightAnchoredButton(string label)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool btnClicked = GUILayout.Button(label, GUILayout.Width(standardWidth), GUILayout.Height(standardHeight));
            EditorGUILayout.EndHorizontal();
            return btnClicked;
        }

        public static bool RightAnchoredButton(string label, float width, float height)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool btnClicked = GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
            return btnClicked;
        }

        public static bool Button(string label)
        {
            bool btnClicked = GUILayout.Button(label, GUILayout.Height(standardHeight));
            return btnClicked;
        }

        public static bool Button(string label, GUIStyle style)
        {
            bool btnClicked = GUILayout.Button(label, style, GUILayout.Height(standardHeight));
            return btnClicked;
        }

        /// <summary>
        /// Draw a tiny square button
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool TinyButton(string label)
        {
            return GUILayout.Button(label, GUILayout.Width(tinyWidth), GUILayout.Height(tinyHeight));
        }

        /// <summary>
        /// Draw a folder browser
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="result">The currently selected folder, this will be modified after user selects another folder</param>
        public static void BrowseFolder(string label, ref string result)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(label, result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("Browse", GUILayout.Width(standardWidth), GUILayout.Height(standardHeight)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                    result = FileUtil.GetProjectRelativePath(selectedFolder);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void BrowseFolderMiniButton(string label, ref string result)
        {
            int lastIndex = EditorGUI.indentLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField(result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("●", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                    result = FileUtil.GetProjectRelativePath(selectedFolder);
            }
            EditorGUI.indentLevel = lastIndex;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a file browser
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="result">The currently selected file path, this will be modified after selecting another file</param>
        /// <param name="filter">File extension filter</param>
        public static void BrowseFile(string label, ref string result, params string[] filter)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(label, result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("Browse", GUILayout.Width(standardWidth), GUILayout.Height(standardHeight)))
            {
                string selectedFile = EditorUtility.OpenFilePanelWithFilters("Select file", Application.dataPath, filter);
                if (!string.IsNullOrEmpty(selectedFile))
                    result = selectedFile;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a toggle group in horizontal layout, only one item can be selected at a time
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="selected">Currently selected item index</param>
        /// <param name="toggleWidth">Width of each toggle</param>
        /// <param name="toggleLabels">Label for each toggle</param>
        /// <returns>Index of the selected item</returns>
        public static int ToggleGroup(string label, int selected, float toggleWidth, params string[] toggleLabels)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect r = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(r.x, r.y, toggleWidth, r.height);

            for (int i = 0; i < toggleLabels.Length; ++i)
            {
                toggleRect.position = r.position + new Vector2(i * toggleWidth, 0);
                if (EditorGUI.ToggleLeft(toggleRect, toggleLabels[i], i == selected))
                {
                    selected = i;
                }
            }

            EditorGUILayout.EndHorizontal();
            return selected;
        }

        /// <summary>
        /// Draw a toggle group in vertical layout, only one item can be selected at a time
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="selected">Currently selected item index</param>
        /// <param name="toggleHeight">Height of each toggle</param>
        /// <param name="toggleLabels">Label for each toggle</param>
        /// <returns>Index of the selected item</returns>
        public static int ToggleGroupVertical(string label, int selected, float toggleHeight, params string[] toggleLabels)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect r = EditorGUILayout.GetControlRect(false, toggleHeight * toggleLabels.Length);
            Rect toggleRect = new Rect(r.x, r.y, r.width, toggleHeight);

            for (int i = 0; i < toggleLabels.Length; ++i)
            {
                toggleRect.position = r.position + new Vector2(0, i * toggleHeight);
                if (EditorGUI.ToggleLeft(toggleRect, toggleLabels[i], i == selected))
                {
                    selected = i;
                }
            }

            EditorGUILayout.EndHorizontal();
            return selected;
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static bool LinkButton(string label, float width, float height)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));
            return LinkButton(r, label);
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool LinkButton(string label)
        {
            return LinkButton(label, standardWidth, standardHeight);
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="r"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool LinkButton(Rect r, string label)
        {
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            return GUI.Button(r, label, LinkLabel);
        }

        public static void DrawOutlineBox(Rect r, Color c)
        {
            Handles.BeginGUI();
            using (var scope = new Handles.DrawingScope(c))
            {
                Vector2 p1 = new Vector2(r.xMin, r.yMin);
                Vector2 p2 = new Vector2(r.xMax, r.yMin);
                Vector2 p3 = new Vector2(r.xMax, r.yMax);
                Vector2 p4 = new Vector2(r.xMin, r.yMax);
                Handles.DrawLines(new Vector3[]
                {
                    p1,p2,
                    p2,p3,
                    p3,p4,
                    p4,p1
                });
            }
            Handles.EndGUI();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color c)
        {
            Handles.BeginGUI();
            using (var scope = new Handles.DrawingScope(c))
            {
                Handles.DrawLine(start, end);
            }
            Handles.EndGUI();
        }

        public static void DrawBottomLine(Rect r, Color c)
        {
            DrawLine(
                new Vector2(r.min.x, r.max.y),
                new Vector2(r.max.x, r.max.y),
                c);
        }

        public static void DrawPlus(Rect r, Color c, float thickness)
        {
            Rect r0 = new Rect();
            r0.size = new Vector2(thickness, r.height);
            r0.center = r.center;
            EditorGUI.DrawRect(r0, c);

            Rect r1 = new Rect();
            r1.size = new Vector2(r.width, thickness);
            r1.center = r.center;
            EditorGUI.DrawRect(r1, c);
        }
        
        public static void SingleLineToggleIntField(string label, ref bool toggle, ref int value)
        {
            EditorGUILayout.BeginHorizontal();
            toggle = EditorGUILayout.Toggle(toggle);
            value = EditorGUILayout.DelayedIntField(label, value);
            EditorGUILayout.EndHorizontal();
        }
        
        public static T ObjectSelectorDragDrop<T>(Rect r, string message, string filter = "") where T : class
        {
            r = EditorGUI.IndentedRect(r);
            int controlId = EditorGUIUtility.GetControlID(FocusType.Passive, r);
            T objectToReturn = default(T);
            Color bgColor = EditorGUIUtility.isProSkin ? JEditorCommon.darkGrey : JEditorCommon.lightGrey;


            EditorGUI.DrawRect(r, bgColor);
            JEditorCommon.DrawOutlineBox(r, JEditorCommon.midGrey);

            Rect messageRect = new Rect();
            messageRect.size = new Vector2(r.width, 12);
            messageRect.center = r.center - Vector2.up * (messageRect.size.y * 0.5f + 2);
            EditorGUI.LabelField(messageRect, message, JEditorCommon.CenteredLabel);

            Rect buttonRect = new Rect();
            buttonRect.size = new Vector2(47, 15);
            buttonRect.center = r.center + Vector2.up * (buttonRect.size.y * 0.5f + 2);
            if (GUI.Button(buttonRect, "Browse", JEditorCommon.CenteredLabel))
            {
                EditorGUIUtility.ShowObjectPicker<Object>(null, false, filter, controlId);
            }
            JEditorCommon.DrawLine(
                new Vector2(buttonRect.min.x, buttonRect.max.y),
                new Vector2(buttonRect.max.x, buttonRect.max.y),
                CenteredLabel.normal.textColor);
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);

            Event e = Event.current;
            if (e != null &&
                r.Contains(e.mousePosition) &&
                (e.type == EventType.DragUpdated ||
                e.type == EventType.Repaint))
            {
                Object[] draggedObject = DragAndDrop.objectReferences;
                if (draggedObject.Length == 1 &&
                    draggedObject[0] is T)
                {
                    DragAndDrop.AcceptDrag();
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = controlId;
                    EditorGUI.DrawRect(r, JUtilities.GetColor(JEditorCommon.selectedItemColor, 0.5f));
                    JEditorCommon.DrawOutlineBox(r, JEditorCommon.selectedItemColor);
                }
            }
            else if (e != null && e.type == EventType.DragPerform && r.Contains(e.mousePosition))
            {
                Object[] draggedObject = DragAndDrop.objectReferences;
                if (draggedObject.Length == 1 &&
                    draggedObject[0] is T)
                {
                    objectToReturn = draggedObject[0] as T;
                }
            }
            else if (e != null && e.type == EventType.ExecuteCommand && e.commandName.Equals("ObjectSelectorClosed"))
            {
                int id = EditorGUIUtility.GetObjectPickerControlID();
                Object o = EditorGUIUtility.GetObjectPickerObject();
                if (id == controlId && o != null)
                {
                    objectToReturn = o as T;
                }
            }

            return objectToReturn;
        }
        
        public static void DrawGameObjectPreview(Rect r, object o)
        {
            GameObject g = (GameObject)o;
            if (g != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(g);
                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(r, preview, null, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(r, assetPreviewGrey);
                    EditorGUI.LabelField(r, "...", JEditorCommon.CenteredWhiteLabel);
                }
            }
        }
        
        public static Vector2 InlineVector2Field(string label, Vector2 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector2Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector3 InlineVector3Field(string label, Vector3 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector3Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector4 InlineVector4Field(string label, Vector4 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector4Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Texture2D InlineTexture2DField(string label, Texture2D value, int indentScope = 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(indentScope))
            {
                value = EditorGUILayout.ObjectField(value, typeof(Texture2D), false) as Texture2D;
            }
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static Cubemap InlineCubemapField(string label, Cubemap value, int indentScope = 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(indentScope))
            {
                value = EditorGUILayout.ObjectField(value, typeof(Cubemap), false) as Cubemap;
            }
            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static void OpenEmailEditor(string receiver, string subject, string body)
        {
            string url = string.Format(
                "mailto:{0}" +
                "?subject={1}" +
                "&body={2}",
                receiver,
                subject.Replace(" ", "%20"),
                body.Replace(" ", "%20"));

            Application.OpenURL(url);
        }
        
        public static string GetClassDisplayName(Type t)
        {
            string label = string.Empty;
            object[] alternativeClassNameAttributes = t.GetCustomAttributes(typeof(JDisplayName), false);
            if (alternativeClassNameAttributes != null && alternativeClassNameAttributes.Length > 0)
            {
                JDisplayName att = alternativeClassNameAttributes[0] as JDisplayName;
                if (att.DisplayName == null ||
                    att.DisplayName.Equals(string.Empty))
                    label = t.Name;
                else
                    label = att.DisplayName;
            }
            else
            {
                label = t.Name;
            }
            return label;
        }

        public static bool Foldout(string label, string prefkeys, bool defaultValue = true)
        {
            string prefKey = JEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", prefkeys);
            bool expanded = EditorPrefs.GetBool(prefKey, defaultValue);
            expanded = EditorGUILayout.Foldout(expanded, label);
            EditorPrefs.SetBool(prefKey, expanded);

            return expanded;
        }

        public static string Ellipsis(string text, int length)
        {
            if (text.Length <= length)
                return text;
            string substring = text.Substring(0, length);
            return substring + "...";
        }

        public static Rect BeginGroup()
        {
            Rect r = EditorGUILayout.BeginVertical();
            GUI.Box(JEditorCommon.boxOffset.Add(r), "");
            return r;
        }

        public static void EndGroup()
        {
            EditorGUILayout.EndVertical();
        }

        private static Dictionary<string, AnimBool> foldoutStates;
        private static Dictionary<string, AnimBool> FoldoutStates
        {
            get
            {
                if (foldoutStates == null)
                {
                    foldoutStates = new Dictionary<string, AnimBool>();
                }
                return foldoutStates;
            }
        }

        private static AnimBool GetFoldoutState(string id, bool defaultValue)
        {
            if (!FoldoutStates.ContainsKey(id))
            {
                AnimBool a = new AnimBool(defaultValue);
                FoldoutStates.Add(id, a);
            }

            return FoldoutStates[id];
        }

        public static void ExpandFoldout(string id)
        {
            string prefKey = JEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            EditorPrefs.SetBool(prefKey, true);
        }

        public static void DrawHeaderBox(Rect r)
        {
            EditorGUI.DrawRect(r, boxHeaderBg);
            DrawOutlineBox(r, boxBorderColor);
            //GUI.Box(r, "");
            //if (EditorGUIUtility.isProSkin)
            //    GUI.Box(r, "");
        }

        public static void DrawBodyBox(Rect r, bool shadow = true)
        {
            EditorGUI.DrawRect(r, boxBodyBg);
            DrawOutlineBox(r, boxBorderColor);

            if (shadow)
            {
                Vector2 start = new Vector2(r.min.x + 1, r.max.y + 1);
                Vector2 end = new Vector2(r.max.x - 1, r.max.y + 1);
                Color32 color = boxBorderColor;
                color.a = EditorGUIUtility.isProSkin ? (byte)100 : (byte)135;
                DrawLine(start, end, color);
            }

            //GUI.Box(r, "");
        }

        public static bool Foldout(string label, bool defaultExpanded, string id, Action innerGUI, GenericMenu context = null)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));

            string prefKey = GetProjectRelatedEditorPrefsKey("foldout", id);
            bool expanded = EditorPrefs.GetBool(prefKey, defaultExpanded);

            Rect headerRect = EditorGUILayout.BeginHorizontal();
            RectOffset headerBoxOffset = new RectOffset(2, 2, 1, 1);
            DrawHeaderBox(headerBoxOffset.Add(headerRect));

            Rect caretRect = EditorGUILayout.GetControlRect(GUILayout.Width(indentSpace));
            EditorGUI.LabelField(caretRect, expanded ? caretDown : caretRight, EditorStyles.miniLabel);
            Rect labelRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            if (context != null)
            {
                GUILayout.FlexibleSpace();
                Rect contextButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
                if (GUI.Button(contextButtonRect, contextIconText, EditorStyles.label))
                {
                    context.ShowAsContext();
                }
            }

            if (GUI.Button(headerRect, "", GUIStyle.none))
            {
                expanded = !expanded;
            }
            EditorGUILayout.EndHorizontal();

            EditorPrefs.SetBool(prefKey, expanded);

            if (expanded)
            {
                Rect bodyRect = EditorGUILayout.BeginVertical();
                RectOffset bodyBoxOffset = new RectOffset(2, 2, 2, 2);
                DrawBodyBox(bodyBoxOffset.Add(bodyRect));
            }

            if (expanded)
            {
                EditorGUI.indentLevel += 1;
                if (innerGUI != null)
                    innerGUI.Invoke();
                EditorGUI.indentLevel -= 1;
                SpacePixel(1);
            }

            if (expanded)
            {
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            return expanded;
        }


        public static void DrawPreview(Rect r, Object o)
        {
            EditorGUI.DrawRect(r, JEditorCommon.assetPreviewGrey);
            Texture2D preview = AssetPreview.GetAssetPreview(o);
            if (preview != null)
            {
                GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit, true);
            }
            else
            {
                if (AssetPreview.IsLoadingAssetPreview(o.GetInstanceID()))
                {
                    EditorGUI.LabelField(r, "Loading preview...", JEditorCommon.CenteredWhiteLabel);
                }
                else
                {
                    EditorGUI.LabelField(r, "Preview not available!", JEditorCommon.CenteredWhiteLabel);
                }
            }
        }

        public static void Box(string content)
        {
            Rect r = EditorGUILayout.BeginVertical();
            RectOffset offset = new RectOffset((int)indentSpace * EditorGUI.indentLevel, 0, 0, 0);
            GUI.Box(offset.Remove(r), "");
            EditorGUILayout.LabelField(content, WordWrapItalicLabel);
            EditorGUILayout.EndVertical();
        }

        public static T ScriptableObjectField<T>(string label, T asset) where T : ScriptableObject
        {
            EditorGUILayout.BeginHorizontal();
            asset = EditorGUILayout.ObjectField(label, asset, typeof(T), false) as T;
            if (asset == null)
            {
                if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    asset = ScriptableObject.CreateInstance<T>();
                    asset.name = string.Format("{0}-{1}", typeof(T).Name, JCommon.GetUniqueID());
                    AssetDatabase.CreateAsset(asset, string.Format("Assets/{0}.asset", asset.name));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(asset);
                }
            }
            EditorGUILayout.EndHorizontal();
            return asset;
        }
    }
}
#endif