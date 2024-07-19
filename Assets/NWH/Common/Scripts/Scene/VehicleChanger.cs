using System.Collections.Generic;
using NWH.Common.Input;
using UnityEngine;
using NWH.Common.Vehicles;
using UnityEngine.Events;

#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif


namespace NWH.Common.SceneManagement
{
    [DefaultExecutionOrder(500)]
    public class VehicleChanger : MonoBehaviour
    {
        public enum CharacterLocation
        {
            OutOfRange,
            Near,
            Inside
        }

        /// <summary>
        ///     Index of the current vehicle in vehicles list.
        /// </summary>
        [Tooltip("    Index of the current vehicle in vehicles list.")]
        public int activeVehicleIndex = 0;

        /// <summary>
        ///     Should the vehicles that the player is currently not using be put to sleep to improve performance?
        /// </summary>
        [Tooltip(
            "    Should the vehicles that the player is currently not using be put to sleep to improve performance?")]
        public bool putOtherVehiclesToSleep = true;

        /// <summary>
        ///     List of all of the vehicles that can be selected and driven in the scene.
        /// </summary>
        [Tooltip("List of all of the vehicles that can be selected and driven in the scene. " +
                 "If set to 0 script will try to auto-find all the vehicles in the scene with a tag define by VehiclesTag parameter.")]
        public List<Vehicle> vehicles = new List<Vehicle>();

        /// <summary>
        ///     Is vehicle changing character based? When true changing vehicles will require getting close to them
        ///     to be able to enter, opposed to pressing a button to switch between vehicles.
        /// </summary>
        [Tooltip(
            "Is vehicle changing character based? When true changing vehicles will require getting close to them\r\nto be able to enter, opposed to pressing a button to switch between vehicles.")]
        public bool characterBased;

        /// <summary>
        ///     Game object representing a character. Can also be another vehicle.
        /// </summary>
        [Tooltip("    Game object representing a character. Can also be another vehicle.")]
        public GameObject characterObject;

        /// <summary>
        ///     Maximum distance at which the character will be able to enter the vehicle.
        /// </summary>
        [Range(0.2f, 3f)]
        [Tooltip("    Maximum distance at which the character will be able to enter the vehicle.")]
        public float enterDistance = 2f;

        /// <summary>
        ///     Tag of the object representing the point from which the enter distance will be measured. Useful if you want to
        ///     enable you character to enter only when near the door.
        /// </summary>
        [Tooltip(
            "Tag of the object representing the point from which the enter distance will be measured. Useful if you want to enable you character to enter only when near the door.")]
        public string enterExitTag = "EnterExitPoint";

        /// <summary>
        ///     Maximum speed at which the character will be able to enter / exit the vehicle.
        /// </summary>
        [Tooltip("    Maximum speed at which the character will be able to enter / exit the vehicle.")]
        public float maxEnterExitVehicleSpeed = 2f;

        /// <summary>
        /// When the location is Near, the player can enter the vehicle.
        /// </summary>
        [UnityEngine.Tooltip("When the location is Near, the player can enter the vehicle.")]
        public CharacterLocation location = CharacterLocation.OutOfRange;

        /// <summary>
        /// Should the player start inside the vehicle?
        /// </summary>
        [UnityEngine.Tooltip("Should the player start inside the vehicle?")]
        public bool startInVehicle = false;

        public UnityEvent onVehicleChanged = new UnityEvent();

        public UnityEvent onDeactivateAll = new UnityEvent();

        /// <summary>
        /// The vehicle the player is nearest to, or in case the player is inside the vehicle, the vehicle the player is inside of.
        /// </summary>
        private Vehicle _nearestVehicle;
        private Vector3 _relativeEnterPosition;
        private GameObject[] _enterExitPoints;
        private GameObject _nearestEnterExitPoint;

        public static VehicleChanger Instance { get; private set; }

        private static Vehicle ActiveVehicle
        {
            get
            {
                if (Instance == null) return null;
                return Instance.activeVehicleIndex < 0 || Instance.activeVehicleIndex >= Instance.vehicles.Count ?
                    null :
                    Instance.vehicles[Instance.activeVehicleIndex];
            }
        }


        private void Awake()
        {
            Instance = this;

            // Remove null vehicles from the vehicles list
            for (int i = vehicles.Count - 1; i >= 0; i--)
            {
                if (vehicles[i] == null)
                {
                    Debug.LogWarning("There is a null reference in the vehicles list. Removing. Make sure that" +
                                     " vehicles list does not contain any null references.");
                    vehicles.RemoveAt(i);
                }
            }
        }


        private void Start()
        {
            if (characterBased && !startInVehicle)
            {
                DeactivateAllIncludingActive();
            }
            else
            {
                DeactivateAllExceptActive();
            }

            if (startInVehicle && ActiveVehicle != null)
            {
                EnterVehicle(ActiveVehicle);
                _relativeEnterPosition = new Vector3(-2.5f, 1f, 0.5f); // There was no enter/exit point, make a guess
            }
        }


