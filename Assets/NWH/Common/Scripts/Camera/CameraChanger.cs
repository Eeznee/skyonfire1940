using System.Collections.Generic;
using NWH.Common.Input;
using UnityEngine;
using UnityEngine.Serialization;
using NWH.Common.Vehicles;


#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif

namespace NWH.Common.Cameras
{
    /// <summary>
    ///     Switches between the camera objects that are children to this object and contain camera tag,
    ///     in order they appear in the hierarchy or in order they are added to the vehicle cameras list.
    /// </summary>
    [DefaultExecutionOrder(20)]
    public class CameraChanger : MonoBehaviour
    {
        /// <summary>
        ///     If true vehicleCameras list will be filled through cameraTag.
        /// </summary>
        [Tooltip("    If true vehicleCameras list will be filled through cameraTag.")]
        public bool autoFindCameras = true;

        /// <summary>
        ///     Index of the camera from vehicle cameras list that will be active first.
        /// </summary>
        [Tooltip("    Index of the camera from vehicle cameras list that will be active first.")]
        public int currentCameraIndex;

        /// <summary>
        ///     List of cameras that the changer will cycle through. Leave empty if you want cameras to be automatically detected.
        ///     To be detected cameras need to have camera tag and be children of the object this script is attached to.
        /// </summary>
        [FormerlySerializedAs("vehicleCameras")]
        [Tooltip(
            "List of cameras that the changer will cycle through. Leave empty if you want cameras to be automatically detected." +
            " To be detected cameras need to have camera tag and be children of the object this script is attached to.")]
        public List<GameObject> cameras = new List<GameObject>();


        private Vehicle _vehicle;


        /// <summary>
        /// Has to be OnEnable as to run before the VehicleController initialization.
        /// </summary>
        private void Awake()
        {
            _vehicle = GetComponentInParent<Vehicle>();
            if (_vehicle == null)
            {
                Debug.LogError("None of the parent objects of CameraChanger contain VehicleController.");
            }

            _vehicle.onEnable.AddListener(EnableCurrentDisableOthers);
            _vehicle.onDisable.AddListener(DisableAllCameras);
            _vehicle.onMultiplayerStatusChanged.AddListener(OnMultiplayerInstanceTypeChanged);

            if (_vehicle == null)
            {
                Debug.Log("None of the parents of camera changer contain VehicleController component. " +
                          "Make sure that the camera changer is amongst the children of VehicleController object.");
            }

            if (autoFindCameras)
            {
                cameras = new List<GameObject>();
                foreach (Camera cam in GetComponentsInChildren<Camera>(true))
                {
                    cameras.Add(cam.gameObject);
                }
            }

            if (cameras.Count == 0)
            {
                Debug.LogWarning("No cameras could be found by CameraChanger. Either add cameras manually or " +
                                 "add them as children to the game object this script is attached to.");
            }
        }


        private void Update()
        {
            if (_vehicle.enabled && !_vehicle.MultiplayerIsRemote && InputProvider.Instances.Count > 0)
            {
                bool changeCamera = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.ChangeCamera());

                if (changeCamera)
                {
                    NextCamera();
                    CheckIfInside();
                }
            }
        }


        private void OnMultiplayerInstanceTypeChanged(bool isRemote)
        {
            if (isRemote)
            {
                DisableAllCameras();
            }
        }


        private void EnableCurrentDisableOthers()
        {
            if (_vehicle.MultiplayerIsRemote)
            {
                return;
            }

            int cameraCount = cameras.Count;
            for (int i = 0; i < cameraCount; i++)
            {
                if (cameras[i] == null)
                {
                    continue;
                }

                if (i == currentCameraIndex)
                {
                    cameras[i].SetActive(true);
                    AudioListener al = cameras[i].GetComponent<AudioListener>();
                    if (al != null)
                    {
                        al.enabled = true;
                    }
                }
                else
                {
                    cameras[i].SetActive(false);
                    AudioListener al = cameras[i].GetComponent<AudioListener>();
                    if (al != null)
                    {
                        al.enabled = false;
                    }
                }
            }
        }


        private void DisableAllCameras()
        {
            int cameraCount = cameras.Count;
            for (int i = 0; i < cameraCount; i++)
            {
                cameras[i].SetActive(false);
                AudioListener al = cameras[i].GetComponent<AudioListener>();
                if (al != null)
                {
                    al.enabled = true;
                }
            }
        }


        /// <summary>
        ///     Activates next camera in order the camera scripts are attached to the camera object.
        /// </summary>
        public void NextCamera()
        {
            if (cameras.Count <= 0)
            {
                return;
            }

            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Count)
            {
                currentCameraIndex = 0;
            }

            EnableCurrentDisableOthers();
        }


        public void PreviousCamera()
        {
            if (cameras.Count <= 0)
            {
                return;
            }

            currentCameraIndex--;
            if (currentCameraIndex < 0)
            {
                currentCameraIndex = cameras.Count - 1;
            }

            EnableCurrentDisableOthers();
        }


        private void CheckIfInside()
        {
            if (cameras.Count == 0 || cameras[currentCameraIndex] == null)
            {
                return;
            }

            CameraInsideVehicle civ = cameras[currentCameraIndex]?.GetComponent<CameraInsideVehicle>();
            if (civ != null)
            {
                _vehicle.CameraInsideVehicle = civ.isInsideVehicle;
            }
            else
            {
                _vehicle.CameraInsideVehicle = false;
            }
        }
    }
}


#if UNITY_EDITOR

namespace NWH.Common.Cameras
{
    [CustomEditor(typeof(CameraChanger))]
    [CanEditMultipleObjects]
    public class CameraChangerEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Field("currentCameraIndex");
            if (drawer.Field("autoFindCameras").boolValue)
            {
                drawer.Info(
                    "When using autoFindCameras make sure that all the cameras are direct children of the object this script is attached to.");
            }
            else
            {
                drawer.ReorderableList("cameras");
            }

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
