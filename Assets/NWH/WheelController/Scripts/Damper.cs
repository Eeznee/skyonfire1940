using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NWH.WheelController3D
{
    /// <summary>
    ///     Suspension damper.
    /// </summary>
    [Serializable]
    public class Damper
    {
        /// <summary>
        ///     Bump rate of the damper in Ns/m.
        /// </summary>
        [FormerlySerializedAs("maxBumpForce")] // Not a direct replacement, but about the same value
        public float bumpRate = 3000f;

        /// <summary>
        ///     Rebound rate of the damper in Ns/m.
        /// </summary>
        [FormerlySerializedAs("maxReboundForce")]
        public float reboundRate = 3000f;

        /// <summary>
        /// Slow bump slope for the damper, used for damper velocity below bumpDivisionVelocity.
        /// Value of 1 means that the bump force increases proportionally to the compression velocity.
        /// </summary>
        [Range(0f, 3f)]
        public float slowBump = 1.4f;

        /// <summary>
        /// Fast bump slope for the damper, used for damper velocity above bumpDivisionVelocity.
        /// Value of 1 means that the bump force increases proportionally to the compression velocity.
        /// </summary>
        [Range(0f, 3f)]
        public float fastBump = 0.6f;

        /// <summary>
        /// Damper velocity at which the bump slope switches from the slowBump to fastBump.
        /// </summary>
        [Range(0f, 0.2f)]
        public float bumpDivisionVelocity = 0.06f;

        /// <summary>
        /// Slow rebound slope for the damper, used for damper velocity below reboundDivisionVelocity.
        /// Value of 1 means that the rebound force increases proportionally to the extension velocity.
        /// </summary>
        [Range(0f, 3f)]
        public float slowRebound = 1.6f;

        /// <summary>
        /// Fast rebound slope for the damper, used for damper velocity above reboundDivisionVelocity.
        /// Value of 1 means that the rebound force increases proportionally to the extension velocity.
        /// </summary>
        [Range(0f, 3f)]
        public float fastRebound = 0.6f;

        /// <summary>
        /// Damper velocity at which the rebound slope switches from the slowRebound to fastRebound.
        /// </summary>
        [Range(0f, 0.2f)]
        public float reboundDivisionVelocity = 0.05f;

        /// <summary>
        ///     Current damper force.
        /// </summary>
        [Tooltip("    Current damper force.")]
        public float force;


        /// <summary>
        /// Calculates damper force based on velocity.
        /// </summary>
        /// <param name="velocity">Compression velocity in m/s.</param>
        /// <returns>Damper force in N.</returns>
        public float CalculateDamperForce(in float velocity)
        {
            if (velocity > 0f)
            {
                return CalculateBumpForce(velocity);
            }
            else
            {
                return CalculateReboundForce(velocity);
            }
        }


        /// <summary>
        /// Calculates damper bump force.
        /// </summary>
        /// <param name="velocity">Spring compression velocity in m/s.</param>
        /// <returns>Damper bump force in N for velocity > 0, otherwise 0 (not bump).</returns>
        private float CalculateBumpForce(in float velocity)
        {
            if (velocity < 0f) return 0f; // We are in rebound, return.

            float x = velocity;
            float y;
            if (x < bumpDivisionVelocity)
            {
                y = x * slowBump;
            }
            else
            {
                y = bumpDivisionVelocity * slowBump + (x - bumpDivisionVelocity) * fastBump;
            }
            return y * bumpRate;
        }


        /// <summary>
        /// Calculates damper rebound force.
        /// </summary>
        /// <param name="velocity">Spring rebound velocity in m/s.</param>
        /// <returns>Damper rebound force in N for velocity < 0, otherwise 0 (not rebound).</returns>
        private float CalculateReboundForce(in float velocity)
        {
            if (velocity > 0f) return 0f; // We are in bump, return.

            float x = -velocity;
            float y;
            if (x < reboundDivisionVelocity)
            {
                y = x * slowRebound;
            }
            else
            {
                y = reboundDivisionVelocity * slowRebound + (x - reboundDivisionVelocity) * fastRebound;
            }
            return -y * reboundRate;
        }
    }
}