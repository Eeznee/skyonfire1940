using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NWH.WheelController3D
{
    /// <summary>
    ///     All info related to longitudinal force calculation.
    /// </summary>
    [Serializable]
    public class Friction
    {
        /// <summary>
        ///     Current force in friction direction.
        /// </summary>
        [Tooltip("    Current force in friction direction.")]
        public float force;

        /// <summary>
        ///     Current slip in friction direction.
        /// </summary>
        [Tooltip("    Current slip in friction direction.")]
        public float slip;


        /// <summary>
        ///     Multiplies the Y value (grip) of the friction graph.
        ///     Formerly known as 'forceCoefficient'.
        /// </summary>
        [Range(0.0f, 2f)]
        [FormerlySerializedAs("forceCoefficient")]
        [UnityEngine.Tooltip("    Multiplies the Y value (grip) of the friction graph.\r\n    Formerly known as 'forceCoefficient'.")]
        public float grip = 1f;


        /// <summary>
        ///     Mutliplies the X value (slip) of the friction graph.
        ///     Formerly known as 'slipCoefficient'.
        /// </summary>
        [Range(0.0f, 2f)]
        [FormerlySerializedAs("slipCoefficient")]
        [UnityEngine.Tooltip("    Mutliplies the X value (slip) of the friction graph.\r\n    Formerly known as 'slipCoefficient'.")]
        public float stiffness = 1f;


        /// <summary>
        ///     Speed at the point of contact with the surface.
        /// </summary>
        [Tooltip("    Speed at the point of contact with the surface.")]
        public float speed;
    }
}