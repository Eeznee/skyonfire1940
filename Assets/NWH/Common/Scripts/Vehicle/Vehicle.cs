using NWH.Common.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NWH.Common.Vehicles
{
    /// <summary>
    ///     Base class for all NWH vehicles.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Vehicle : MonoBehaviour
    {
        // Points to the last enabled vehicle.
        public static Vehicle ActiveVehicle
        {
            get
            {
                int activeVehicleCount = ActiveVehicles.Count;
                if (activeVehicleCount == 0)
                {
                    return null;
                }
                else
                {
                    return ActiveVehicles[activeVehicleCount - 1];
                }
            }
        }

        public static List<Vehicle> ActiveVehicles = new List<Vehicle>();

        /// <summary>
        /// Called when active vehicle is changed.
        /// First parameter is the previously active vehicle and the second parameter is the currently active vehicle 
        /// (at the time of the callback). Params can be null.
        /// </summary>
        public static UnityEvent<Vehicle, Vehicle> onActiveVehicleChanged = new UnityEvent<Vehicle, Vehicle>();

        /// <summary>
        ///     Cached value of vehicle rigidbody.
        /// </summary>
        [UnityEngine.Tooltip("    Cached value of vehicle rigidbody.")]
        [NonSerialized]
        public Rigidbody vehicleRigidbody;


        /// <summary>
        ///     Cached value of vehicle transform.
        /// </summary>
        [Tooltip("    Cached value of vehicle transform.")]
        [NonSerialized]
        public Transform vehicleTransform;

        /// <summary>
        /// True if the vehicle can be driven by the player. False if the
        /// vehicle is passive (such as a trailer). A vehicle that has isPlayerControllable
        /// can be the ActiveVehicle.
        /// </summary>
        public bool isPlayerControllable = true;


        #region CAMERA

        /// <summary>
        ///     True when camera is inside vehicle (cockpit, cabin, etc.).
        ///     Set by the 'CameraInsideVehicle' component.
        ///     Used for audio effects.
        /// </summary>
        public bool CameraInsideVehicle
        {
            get { return _cameraInsideVehicle; }
            set
            {
                if (_cameraInsideVehicle && !value)
                {
                    onCameraExitVehicle.Invoke();
                }
                else if (!CameraInsideVehicle && value)
                {
                    onCameraEnterVehicle.Invoke();
                }

                _cameraInsideVehicle = value;
            }
        }

        private bool _cameraInsideVehicle = false;


        /// <summary>
        /// Called when the camera enters the vehicle.
        /// </summary>
        public UnityEvent onCameraEnterVehicle = new UnityEvent();


        /// <summary>
        /// Called when the camera exists the vehicle.
        /// </summary>
        public UnityEvent onCameraExitVehicle = new UnityEvent();

        #endregion



        #region EVENTS

        /// <summary>
        ///     Called when vehicle is put to sleep.
        /// </summary>
        [Tooltip("    Called when vehicle is put to sleep.")]
        [NonSerialized]
        public UnityEvent onDisable = new UnityEvent();

        /// <summary>
        ///     Called when vehicle is woken up.
        /// </summary>
        [Tooltip("    Called when vehicle is woken up.")]
        [NonSerialized]
        public UnityEvent onEnable = new UnityEvent();

        #endregion



        #region MULTIPLAYER

        /// <summary>
        ///     Determines if vehicle is running locally is synchronized over active multiplayer framework.
        /// </summary>
        [Tooltip("    Determines if vehicle is running locally is synchronized over active multiplayer framework.")]
        private bool _multiplayerIsRemote = false;


        /// <summary>
        /// True if the vehicle is a client (remote).
        /// If true the input is expected to be synced through the network.
        /// </summary>
        public bool MultiplayerIsRemote
        {
            get
            {
                return _multiplayerIsRemote;
            }
            set
            {
                if (_multiplayerIsRemote && !value)
                {
                    onMultiplayerStatusChanged.Invoke(false);
                }
                else if (!_multiplayerIsRemote && value)
                {
                    onMultiplayerStatusChanged.Invoke(true);
                }
                _multiplayerIsRemote = value;
            }
        }

        /// <summary>
        /// Invoked when MultiplayerIsRemote value gets changed.
        /// Is true if remote.
        /// </summary>
        public UnityEvent<bool> onMultiplayerStatusChanged = new UnityEvent<bool>();

        #endregion



        #region PHYSICAL_PROPERTIES

        /// <summary>
        ///     Cached acceleration in local coordinates (z-forward)
        /// </summary>
        public Vector3 LocalAcceleration { get; private set; }


        /// <summary>
        ///     Cached acceleration in forward direction in local coordinates (z-forward).
        /// </summary>
        public float LocalForwardAcceleration { get; private set; }


        /// <summary>
        ///     Cached velocity in forward direction in local coordinates (z-forward).
        /// </summary>
        public float LocalForwardVelocity { get; private set; }


        /// <summary>
        ///     Cached velocity in m/s in local coordinates.
        /// </summary>
        public Vector3 LocalVelocity { get; private set; }


        /// <summary>
        ///     Cached speed of the vehicle in the forward direction. ALWAYS POSITIVE.
        ///     For positive/negative version use SpeedSigned.
        /// </summary>
        public float Speed
        {
            get { return LocalForwardVelocity < 0 ? -LocalForwardVelocity : LocalForwardVelocity; }
        }


        /// <summary>
        ///     Cached speed of the vehicle in the forward direction. Can be positive (forward) or negative (reverse).
        ///     Equal to LocalForwardVelocity.
        /// </summary>
        public float SpeedSigned
        {
            get { return LocalForwardVelocity; }
        }


        /// <summary>
        ///     Cached velocity of the vehicle in world coordinates.
        /// </summary>
        public Vector3 Velocity { get; protected set; }


        /// <summary>
        ///     Cached velocity magnitude of the vehicle in world coordinates.
        /// </summary>
        public float VelocityMagnitude { get; protected set; }


        /// <summary>
        /// Cached angular velocity of the vehicle.
        /// </summary>
        public Vector3 AngularVelocity { get; protected set; }


        /// <summary>
        /// Cached angular velocity maginitude of the vehicle.
        /// </summary>
        public float AngularVelocityMagnitude { get; protected set; }

        private Vector3 _prevLocalVelocity;

        #endregion



        public virtual void Awake()
        {
            vehicleTransform = transform;
            vehicleRigidbody = GetComponent<Rigidbody>();
            vehicleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }


        public virtual void FixedUpdate()
        {
            // Pre-calculate values
            _prevLocalVelocity = LocalVelocity;
            Velocity = vehicleRigidbody.velocity;
            LocalVelocity = transform.InverseTransformDirection(Velocity);
            LocalAcceleration = (LocalVelocity - _prevLocalVelocity) / Time.fixedDeltaTime;
            LocalForwardVelocity = LocalVelocity.z;
            LocalForwardAcceleration = LocalAcceleration.z;
            VelocityMagnitude = Velocity.magnitude;
            AngularVelocity = vehicleRigidbody.angularVelocity;
            AngularVelocityMagnitude = AngularVelocity.magnitude;
        }


        public virtual void OnEnable()
        {
            onEnable.Invoke();

            if (isPlayerControllable)
            {
                if (!ActiveVehicles.Contains(this))
                {
                    Vehicle previouslyActiveVehicle = ActiveVehicle;
                    ActiveVehicles.Add(this);
                    Vehicle currentlyActiveVehicle = ActiveVehicle;
                    onActiveVehicleChanged.Invoke(previouslyActiveVehicle, currentlyActiveVehicle);
                }
            }
        }


        public virtual void OnDisable()
        {
            onDisable.Invoke();

            if (isPlayerControllable)
            {
                Vehicle previouslyActiveVehicle = ActiveVehicle;
                ActiveVehicles.RemoveAll(vehicle => vehicle == this);
                Vehicle currentlyActiveVehicle = ActiveVehicle;
                onActiveVehicleChanged.Invoke(previouslyActiveVehicle, currentlyActiveVehicle);
            }
        }
    }
}