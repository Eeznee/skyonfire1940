#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NWH.NUI
{
    public class NUIPropertyDrawer : PropertyDrawer
    {
        protected NUIDrawer drawer = new NUIDrawer();


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return drawer.GetHeight(NUIDrawer.GenerateKey(property));
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnNUI(position, property, label);
        }


        public virtual string GetDocumentationBaseURL()
        {
            return "http://nwhvehiclephysics.com/doku.php/";
        }


        public virtual bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (drawer == null)
            {
                drawer = new NUIDrawer();
            }

            drawer.documentationBaseURL = GetDocumentationBaseURL();
            drawer.BeginProperty(position, property, label);

            string name = property.FindPropertyRelative("name")?.stringValue;
            if (string.IsNullOrEmpty(name))
            {
                name = property.displayName;
            }

            if (!drawer.Header(name))
            {
                drawer.EndProperty();
                return false;
            }

            return true;
        }
    }
}

#endif
