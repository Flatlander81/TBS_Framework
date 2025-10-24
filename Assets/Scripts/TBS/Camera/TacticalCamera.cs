using UnityEngine;
using UnityEngine.InputSystem;

namespace TBS.Camera
{
    /// <summary>
    /// Tactical camera controller with pan, zoom, and rotation.
    /// Designed for top-down tactical gameplay.
    /// </summary>
    public class TacticalCamera : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private bool useScreenEdgePan = true;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private bool enableRotation = true;

        [Header("Bounds")]
        [SerializeField] private bool limitPan = true;
        [SerializeField] private Vector2 panLimitX = new Vector2(-50, 50);
        [SerializeField] private Vector2 panLimitZ = new Vector2(-50, 50);

        [Header("Tilt Settings")]
        [SerializeField] private float tiltAngle = 45f;
        [SerializeField] private bool lockTilt = true;

        private UnityEngine.Camera cam;
        private Vector3 targetPosition;
        private float currentZoom;

        private void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
            targetPosition = transform.position;

            // Set initial tilt
            Vector3 rotation = transform.eulerAngles;
            rotation.x = tiltAngle;
            transform.eulerAngles = rotation;

            // Set initial zoom
            if (cam.orthographic)
            {
                currentZoom = cam.orthographicSize;
            }
            else
            {
                currentZoom = transform.position.y;
            }
        }

        private void Update()
        {
            HandlePan();
            HandleZoom();
            HandleRotation();

            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSpeed);
        }

        private void HandlePan()
        {
            Vector3 moveDirection = Vector3.zero;

            // WASD keys
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    moveDirection += Vector3.forward;
                }
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    moveDirection += Vector3.back;
                }
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    moveDirection += Vector3.left;
                }
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    moveDirection += Vector3.right;
                }
            }

            // Screen edge panning
            if (useScreenEdgePan)
            {
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    Vector2 mousePos = mouse.position.ReadValue();
                    if (mousePos.y >= Screen.height - panBorderThickness)
                    {
                        moveDirection += Vector3.forward;
                    }
                    if (mousePos.y <= panBorderThickness)
                    {
                        moveDirection += Vector3.back;
                    }
                    if (mousePos.x >= Screen.width - panBorderThickness)
                    {
                        moveDirection += Vector3.right;
                    }
                    if (mousePos.x <= panBorderThickness)
                    {
                        moveDirection += Vector3.left;
                    }
                }
            }

            // Apply rotation to movement direction
            moveDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * moveDirection;

            // Apply movement
            targetPosition += moveDirection.normalized * panSpeed * Time.deltaTime;

            // Apply limits
            if (limitPan)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);
            }
        }

        private void HandleZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            float scroll = mouse.scroll.ReadValue().y / 120f; // Normalize scroll value

            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                if (cam.orthographic)
                {
                    cam.orthographicSize = currentZoom;
                }
                else
                {
                    Vector3 pos = targetPosition;
                    pos.y = currentZoom;
                    targetPosition = pos;
                }
            }
        }

        private void HandleRotation()
        {
            if (!enableRotation)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                // Q and E for rotation
                if (keyboard.qKey.isPressed)
                {
                    transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
                }
                if (keyboard.eKey.isPressed)
                {
                    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                }
            }

            // Lock tilt angle
            if (lockTilt)
            {
                Vector3 rotation = transform.eulerAngles;
                rotation.x = tiltAngle;
                rotation.z = 0;
                transform.eulerAngles = rotation;
            }
        }

        /// <summary>
        /// Focuses the camera on a specific world position.
        /// </summary>
        public void FocusOnPosition(Vector3 position)
        {
            targetPosition = position;
            targetPosition.y = currentZoom;
        }

        /// <summary>
        /// Sets the camera rotation to a specific angle.
        /// </summary>
        public void SetRotation(float yRotation)
        {
            Vector3 rotation = transform.eulerAngles;
            rotation.y = yRotation;
            rotation.x = tiltAngle;
            rotation.z = 0;
            transform.eulerAngles = rotation;
        }

        /// <summary>
        /// Resets the camera to default position and rotation.
        /// </summary>
        public void ResetCamera()
        {
            targetPosition = new Vector3(0, currentZoom, 0);
            SetRotation(0);
        }
    }
}
