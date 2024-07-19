#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;

namespace NWH.Common.Demo
{
    [CustomEditor(typeof(DragObject))]
    [CanEditMultipleObjects]
    public class DragObjectEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.EndEditor(this);
            return true;
        }


        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}

#endif