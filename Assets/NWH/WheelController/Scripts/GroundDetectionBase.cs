using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.WheelController3D
{
    /// <summary>
    /// Base class for wheel ground detection. 
    /// By default StandardGroundDetection is used, but in case it is needed a custom GroundDetection script
    /// can be created and attached to the WheelController. It only needs to implement the WheelCast() function
    /// and return the point nearest to the wheel center.
    /// </summary>
    [RequireComponent(typeof(WheelController))]
    public abstract class GroundDetectionBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin">Origin of the wheel cast.</param>
        /// <param name="direction">Direction of the wheel cast.</param>
        /// <param name="distance">Distance the cast will travel.</param>
        /// <param name="radius">Radius of the wheel.</param>
        /// <param name="width">Width of the wheel.</param>
        public abstract bool WheelCast(in Vector3 origin, in Vector3 direction, in float distance, in float radius, in float width, ref WheelHit result, LayerMask layerMask);
    }
}

