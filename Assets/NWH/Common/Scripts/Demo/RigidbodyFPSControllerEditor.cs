#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;

namespace NWH.Common.Demo.Editor
{
    [CustomEditor(typeof(RigidbodyFPSController))]
    [CanEditMultipleObjects]
    public class RigidbodyFPSControllerEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Field("gravity");
            drawer.Field("maximumY");
            drawer.Field("maxVelocityChange");
            drawer.Field("minimumY");
            drawer.Field("sensitivityX");
            drawer.Field("sensitivityY");
            drawer.Field("speed");

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
