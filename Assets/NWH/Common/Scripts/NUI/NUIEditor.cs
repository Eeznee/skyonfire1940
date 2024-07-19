#if UNITY_EDITOR
using UnityEditor;

namespace NWH.NUI
{
    [CanEditMultipleObjects]
    public class NUIEditor : Editor
    {
        public NUIDrawer drawer = new NUIDrawer();


        public override void OnInspectorGUI()
        {
            OnInspectorNUI();
        }


        public virtual bool OnInspectorNUI()
        {
            if (drawer == null)
            {
                drawer = new NUIDrawer();
            }

            drawer.documentationBaseURL = GetDocumentationBaseURL();

            drawer.BeginEditor(serializedObject);
            if (!drawer.Header(serializedObject.targetObject.GetType().Name))
            {
                drawer.EndEditor();
                return false;
            }

            return true;
        }


        public virtual string GetDocumentationBaseURL()
        {
            return "http://nwhvehiclephysics.com/doku.php/";
        }


        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}

#endif
