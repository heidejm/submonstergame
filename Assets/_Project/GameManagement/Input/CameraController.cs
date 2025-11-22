using UnityEngine;
using UnityEngine.InputSystem;

namespace SubGame.GameManagement.Input
{
    /// <summary>
    /// Simple camera controller for navigating the 3D grid.
    /// WASD to pan, Q/E to rotate, scroll to zoom.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _rotateSpeed = 60f;
        [SerializeField] private float _zoomSpeed = 5f;

        [Header("Zoom Limits")]
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 60f; // Larger for bigger grid

        [Header("Target")]
        [SerializeField] private Transform _target;

        private float _currentZoom = 35f; // Zoomed out more for larger grid
        private float _currentRotation = 45f;
        private Vector3 _targetPosition;

        private void Start()
        {
            if (_target == null)
            {
                // Create a target pivot at the center of a 15x15 grid with 2 unit cells
                // Grid spans 0-30 units, so center is at 15
                GameObject pivot = new GameObject("CameraPivot");
                pivot.transform.position = new Vector3(15f, 5f, 15f); // Centered, slightly elevated
                _target = pivot.transform;
            }

            _targetPosition = _target.position;
            UpdateCameraPosition();
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            UpdateCameraPosition();
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current == null)
                return;

            // Pan with WASD
            Vector3 moveDir = Vector3.zero;

            if (Keyboard.current.wKey.isPressed)
                moveDir += GetForwardDirection();
            if (Keyboard.current.sKey.isPressed)
                moveDir -= GetForwardDirection();
            if (Keyboard.current.aKey.isPressed)
                moveDir -= GetRightDirection();
            if (Keyboard.current.dKey.isPressed)
                moveDir += GetRightDirection();

            if (moveDir != Vector3.zero)
            {
                _targetPosition += moveDir.normalized * _panSpeed * Time.deltaTime;
            }

            // Rotate with Q/E
            if (Keyboard.current.qKey.isPressed)
                _currentRotation -= _rotateSpeed * Time.deltaTime;
            if (Keyboard.current.eKey.isPressed)
                _currentRotation += _rotateSpeed * Time.deltaTime;
        }

        private void HandleMouseInput()
        {
            if (Mouse.current == null)
                return;

            // Zoom with scroll wheel
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0)
            {
                _currentZoom -= scroll * _zoomSpeed * Time.deltaTime * 10f;
                _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
            }
        }

        private void UpdateCameraPosition()
        {
            if (_target != null)
            {
                _target.position = _targetPosition;
            }

            // Calculate camera position based on rotation and zoom
            // Camera orbits around target, positioned above and behind
            float radians = _currentRotation * Mathf.Deg2Rad;
            float horizontalDist = _currentZoom * 0.7f;
            float height = _currentZoom * 0.8f;

            Vector3 cameraPos = new Vector3(
                _targetPosition.x - Mathf.Sin(radians) * horizontalDist,
                _targetPosition.y + height,
                _targetPosition.z - Mathf.Cos(radians) * horizontalDist
            );

            transform.position = cameraPos;
            transform.LookAt(_targetPosition);
        }

        private Vector3 GetForwardDirection()
        {
            // Forward relative to camera rotation (ignoring Y)
            float radians = _currentRotation * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
        }

        private Vector3 GetRightDirection()
        {
            // Right relative to camera rotation
            float radians = (_currentRotation + 90) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
        }

        /// <summary>
        /// Centers the camera on a world position.
        /// </summary>
        public void CenterOn(Vector3 worldPosition)
        {
            _targetPosition = worldPosition;
        }

        /// <summary>
        /// Centers the camera on the grid center.
        /// </summary>
        public void CenterOnGrid(int gridWidth, int gridDepth, float cellSize)
        {
            _targetPosition = new Vector3(
                gridWidth * cellSize / 2f,
                0f,
                gridDepth * cellSize / 2f
            );
        }
    }
}
