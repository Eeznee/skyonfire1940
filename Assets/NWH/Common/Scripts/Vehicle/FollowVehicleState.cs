using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using NWH.NUI;
#endif

namespace NWH.Common.Vehicles
{
    /// <summary>
    ///     Enables the object when vehicle is awake, disables it when vehicle is sleeping.
    /// </summary>
    [DefaultExecutionOrder(21)]
    public partial class FollowVehicleState : MonoBehaviour
    {
        private Vehicle _vc;


        private void OnEnable()
        {
            _vc = GetComponentInParent<Vehicle>();
            if (_vc == null)
            {
                Debug.LogError("VehicleController not found.");
            }

            _vc.onEnable.AddListener(OnVehicleWake);
            _vc.onDisable.AddListener(OnVehicleSleep);

            if (_vc.enabled)
            {
                OnVehicleWake();
            }
            else
            {
                OnVehicleSleep();
            }
        }


        private void OnVehicleWake()
        {
            gameObject.SetActive(true);
        }


        private void OnVehicleSleep()
        {
            gameObject.SetActive(false);
        }
    }
}

#if UNITY_EDITOR
namespace NWH.Common.Vehicles
{
    [CustomEditor(typeof(FollowVehicleState))]
    [CanEditMultipleObjects]
    public partial class FollowVehicleStateEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Info("Enables/disables the GameObject based on Vehicle state (awake/asleep).");

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
