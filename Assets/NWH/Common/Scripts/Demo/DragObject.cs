using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

#endif

namespace NWH.Common.Demo
{
    /// <summary>
    ///     Simple script that drags Rigidbody behind the mouse cursor when MMB is held down.
    /// </summary>
    public class DragObject : MonoBehaviour
    {
        private Rigidbody _rb;
        private bool _dragging;
        private Ray _mouseRay;
        private RaycastHit _hit;
        private Vector3 _localHitPoint;
        private Vector3 _globalHitPoint;
        private Vector3 _rbScreenPos;
        private Camera _cam;
        private float _distance;
        private float _forceMagnitude;
        private Vector3 _direction;
        private Vector3 _force;
        private float _forceXz;
        private Vector3 _resultantForce;
        private bool _draggingButtonWasPressed;
        private bool _draggingButtonWasReleased;
        private Vector2 _mousePosition;


        private void Start()
        {
            _cam = GetComponent<Camera>();
        }


        private void Update()
        {
            if (_cam == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            _mousePosition = Mouse.current.position.ReadValue();
            _mouseRay = _cam.ScreenPointToRay(_mousePosition);
            _draggingButtonWasPressed = Mouse.current.middleButton.wasPressedThisFrame;
            _draggingButtonWasReleased = Mouse.current.middleButton.wasReleasedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            _mousePosition = UnityEngine.Input.mousePosition;
            _mouseRay = _cam.ScreenPointToRay(_mousePosition);
            _draggingButtonWasPressed = UnityEngine.Input.GetKeyDown(KeyCode.Mouse2);
            _draggingButtonWasReleased = UnityEngine.Input.GetKeyUp(KeyCode.Mouse2);
#endif

            _mouseRay = _cam.ScreenPointToRay(_mousePosition);

            if (_draggingButtonWasPressed && !_dragging)
            {
                if (Physics.Raycast(_mouseRay, out _hit, 600f))
                {
                    _rb = _hit.transform.GetComponent<Rigidbody>();
                    if (_rb != null)
                    {
                        _dragging = true;
                        _localHitPoint = _rb.transform.InverseTransformPoint(_hit.point);
                    }
                    else
                    {
                        _dragging = false;
                    }
                }
            }

            if (_draggingButtonWasReleased)
            {
                _dragging = false;
            }
        }


        private void FixedUpdate()
        {
            if (_dragging)
            {
                _globalHitPoint = _rb.transform.TransformPoint(_localHitPoint);
                _rbScreenPos = _cam.WorldToScreenPoint(_globalHitPoint);

                _distance = Vector2.Distance(_mousePosition, _rbScreenPos);
                _forceMagnitude = _distance * _rb.mass * 0.1f;

                _direction = ((Vector3)_mousePosition - _rbScreenPos).normalized;
                _force = _forceMagnitude * _direction;
                _forceXz = _force.x + _force.z;
                _resultantForce = new Vector3(_forceXz * transform.right.x, _force.y, _forceXz * transform.right.z);
                _resultantForce = Vector3.ClampMagnitude(_resultantForce, _rb.mass * 100f);
                _rb.AddForceAtPosition(_resultantForce, _globalHitPoint);
            }
        }


        private void OnDrawGizmos()
        {
            if (_dragging)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_globalHitPoint, 0.01f);
            }
        }
    }
}