        private void Update()
        {
            if (!characterBased)
            {
                bool changeVehicleInput = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.ChangeVehicle());

                if (changeVehicleInput)
                {
                    NextVehicle();
                }
            }
            else if (characterObject != null)
            {
                if (location != CharacterLocation.Inside)
                {
                    location = CharacterLocation.OutOfRange;

                    if (!characterObject.activeSelf)
                    {
                        characterObject.SetActive(true);
                    }

                    _enterExitPoints = GameObject.FindGameObjectsWithTag(enterExitTag);

                    _nearestEnterExitPoint = null;
                    float nearestSqrDist = Mathf.Infinity;

                    foreach (GameObject eep in _enterExitPoints)
                    {
                        float sqrDist = Vector3.SqrMagnitude(characterObject.transform.position - eep.transform.position);
                        if (sqrDist < nearestSqrDist)
                        {
                            nearestSqrDist = sqrDist;
                            _nearestEnterExitPoint = eep;
                        }
                    }

                    if (_nearestEnterExitPoint == null)
                    {
                        return;
                    }

                    if (Vector3.Magnitude(Vector3.ProjectOnPlane(
                                              _nearestEnterExitPoint.transform.position -
                                              characterObject.transform.position,
                                              Vector3.up)) < enterDistance)
                    {
                        location = CharacterLocation.Near;
                        _nearestVehicle = _nearestEnterExitPoint.GetComponentInParent<Vehicle>();
                    }
                }

                bool changeVehiclePressed = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.ChangeVehicle());
                if (InputProvider.Instances.Count > 0 && changeVehiclePressed)
                {
                    // Enter vehicle
                    if (location == CharacterLocation.Near && _nearestVehicle.Speed < maxEnterExitVehicleSpeed)
                    {
                        EnterVehicle(_nearestVehicle);
                    }

                    // Exit vehicle
                    else if (location == CharacterLocation.Inside && _nearestVehicle.Speed < maxEnterExitVehicleSpeed)
                    {
                        ExitVehicle(_nearestVehicle);
                    }
                }
            }
        }


        public void EnterVehicle(Vehicle v)
        {
            _nearestVehicle = v;
            if (characterBased)
            {
                characterObject.SetActive(false);
                _relativeEnterPosition = v.transform.InverseTransformPoint(characterObject.transform.position);
                location = CharacterLocation.Inside;
            }
            Instance.ChangeVehicle(v);
        }

        public void ExitVehicle(Vehicle v)
        {
            // Call deactivate all to deactivate on the same frame, preventing 2 audio listeners warning.
            Instance.DeactivateAllIncludingActive();
            location = CharacterLocation.OutOfRange;
            if (characterBased)
            {
                characterObject.transform.position = v.transform.TransformPoint(_relativeEnterPosition);
                characterObject.transform.forward = v.transform.right;
                characterObject.transform.up = Vector3.up;
                characterObject.SetActive(true);
            }
        }


        public void RegisterVehicle(Vehicle v)
        {
            if (!vehicles.Contains(v))
            {
                vehicles.Add(v);
                if (activeVehicleIndex != vehicles.Count - 1)
                {
                    v.enabled = false;
                }
            }
        }


        public void DeregisterVehicle(Vehicle v)
        {
            if (ActiveVehicle == v)
            {
                NextVehicle();
            }
            vehicles.Remove(v);
        }


        /// <summary>
        ///     Changes vehicle to requested vehicle.
        /// </summary>
        /// <param name="index">Index of a vehicle in Vehicles list.</param>
        public void ChangeVehicle(int index)
        {
            if (vehicles.Count == 0) return;

            activeVehicleIndex = index;
            if (activeVehicleIndex >= vehicles.Count)
            {
                activeVehicleIndex = 0;
            }

            DeactivateAllExceptActive();

            onVehicleChanged.Invoke();
        }


        /// <summary>
        ///     Changes vehicle to a vehicle with the requested name if there is such a vehicle.
        /// </summary>
        public void ChangeVehicle(Vehicle ac)
        {
            int vehicleIndex = vehicles.IndexOf(ac);

            if (vehicleIndex >= 0)
            {
                ChangeVehicle(vehicleIndex);
            }
        }


        /// <summary>
        ///     Changes vehicle to a next vehicle on the Vehicles list.
        /// </summary>
        public void NextVehicle()
        {
            if (vehicles.Count == 1)
            {
                return;
            }

            ChangeVehicle(activeVehicleIndex + 1);
        }


        public void PreviousVehicle()
        {
            if (vehicles.Count == 1)
            {
                return;
            }

            int previousIndex = activeVehicleIndex == 0 ? vehicles.Count - 1 : activeVehicleIndex - 1;


            ChangeVehicle(previousIndex);
        }


        public void DeactivateAllExceptActive()
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (i == activeVehicleIndex)
                {
                    vehicles[i].enabled = true;
                }
                else if (putOtherVehiclesToSleep)
                {
                    vehicles[i].enabled = false;
                }
            }
        }


        public void DeactivateAllIncludingActive()
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                vehicles[i].enabled = false;
            }

            onDeactivateAll.Invoke();
        }
    }
}



#if UNITY_EDITOR

namespace NWH.Common.SceneManagement
{
    [CustomEditor(typeof(VehicleChanger))]
    [CanEditMultipleObjects]
    public class VehicleChangerEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            VehicleChanger sc = drawer.GetObject<VehicleChanger>();

            drawer.BeginSubsection("Vehicles");
            drawer.ReorderableList("vehicles");

            //drawer.Field("deactivateAll");
            drawer.Field("putOtherVehiclesToSleep");
            drawer.Field("activeVehicleIndex");
            if (Application.isPlaying)
            {
                drawer.Label("Active Vehicle: " +
                             (Vehicle.ActiveVehicle == null ? "None" : Vehicle.ActiveVehicle.name));
            }
            drawer.EndSubsection();


            drawer.BeginSubsection("Character-based Switching");
            if (drawer.Field("characterBased").boolValue)
            {
                drawer.Field("characterObject");
                drawer.Field("enterDistance", true, "m");
                drawer.Field("startInVehicle");
                drawer.Field("enterExitTag");
                drawer.Field("maxEnterExitVehicleSpeed", true, "m/s");
                drawer.Field("location", false);
            }
            drawer.EndSubsection();

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
