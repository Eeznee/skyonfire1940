using UnityEngine;

namespace NWH.Common.Input
{
    /// <summary>
    ///     InputProvider for scene and camera related behavior.
    /// </summary>
    public abstract class SceneInputProviderBase : InputProvider
    {
        /// <summary>
        ///     If true a button press will be required to unlock camera rotation.
        /// </summary>
        [UnityEngine.Tooltip("    If true a button press will be required to unlock camera rotation.")]
        public bool requireCameraRotationModifier = true;

        /// <summary>
        ///     If true a button press will be required to unlock camera panning.
        /// </summary>
        [UnityEngine.Tooltip("    If true a button press will be required to unlock camera panning.")]
        public bool requireCameraPanningModifier = true;


        // Common camera bindings
        public virtual bool ChangeCamera()
        {
            return false;
        }


        public virtual Vector2 CameraRotation()
        {
            return Vector2.zero;
        }


        public virtual Vector2 CameraPanning()
        {
            return Vector2.zero;
        }


        public virtual bool CameraRotationModifier()
        {
            return !requireCameraRotationModifier;
        }


        public virtual bool CameraPanningModifier()
        {
            return !requireCameraPanningModifier;
        }


        public virtual float CameraZoom()
        {
            return 0;
        }


        // Common scene bindings
        public virtual bool ChangeVehicle()
        {
            return false;
        }


        public virtual Vector2 CharacterMovement()
        {
            return Vector2.zero;
        }


        public virtual bool ToggleGUI()
        {
            return false;
        }
    }
}