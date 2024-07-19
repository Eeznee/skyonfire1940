using UnityEngine;

#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif


namespace NWH.Common.Input
{
    public class InputSystemSceneInputProvider : SceneInputProviderBase
    {
        public SceneInputActions sceneInputActions;

        private bool _rotationModifier;
        private bool _panningModifier;


        public override void Awake()
        {
            base.Awake();

            sceneInputActions = new SceneInputActions();
            sceneInputActions.Enable();

            sceneInputActions.CameraControls.CameraRotationModifier.started += ctx => _rotationModifier = true;
            sceneInputActions.CameraControls.CameraRotationModifier.canceled += ctx => _rotationModifier = false;

            sceneInputActions.CameraControls.CameraPanningModifier.started += ctx => _panningModifier = true;
            sceneInputActions.CameraControls.CameraPanningModifier.canceled += ctx => _panningModifier = false;
        }


        public override bool ChangeCamera()
        {
            return sceneInputActions.CameraControls.ChangeCamera.triggered;
        }


        public override Vector2 CameraRotation()
        {
            return sceneInputActions.CameraControls.CameraRotation.ReadValue<Vector2>();
        }


        public override Vector2 CameraPanning()
        {
            return sceneInputActions.CameraControls.CameraPanning.ReadValue<Vector2>();
        }


        public override bool CameraRotationModifier()
        {
            return _rotationModifier || !requireCameraRotationModifier;
        }


        public override bool CameraPanningModifier()
        {
            return _panningModifier || !requireCameraPanningModifier;
        }


        public override float CameraZoom()
        {
            return sceneInputActions.CameraControls.CameraZoom.ReadValue<float>() * 0.1f;
        }


        public override bool ChangeVehicle()
        {
            return sceneInputActions.SceneControls.ChangeVehicle.triggered;
        }


        public override Vector2 CharacterMovement()
        {
            return sceneInputActions.SceneControls.FPSMovement.ReadValue<Vector2>();
        }


        public override bool ToggleGUI()
        {
            return sceneInputActions.SceneControls.ToggleGUI.triggered;
        }
    }
}


#if UNITY_EDITOR

namespace NWH.Common.Input
{
    [CustomEditor(typeof(InputSystemSceneInputProvider))]
    public class InputSystemSceneInputProviderEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Info("Input settings for Unity's new input system can be changed by modifying 'SceneInputActions' " +
                        "file (double click on it to open).");

            drawer.Field("requireCameraRotationModifier");
            drawer.Field("requireCameraPanningModifier");

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
