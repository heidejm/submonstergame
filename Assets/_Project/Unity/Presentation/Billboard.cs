using UnityEngine;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Makes an object always face the camera.
    /// Attach to health bar canvases or other UI elements that should face the player.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [Tooltip("If null, will find Camera.main")]
        [SerializeField] private Camera _targetCamera;

        [Tooltip("Lock rotation on specific axes")]
        [SerializeField] private bool _lockX = false;
        [SerializeField] private bool _lockY = false;
        [SerializeField] private bool _lockZ = false;

        private void Start()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
                if (_targetCamera == null) return;
            }

            // Face the camera
            Vector3 targetPosition = _targetCamera.transform.position;

            // Apply axis locks
            if (_lockX) targetPosition.x = transform.position.x;
            if (_lockY) targetPosition.y = transform.position.y;
            if (_lockZ) targetPosition.z = transform.position.z;

            transform.LookAt(targetPosition);

            // Flip 180 degrees so the front faces the camera (not the back)
            transform.Rotate(0, 180, 0);
        }
    }
}
