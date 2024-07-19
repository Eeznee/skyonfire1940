#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NWH.NUI
{
    /// <summary>
    ///     A small Editor GUI library designed as a replacement for EditorGUILayout.
    /// </summary>
    public class NUIDrawer
    {
        public enum DrawerType
        {
            Property,
            Editor,
        }

        public string documentationBaseURL = "http://nwhvehiclephysics.com/doku.php/";

        public Rect positionRect = new Rect(-1, -1, -1, -1);
        public SerializedObject serializedObject;
        public SerializedProperty serializedProperty;
        public float totalHeight;

        private readonly float sidePadding = 6f;
        private GUIStyle bgStyle = new GUIStyle();
        private readonly GUIStyle unitStyle = new GUIStyle();
        private string _cachedKey;
        private DrawerType _drawerType = DrawerType.Property;
        private Rect customPositionRect = new Rect(-1, -1, -1, -1);


        private void InitializeGUIElements()
        {
            // Background style
            bgStyle = EditorStyles.helpBox;

            // Initialize styles
            unitStyle.fontSize = 9;
            unitStyle.alignment = TextAnchor.MiddleRight;
            unitStyle.normal.textColor = Color.grey;
        }


        public void UpdateCachedKey()
        {
            _cachedKey = GenerateKey();
        }


        public static string GenerateKey(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null) return "null";
            return GenerateKey(property.serializedObject) + property.propertyPath;
        }


        public static string GenerateKey(SerializedObject obj)
        {
            return obj.targetObject.GetInstanceID().ToString();
        }


        public static string GenerateKey(Object obj)
        {
            return obj.GetInstanceID().ToString();
        }


        public static int GetPropertyDepth(SerializedProperty property)
        {
            return property.propertyPath.Split('.').Length - 1;
        }


        public static void SetTabIndex(string tabName, int value)
        {
            //EditorCache.SetCachedValue("tabIndex", value, tabName);
            EditorPrefs.SetInt("NWH" + tabName, value);
        }


        public void AdvancePosition(float height = NUISettings.fieldHeight)
        {
            positionRect = new Rect(positionRect.x, positionRect.y + height, positionRect.width,
                                    NUISettings.fieldHeight);
            totalHeight += height;
        }



        public void BeginEditor(SerializedObject serializedObject)
        {
            _drawerType = DrawerType.Editor;
            this.serializedObject = serializedObject;
            serializedProperty = null;
            _cachedKey = GenerateKey();

            if (serializedObject == null)
            {
                Debug.LogError("Cannot draw editor for null serializedObject.");
                return;
            }

            serializedObject.Update();

            if (customPositionRect.x < 0 && customPositionRect.width < 0)
            {
                positionRect = EditorGUILayout.GetControlRect();
                positionRect.x = positionRect.x - 3;
                positionRect.width = positionRect.width + 6;
            }
            else
            {
                positionRect = customPositionRect;
            }

            totalHeight = 0;
            InitializeGUIElements();
            EditorGUI.BeginChangeCheck();

            DrawBackgroundRect(new Rect(positionRect.x, positionRect.y, positionRect.width, GetHeight()));
        }


        public void BeginProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                Debug.LogError("Cannot draw property drawer for null property.");
                return;
            }

            _drawerType = DrawerType.Property;
            serializedProperty = property;
            serializedObject = property.serializedObject;
            _cachedKey = GenerateKey();

            totalHeight = 0;
            positionRect = position;

            bool guiWasEnabled = true;
            if (EditorCache.GetGuiWasEnabledCValue(GenerateKey(), ref guiWasEnabled))
            {
                GUI.enabled = guiWasEnabled;
            }

            InitializeGUIElements();

            Rect totalRect = new Rect(positionRect.x, positionRect.y, positionRect.width, GetHeight());
            DrawBackgroundRect(new Rect(positionRect.x, positionRect.y, positionRect.width, GetHeight()));
            //EditorGUI.BeginProperty(totalRect, label, property);

            EditorGUI.BeginChangeCheck();
        }


        public void BeginSubsection(string label)
        {
            Title(label);
            IncreaseIndent();
        }


        public bool Button(string label, GUIStyle style = null)
        {
            Rect buttonRect = positionRect;
            buttonRect.y += 2f;
            buttonRect.height = NUISettings.fieldHeight - 4f;

            GUIStyle buttonStyle = style != null ? new GUIStyle(style) : new GUIStyle(EditorStyles.miniButton);
            buttonStyle.fixedHeight = buttonRect.height;

            bool buttonState = GUI.Button(buttonRect, label, buttonStyle);

            AdvancePosition(buttonRect.height + NUISettings.fieldSpacing);
            return buttonState;
        }


        public void DecreaseIndent()
        {
            positionRect = new Rect(positionRect.x - 6, positionRect.y, positionRect.width + 6, positionRect.height);
        }


        public void DrawBackgroundRect(Rect rect)
        {
            if (rect.height < 2f)
            {
                return;
            }

            GUI.Box(rect, "", bgStyle);
            Color bgColor = EditorGUIUtility.isProSkin
                                ? new Color(1, 1, 1, 0.015f)
                                : new Color(1, 1, 1, 0.1f);
            EditorGUI.DrawRect(rect, bgColor);
        }


        public void DrawEditorTexture(Rect rect, string path, ScaleMode scaleMode = ScaleMode.StretchToFill)
        {
            Texture2D tex = GetTexture(path);
            if (tex != null)
            {
                DrawEditorTexture(rect, tex, scaleMode);
            }
        }


        public void DrawEditorTexture(Rect rect, Texture2D texture, ScaleMode scaleMode = ScaleMode.StretchToFill)
        {
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, scaleMode);
            }
        }


        public void EmbeddedObjectEditor<T>(Object obj, Rect rect, float leftMargin = 0) where T : NUIEditor
        {
            if (obj == null)
            {
                return;
            }

            NUIEditor editor = null;
            string key = GenerateKey(obj);
            EditorCache.GetNUIEditorCacheValue(key, ref editor);
            T nuiEditor = editor as T;

            if (nuiEditor == null)
            {
                nuiEditor = Editor.CreateEditor(obj) as T;

                if (nuiEditor == null)
                {
                    Debug.LogError("Failed to create scriptable object editor.");
                    return;
                }

                EditorCache.SetNUIEditorCacheValue(GenerateKey(obj), nuiEditor);
            }

            rect.y += 8;

            if (nuiEditor != null)
            {
                Space();
                nuiEditor.drawer.customPositionRect = positionRect;
                nuiEditor.OnInspectorNUI();

                float editorHeight = GetHeight(key);
                GUILayout.Space(
                    -editorHeight); // Negate space that embedded editor has already added to prevent overly-large final editor
                AdvancePosition(editorHeight);
            }
            else
            {
                Debug.LogError("Cannot draw null editor.");
            }

            UpdateCachedKey();
        }


        public void EndEditor(NUIEditor nuiEditor = null)
        {
            if (totalHeight > 32f)
            {
                totalHeight += 5; // Add padding if expanded
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (serializedObject.targetObject != null)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            SetHeight(totalHeight);
            GUILayout.Space(totalHeight);
        }


        public virtual void EndProperty()
        {
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (totalHeight > 30f)
            {
                Space(6); // Add padding if expanded
            }

            bool wasEnabled = true;
            EditorCache.GetGuiWasEnabledCValue(GenerateKey(), ref wasEnabled);
            if (!GUI.enabled && wasEnabled)
            {
                EditorGUI.EndDisabledGroup();
            }

            SetHeight(totalHeight);
            GUI.enabled = true;
        }


        public void EndSubsection()
        {
            DecreaseIndent();
            Space();
        }

        public SerializedProperty FloatSlider(string propertyRelativeName, float min = 0, float max = 1, string leftLabel = "",
            string rightLabel = "", bool showNumValue = true)
        {
            Debug.Assert(max > min);

            SerializedProperty property = FindProperty(propertyRelativeName);

            if (property == null)
            {
                Debug.LogWarning($"Could not find property '{propertyRelativeName}'");
                return null;
            }

            EditorGUI.LabelField(positionRect,
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(AddSpacesToSentence(propertyRelativeName, true)));

            Rect sliderRect = new Rect(positionRect);
            float leftOffset = sliderRect.width * 0.4f;
            sliderRect.x += leftOffset;
            sliderRect.width -= leftOffset;

            if (showNumValue)
            {
                property.floatValue = EditorGUI.Slider(sliderRect, property.floatValue, min, max);
            }
            else
            {
                property.floatValue = GUI.HorizontalSlider(sliderRect, property.floatValue, min, max);
            }

            float midValue = (min + max) / 2f;
            float range = (max - min);
            float snapZone = range / 90f;
            float propertyValue = property.floatValue;
            if (propertyValue > midValue - snapZone && propertyValue < midValue + snapZone)
            {
                property.floatValue = midValue;
            }


            Rect legendRect = new Rect(sliderRect);
            legendRect.y += 4;

            if (showNumValue)
            {
                legendRect.width -= 53f;
            }
            EditorGUI.LabelField(legendRect, $"{leftLabel} <----> {rightLabel}", EditorStyles.centeredGreyMiniLabel);

            AdvancePosition(NUISettings.fieldHeight + NUISettings.fieldSpacing);

            return property;
        }

        public SerializedProperty Field(string propertyRelativeName, bool enabled = true, string unit = null,
            string alternateLabel = null, float fieldHeight = -1, bool includeChildren = false)
        {
            SerializedProperty property = FindProperty(propertyRelativeName);

            if (property == null)
            {
                Debug.LogWarning($"Could not find relative property '{propertyRelativeName}' on " +
                    $"'{serializedObject?.targetObject?.name}' > '{serializedProperty?.name}'");
                return null;
            }

            property.isExpanded = true;
            bool guiWasEnabled = GUI.enabled;
            EditorGUI.BeginDisabledGroup(!enabled);

            if (fieldHeight < 0)
            {
                fieldHeight = (int)EditorGUI.GetPropertyHeight(property, includeChildren);
            }

            Rect fieldRect = positionRect;
            fieldRect.height = fieldHeight;

            // Draw field
            if (string.IsNullOrEmpty(alternateLabel))
            {
                EditorGUI.PropertyField(fieldRect, property, includeChildren);
            }
            else
            {
                EditorGUI.PropertyField(fieldRect, property, new GUIContent(alternateLabel), includeChildren);
            }

            // Draw unit
            if (unit != null)
            {
                float unitRectWidth = 8f + unit.Length * 8f;
                Rect unitRect = new Rect(fieldRect.x + fieldRect.width - unitRectWidth - 5, fieldRect.y,
                                         unitRectWidth, fieldRect.height);
                EditorGUI.LabelField(unitRect, new GUIContent(unit), unitStyle);
            }

            if (guiWasEnabled)
            {
                EditorGUI.EndDisabledGroup();
            }

            AdvancePosition(fieldHeight + NUISettings.fieldSpacing);
            return property;
        }


        public void DrawDefaultEditor()
        {
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name == "m_Script")
                    {
                        continue;
                    }

                    Field(prop.name);
                    AdvancePosition(EditorGUI.GetPropertyHeight(prop, true));
                } while (prop.NextVisible(false));
            }
        }


        public SerializedProperty FindProperty(string name)
        {
            return _drawerType == DrawerType.Property
                       ? serializedProperty.FindPropertyRelative(name)
                       : serializedObject.FindProperty(name);
        }


        public string GenerateKey()
        {
            return _drawerType == DrawerType.Property ? GenerateKey(serializedProperty) : GenerateKey(serializedObject);
        }


        public float GetHeight()
        {
            return GetHeight(_cachedKey);
        }


        public float GetHeight(string key)
        {
            float height = 0;
            EditorCache.GetHeightCacheValue(key, ref height);
            return height <= 0 ? 1 : height;
        }


        public T GetObject<T>() where T : class
        {
            if (_drawerType == DrawerType.Property)
            {
                return SerializedPropertyHelper.GetTargetObjectOfProperty(serializedProperty) as T;
            }

            return serializedObject.targetObject as T;
        }


        public int GetTabIndex(string tabName)
        {
            return EditorPrefs.GetInt("NWH" + tabName, 0);
        }


        public Texture2D GetTexture(string resourcesPath)
        {
            Texture2D tex = null;
            EditorCache.GetTexture2DCacheValue(resourcesPath, ref tex);
            if (tex == null)
            {
                tex = Resources.Load(resourcesPath) as Texture2D;
                if (tex == null)
                {
                    Debug.LogError($"{resourcesPath} not found or not Texture2D.");
                }
                else
                {
                    EditorCache.SetTexture2DCacheValue(resourcesPath, tex);
                }
            }

            return tex;
        }


        public bool Header(string label, bool drawFoldoutButton = true)
        {
            label = AddSpacesToSentence(label, true);

            Rect backgroundRect = new Rect(positionRect.x, positionRect.y, positionRect.width, 22);

            GUIContent content = new GUIContent(label);
            EditorStyles.label.CalcMinMaxWidth(content, out float minLabelWidth, out float maxLabelWidth);

#if UNITY_2019_3_OR_NEWER
            Rect labelRect = new Rect(positionRect.x + NUISettings.textMargin + 15f, positionRect.y - 1, maxLabelWidth,
                                      NUISettings.fieldHeight);
#else
            Rect labelRect = new Rect(positionRect.x + NUISettings.textMargin + 15f, positionRect.y + 3, maxLabelWidth,
                NUISettings.fieldHeight);
#endif

            // If object, check if scriptable or normal
            bool isScriptableObject = false;
            if (_drawerType == DrawerType.Editor)
            {
                isScriptableObject = serializedObject.targetObject is ScriptableObject;
            }

            // Draw background
            Color bgColor = _drawerType == DrawerType.Property ? NUISettings.propertyHeaderColor :
                            isScriptableObject ? NUISettings.scriptableObjectHeaderColor :
                                                                 NUISettings.editorHeaderColor;
            EditorGUI.DrawRect(backgroundRect, bgColor);


            // Draw label
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.white;
            EditorGUI.LabelField(labelRect, new GUIContent(label), style);

            // Draw help link
            Rect helpRect = new Rect(labelRect.x + labelRect.width + 5f, positionRect.y + 3, 16, 16);
            DrawEditorTexture(helpRect, "NUI/help");
            if (Event.current.type == EventType.MouseUp && helpRect.Contains(Event.current.mousePosition))
            {
                string fullTypeName = _drawerType == DrawerType.Property
                                          ? SerializedPropertyHelper.GetTargetObjectOfProperty(serializedProperty)
                                                                    .GetType().FullName
                                          : serializedObject.targetObject.GetType().FullName;
                string objectPath = fullTypeName.Replace('.', '/');
                Application.OpenURL($"{documentationBaseURL}/{objectPath}");
            }

            AdvancePosition();

            // Apply padding
            positionRect = new Rect(
                positionRect.x + sidePadding,
                positionRect.y,
                positionRect.width - sidePadding * 2f,
                positionRect.height);

            if (_drawerType == DrawerType.Editor)
            {
                if (serializedObject.targetObject is ScriptableObject)
                {
                    Info("This is a ScriptableObject. All changes are global.");
                }
            }

            SetHeight(totalHeight);
            return true;
        }


        public void HorizontalRuler(float thickness = 1f)
        {
            EditorGUI.DrawRect(new Rect(positionRect.x, positionRect.y + 2, positionRect.width, thickness),
                               EditorGUIUtility.isProSkin
                                   ? new Color(0.1f, 0.1f, 0.1f, 1f)
                                   : new Color(0.5f, 0.5f, 0.5f, 1f));
            AdvancePosition(thickness + 4);
        }


        public int HorizontalToolbar(string name, string[] texts, bool fillWidth = true, bool singleLine = false,
            float targetButtonWidth = 80f, float buttonHeight = 18f)
        {
            float toolbarHeight = 0;
            int tabIndex = GetTabIndex(name);

            if (positionRect.width > 20f)
            {
                Rect initRect = positionRect;
                initRect.x += 2f;
                float rowHeight = buttonHeight;
                int buttonCount = texts.Length;
                float rowWidth = positionRect.width - 2f;
                float singleLineWidth = buttonCount * targetButtonWidth;
                int rowCount = singleLine ? 1 : rowWidth > 10 ? Mathf.CeilToInt(singleLineWidth / rowWidth) : 1;
                int maxButtonsPerRow = Mathf.FloorToInt(rowWidth / targetButtonWidth);
                int lastRowButtons = buttonCount - maxButtonsPerRow * (rowCount - 1);
                toolbarHeight = rowCount * rowHeight;

                Rect bottomLine = new Rect(initRect.x - 2f, initRect.y + toolbarHeight, initRect.width, 1f);
                EditorGUI.DrawRect(bottomLine, new Color(0.4f, 0.4f, 0.5f, 1f));

                string[][] subTexts = new string[rowCount][];
                int offset = 0;
                for (int i = 0; i < rowCount; i++)
                {
                    if (i == rowCount - 1)
                    {
                        subTexts[i] = new string[lastRowButtons];
                        for (int j = 0; j < lastRowButtons; j++)
                        {
                            subTexts[i][j] = texts[offset++];
                        }
                    }
                    else
                    {
                        subTexts[i] = new string[maxButtonsPerRow];
                        for (int j = 0; j < maxButtonsPerRow; j++)
                        {
                            subTexts[i][j] = texts[offset++];
                        }
                    }
                }


                int counter = 0;
                for (int x = 0; x < rowCount; x++)
                {
                    float buttonWidth = fillWidth
                                            ? x == rowCount - 1 ? rowWidth / lastRowButtons :
                                                                  rowWidth / maxButtonsPerRow
                                            : targetButtonWidth;
                    for (int y = 0; y < subTexts[x].Length; y++)
                    {
                        GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButtonMid);
                        buttonStyle.fixedHeight = buttonHeight;

                        Color initColor = GUI.color;

                        Rect buttonRect = new Rect(initRect.x + y * buttonWidth, initRect.y, buttonWidth,
                                                   rowHeight + 1);

                        if (tabIndex == counter)
                        {
                            GUI.color = new Color(1f, 1f, 1f, 1f);
                        }
                        else
                        {
                            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                        }

                        if (GUI.Button(buttonRect, subTexts[x][y], buttonStyle))
                        {
                            tabIndex = counter;
                        }

                        if (tabIndex == counter)
                        {
                            Rect highlightRect = new Rect(buttonRect.x - 2f, buttonRect.y + buttonRect.height - 2,
                                                          buttonRect.width,
                                                          2);
                            EditorGUI.DrawRect(highlightRect, NUISettings.lightBlueColor);
                        }

                        GUI.color = initColor;
                        counter++;
                    }

                    initRect.y += rowHeight;
                }

                SetTabIndex(name, tabIndex);
                EditorCache.SetHeightCacheValue(GenerateKey() + name, toolbarHeight);
            }
            else
            {
                EditorCache.GetHeightCacheValue(GenerateKey() + name, ref toolbarHeight);
            }

            AdvancePosition(toolbarHeight + 2);
            return tabIndex;
        }


        public void IncreaseIndent()
        {
            positionRect = new Rect(positionRect.x + 6, positionRect.y, positionRect.width - 6, positionRect.height);
        }


        public void Indent(int indent, float step = 10f)
        {
            positionRect = new Rect(positionRect.x + indent * step, positionRect.y, positionRect.width - indent * step,
                                    positionRect.height);
        }


        public void Info(string text, MessageType messageType = MessageType.Info)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;

            float height = 0;
            if (positionRect.width > 20f)
            {
                float width = positionRect.width;
                GUIContent content = new GUIContent(text);
                height = style.CalcHeight(content, positionRect.width);
                style.fixedHeight = height;
                Rect infoRect = new Rect(positionRect.x, positionRect.y, positionRect.width, height);
                EditorGUI.HelpBox(infoRect, text, messageType);

                EditorCache.SetHeightCacheValue(GenerateKey() + text.GetHashCode(), height); // not the most optimal
            }
            else
            {
                EditorCache.GetHeightCacheValue(GenerateKey() + text.GetHashCode(), ref height);
            }

            AdvancePosition(height + NUISettings.fieldSpacing);
        }


        public void Label(string label, bool bold = false, bool active = true)
        {
            bool wasActive = GUI.enabled;
            EditorGUI.BeginDisabledGroup(!active);
            GUIStyle style = bold ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUI.LabelField(new Rect(positionRect.x, positionRect.y, positionRect.width, NUISettings.fieldHeight),
                                 new GUIContent(label), style);
            if (wasActive)
            {
                EditorGUI.EndDisabledGroup();
            }

            AdvancePosition();
        }


        public void Property(string propertyName, bool drawChildren = true, bool expanded = true)
        {
            Property(FindProperty(propertyName), drawChildren, true);
        }


        public void Property(SerializedProperty p, bool includeChildren = true, bool expanded = true,
            bool disabled = false)
        {
            if (p == null)
            {
                Debug.LogWarning("Property could not be found.");
                return;
            }

            bool wasEnabled = GUI.enabled;
            EditorGUI.BeginDisabledGroup(disabled);

            p.isExpanded = true;
            EditorGUI.PropertyField(positionRect, p, includeChildren);

            AdvancePosition(EditorGUI.GetPropertyHeight(p) + NUISettings.fieldSpacing);

            if (wasEnabled)
            {
                EditorGUI.EndDisabledGroup();
            }

            UpdateCachedKey();
        }


        public void ReorderableList(string propertyName, string label = null, bool draggable = true,
            bool showAddRemoveButtons = true,
            Type baseType = null, float elementSpacing = 0)
        {
            SerializedProperty listProperty = FindProperty(propertyName);
            if (listProperty == null)
            {
                Debug.LogError("Property or SerializedObject is null.");
                return;
            }

            ReorderableList reorderableList = null;
            EditorCache.GetReorderableListCacheValue(GenerateKey(listProperty), ref reorderableList);

            // A fix for Unity 2021.1 bug that causes issues with serializedProperty serializedObject becoming disposed 
            // and crashing the editor if assigned. Null check does not work in this case.
            try
            {
                bool t = reorderableList.serializedProperty.name == null;
            }
            catch
            {
                reorderableList = new ReorderableList(listProperty.serializedObject, listProperty, draggable,
                                                      true, true, true);
            }

            if (reorderableList == null) return;

            reorderableList.serializedProperty = listProperty;
            reorderableList.displayAdd = showAddRemoveButtons;
            reorderableList.displayRemove = showAddRemoveButtons;

            if (reorderableList.serializedProperty == null) return;

            string headerLabel = string.IsNullOrEmpty(label) ? listProperty.displayName : label;
            reorderableList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, new GUIContent(headerLabel)); };

            reorderableList.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index < 0 || index > reorderableList.serializedProperty.arraySize - 1) return;
                SerializedProperty p = listProperty.GetArrayElementAtIndex(index);
                if (p == null) return;
                EditorGUI.PropertyField(rect, p, true);
            };

            reorderableList.elementHeightCallback = index =>
            {
                if (index < 0 || index > reorderableList.serializedProperty.arraySize - 1) return 0;
                SerializedProperty p =
                    listProperty.GetArrayElementAtIndex(index);
                if (p == null) return 0;
                float height = EditorGUI.GetPropertyHeight(p, true);
                return height + elementSpacing + 1;
            };

            float listHeight = reorderableList.GetHeight() + 1;
            Rect listRect = new Rect(positionRect.x, positionRect.y + 3f, positionRect.width, listHeight);
            reorderableList.DoList(listRect);

            AdvancePosition(listHeight + NUISettings.fieldSpacing);

            EditorCache.SetReorderableListCacheValue(GenerateKey(listProperty), reorderableList);
        }


        public void SetHeight(float height)
        {
            SetHeight(_cachedKey, height);
        }


        public void SetHeight(string key, float height)
        {
            EditorCache.SetHeightCacheValue(key, height);
        }


        public void Space(float spaceSize = 5f)
        {
            AdvancePosition(spaceSize);
        }


        public void SplitRectVertically(Rect inRect, float splitPoint, out Rect rectA, out Rect rectB,
            float centerMargin = 4f)
        {
            float rectAWidth = inRect.width * splitPoint;
            rectA = new Rect(inRect.x, inRect.y, rectAWidth, inRect.height);
            rectB = new Rect(inRect.x + rectAWidth + centerMargin, inRect.y, inRect.width - rectAWidth, inRect.height);
        }


        public void Title(string label)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            EditorGUI.DrawRect(new Rect(new Vector2(positionRect.position.x, positionRect.position.y + 2),
                                        new Vector2(positionRect.size.x, positionRect.size.y - 4f)),
                               EditorGUIUtility.isProSkin
                                   ? new Color(0.21f, 0.21f, 0.21f)
                                   : NUISettings.lightGreyColor);

#if UNITY_2019_3_OR_NEWER
            EditorGUI.LabelField(positionRect, new GUIContent(" " + label), style);
#else
            EditorGUI.LabelField(new Rect(positionRect.x, positionRect.y + 3f, positionRect.width, positionRect.height),
                new GUIContent(" " + label), style);
#endif

            AdvancePosition();
        }


        private string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) ||
                        preserveAcronyms && char.IsUpper(text[i - 1]) &&
                        i < text.Length - 1 && !char.IsUpper(text[i + 1]))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }
    }
}

#endif
