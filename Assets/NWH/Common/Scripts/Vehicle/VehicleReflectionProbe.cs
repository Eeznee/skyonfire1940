using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using NWH.NUI;
#endif

namespace NWH.Common.Vehicles
{
    /// <summary>
    ///     Manages reflection probe for the vehicle and switches between probe types as needed.
    /// </summary>
    [RequireComponent(typeof(ReflectionProbe))]
    [DefaultExecutionOrder(19)]
    public partial class VehicleReflectionProbe : MonoBehaviour
    {
        public enum ProbeType
        {
            Baked,
            Realtime,
        }

        public ProbeType awakeProbeType = ProbeType.Realtime;
        public ProbeType asleepProbeType = ProbeType.Baked;
        public bool bakeOnStart = true;
        public bool bakeOnSleep = true;

        private ReflectionProbe _reflectionProbe;
        private Vehicle _vc;


        private void OnEnable()
        {
            _vc = GetComponentInParent<Vehicle>();
            if (_vc == null)
            {
                Debug.LogError("VehicleController not found.");
            }

            _reflectionProbe = GetComponent<ReflectionProbe>();
            _vc.onEnable.AddListener(OnVehicleEnable);
            _vc.onDisable.AddListener(OnVehicleDisable);

            _reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;

            if (bakeOnStart)
            {
                _reflectionProbe.RenderProbe();
            }
        }


        private void OnVehicleEnable()
        {
            _reflectionProbe.mode = awakeProbeType == ProbeType.Baked
                                        ? _reflectionProbe.mode = ReflectionProbeMode.Baked
                                        : ReflectionProbeMode.Realtime;
        }


        private void OnVehicleDisable()
        {
            _reflectionProbe.mode = asleepProbeType == ProbeType.Baked
                                        ? _reflectionProbe.mode = ReflectionProbeMode.Baked
                                        : ReflectionProbeMode.Realtime;

            if (bakeOnSleep && _reflectionProbe.isActiveAndEnabled)
            {
                _reflectionProbe.RenderProbe();
            }
        }
    }
}


#if UNITY_EDITOR
namespace NWH.Common.Vehicles
{
    [CustomEditor(typeof(VehicleReflectionProbe))]
    [CanEditMultipleObjects]
    public partial class VehicleReflectionProbeEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Field("awakeProbeType");
            drawer.Field("asleepProbeType");
            drawer.Field("bakeOnStart");
            drawer.Field("bakeOnSleep");

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
