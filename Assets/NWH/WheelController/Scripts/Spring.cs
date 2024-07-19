using System;
using UnityEngine;

namespace NWH.WheelController3D
{
    /// <summary>
    ///     Suspension spring.
    /// </summary>
    [Serializable]
    public class Spring
    {
        public enum ExtensionState
        {
            Normal,
            OverExtended,
            BottomedOut
        }

        /// <summary>
        /// The state the spring is in.
        /// </summary>
        [UnityEngine.Tooltip("The state the spring is in.")]
        public ExtensionState extensionState = ExtensionState.Normal;

        /// <summary>
        ///     How much is spring currently compressed. 0 means fully relaxed and 1 fully compressed.
        /// </summary>
        [Tooltip("    How much is spring currently compressed. 0 means fully relaxed and 1 fully compressed.")]
        public float compression;

        /// <summary>
        ///     Current force the spring is exerting in [N].
        /// </summary>
        [Tooltip("    Current force the spring is exerting in [N].")]
        public float force;

        /// <summary>
        ///     Force curve where X axis represents spring travel [0,1] and Y axis represents force coefficient [0, 1].
        ///     Force coefficient is multiplied by maxForce to get the final spring force.
        /// </summary>
        [Tooltip(
            "Force curve where X axis represents spring travel [0,1] and Y axis represents force coefficient [0, 1].\r\nForce coefficient is multiplied by maxForce to get the final spring force.")]
        public AnimationCurve forceCurve;

        /// <summary>
        ///     Current length of the spring.
        /// </summary>
        [Tooltip("    Current length of the spring.")]
        public float length;

        /// <summary>
        ///     Maximum force spring can exert.
        /// </summary>
        [Tooltip("    Maximum force spring can exert.")]
        public float maxForce = 16000.0f;

        /// <summary>
        ///     Length of fully relaxed spring.
        /// </summary>
        [Tooltip("    Length of fully relaxed spring.")]
        public float maxLength = 0.35f;

        /// <summary>
        ///     Length of the spring during the previous physics update.
        /// </summary>
        [UnityEngine.Tooltip("    Length of the spring during the previous physics update.")]
        public float prevLength;

        /// <summary>
        ///     Rate of change of the length of the spring in [m/s].
        /// </summary>
        [Tooltip("    Rate of change of the length of the spring in [m/s].")]
        public float compressionVelocity;

        /// <summary>
        /// Velocity of the spring in the previous frame.
        /// </summary>
        [UnityEngine.Tooltip("Velocity of the spring in the previous frame.")]
        public float prevVelocity;
    }
}