using UnityEngine;

namespace NWH.Common.CoM
{
    /// <summary>
    /// Represents object that has mass and is a child of VariableCoM.
    /// Affects rigidbody center of mass and inertia.
    /// </summary>
    public interface IMassAffector
    {
        /// <summary>
        /// Returns mass of the mass affector in kilograms.
        /// </summary>
        float GetMass();

        /// <summary>
        /// Returns the center of mass of the affector in world coordinates.
        /// </summary>
        Vector3 GetWorldCenterOfMass();

        /// <summary>
        /// Returns transform of the mass affector.
        /// </summary>
        Transform GetTransform();
    }
}