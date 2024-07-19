using NWH.Common.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NWH.Common.Demo
{
    /// <summary>
    ///     Demo script provided by Unity Community Wiki - wiki.unity3d.com
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFPSController : MonoBehaviour
    {
        public float gravity = 10.0f;
        public float jumpHeight = 2.0f;
        public float maximumY = 60f;
        public float maxVelocityChange = 10.0f;
        public float minimumY = -60f;
        public float sensitivityX = 15f;
        public float sensitivityY = 15f;

        public float speed = 10.0f;

        private bool _grounded;
        private Rigidbody _rb;
        private float _rotationY;

        private Vector2 _movement;
        private Vector2 _cameraRotationInput;

        private bool PointerOverUI
        {
            get { return EventSystem.current.IsPointerOverGameObject(); }
        }


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _rb.useGravity = false;
        }


        private void LateUpdate()
        {
            _movement = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CharacterMovement());
            _cameraRotationInput = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraRotation());

            if (_grounded)
            {
                // Calculate how fast we should be moving
                Vector3 targetVelocity = new Vector3(_movement.x, 0, _movement.y);
                targetVelocity = transform.TransformDirection(targetVelocity);
                targetVelocity *= speed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = _rb.velocity;
                Vector3 velocityChange = targetVelocity - velocity;
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }

            float timeFactor = Time.deltaTime * 20f;
            float rotationX = transform.localEulerAngles.y + _cameraRotationInput.x * sensitivityX * timeFactor;
            _rotationY += _cameraRotationInput.y * sensitivityY * timeFactor;
            _rotationY = Mathf.Clamp(_rotationY, minimumY, maximumY);
            transform.localEulerAngles = new Vector3(-_rotationY, rotationX, 0);
            _rb.AddForce(new Vector3(0, -gravity * _rb.mass, 0));

            _grounded = false;
        }


        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2 * jumpHeight * gravity);
        }


        private void OnCollisionStay()
        {
            _grounded = true;
        }
    }
}