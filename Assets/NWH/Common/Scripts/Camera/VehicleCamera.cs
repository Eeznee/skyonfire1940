using UnityEngine;

#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif

namespace NWH.Common.Cameras
{
    public class VehicleCamera : MonoBehaviour
    {
        /// <summary>
        ///     Transform that this script is targeting. Can be left empty if head movement is not being used.
        /// </summary>
        [Tooltip(
            "Transform that this script is targeting. Can be left empty if head movement is not being used.")]
        public Transform target;


        public virtual void Awake()
        {
            if (target == null)
            {
                target = GetComponentInParent<Rigidbody>()?.transform;
            }
        }
    }
}


#if UNITY_EDITOR

namespace NWH.Common.Cameras
{
    [CustomEditor(typeof(VehicleCamera))]
    [CanEditMultipleObjects]
    public class VehicleCameraEditor : NUIEditor
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
