using NWH.Common.Vehicles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using NWH.NUI;
#endif

namespace NWH.WheelController3D
{
    /// <summary>
    /// Imitates tyre wear by reducing the grip available using the slip and load 
    /// values of the wheel.
    /// </summary>
    [DisallowMultipleComponent]
    public class TyreWear : MonoBehaviour
    {
        /// <summary>
        /// Wear rate coefficient. Unitless.
        /// </summary>
        [UnityEngine.Tooltip("Wear rate coefficient. Unitless.")]
        public float wearRate = 0.01f;

        /// <summary>
        /// Grip coefficient at the 100% wear level.
        /// </summary>
        [UnityEngine.Tooltip("Grip coefficient at the 100% wear level.")]
        public float maxGripReduction = 0.4f;

        /// <summary>
        /// The effect of load on the tire wear.
        /// </summary>
        [UnityEngine.Tooltip("The effect of load on the tire wear.")]
        public float loadWearContribution = 1f;

        /// <summary>
        /// Wear coefficient for lateral slip.
        /// </summary>
        [UnityEngine.Tooltip("Wear coefficient for lateral slip.")]
        public float lateralSlipWearContribution = 1f;

        /// <summary>
        /// Wear coefficient for longitudinal slip.
        /// </summary>
        [UnityEngine.Tooltip("Wear coefficient for longitudinal slip.")]
        public float longitudinalSlipWearContribution = 1f;

        /// <summary>
        /// Coroutine update frequency in seconds.
        /// </summary>
        [Range(0.01f, 0.5f)]
        [UnityEngine.Tooltip("Coroutine update frequency in seconds.")]
        public float updateRate = 0.1f;

        /// <summary>
        /// Current tire wear. 0 = no wear, 1 = fully worn.
        /// </summary>
        [Range(0f, 1f)]
        [UnityEngine.Tooltip("Current tire wear. 0 = no wear, 1 = fully worn.")]
        public float wear;


        private WheelUAPI _wc;
        private float _initLatGrip;
        private float _initLngGrip;


        private void Awake()
        {
            _wc = GetComponent<WheelUAPI>();
            Debug.Assert(_wc != null, "WheelController not found.");

            _initLatGrip = _wc.LateralFrictionGrip;
            _initLngGrip = _wc.LongitudinalFrictionGrip;
        }


        private void OnEnable()
        {
            StartCoroutine(TyreWearCoroutine());
        }


        private void OnDisable()
        {
            StopCoroutine(TyreWearCoroutine());
        }


        private IEnumerator TyreWearCoroutine()
        {
            while (true)
            {
                if (_wc.ParentRigidbody.velocity.sqrMagnitude > 0.5f)
                {
                    float loadFactor = Mathf.Clamp01(_wc.Load / _wc.MaxLoad) * loadWearContribution;
                    float lngWear = Mathf.Abs(_wc.LongitudinalSlip) * longitudinalSlipWearContribution;
                    float latWear = Mathf.Abs(_wc.LateralSlip) * lateralSlipWearContribution;
                    wear += (lngWear + latWear) * loadFactor * updateRate * wearRate;
                    wear = Mathf.Clamp01(wear);
                }

                _wc.LateralFrictionGrip = _initLatGrip - (wear * maxGripReduction);
                _wc.LongitudinalFrictionGrip = _initLngGrip - (wear * maxGripReduction);

                yield return new WaitForSeconds(updateRate);
            }
        }
    }
}



#if UNITY_EDITOR
namespace NWH.WheelController3D
{
    /// <summary>
    ///     Editor for WheelController.
    /// </summary>
    [CustomEditor(typeof(TyreWear))]
    [CanEditMultipleObjects]
    public class TyeWearEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI()) return false;

            drawer.Field("wear", true, "x100%");
            drawer.Field("wearRate");
            drawer.Field("maxGripReduction");
            drawer.Field("loadWearContribution");
            drawer.Field("lateralSlipWearContribution");
            drawer.Field("longitudinalSlipWearContribution");
            drawer.Field("updateRate");


            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif