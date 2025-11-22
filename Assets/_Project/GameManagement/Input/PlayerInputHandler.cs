using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SubGame.Core.Grid;
using SubGame.Core.Entities;

namespace SubGame.GameManagement.Input
{
    /// <summary>
    /// Handles player input for the game.
    /// Converts mouse clicks to game commands.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Camera _mainCamera;

        [Header("Input Settings")]
        [SerializeField] private LayerMask _groundLayerMask = -1;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _movementIndicatorPrefab;
        [SerializeField] private Color _validMoveColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color _invalidMoveColor = new Color(1f, 0f, 0f, 0.5f);

        private List<GameObject> _movementIndicators = new List<GameObject>();
        private GridCoordinate? _hoveredCell;
        private bool _showingMovementRange;

        private void Start()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_gameManager == null)
            {
                _gameManager = FindFirstObjectByType<GameManager>();
            }

            // Subscribe to active entity changes to update movement range display
            if (_gameManager != null)
            {
                _gameManager.OnActiveEntityChanged += HandleActiveEntityChanged;
            }
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnActiveEntityChanged -= HandleActiveEntityChanged;
            }

            ClearMovementIndicators();
        }

        private void Update()
        {
            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleMouseInput()
        {
            if (_mainCamera == null || _gameManager == null || Mouse.current == null)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            // Debug: try raycast without layer mask first
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Force Y to 0 for ground-level gameplay (avoids floating point issues)
                Vector3 hitPoint = hit.point;
                hitPoint.y = 0f;
                GridCoordinate hoveredCoord = _gameManager.WorldToGridPosition(hitPoint);

                // Update hovered cell
                if (!_hoveredCell.HasValue || !_hoveredCell.Value.Equals(hoveredCoord))
                {
                    _hoveredCell = hoveredCoord;
                    OnHoveredCellChanged(hoveredCoord);
                }

                // Handle left click - move
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Debug.Log($"Left-click at world position: {hit.point}, grid: {hoveredCoord}");
                    OnCellClicked(hoveredCoord);
                }

                // Handle right click - attack
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    Debug.Log($"Right-click at world position: {hit.point}, grid: {hoveredCoord}");
                    OnCellRightClicked(hoveredCoord);
                }
            }
            else
            {
                _hoveredCell = null;
            }
        }

        private void HandleKeyboardInput()
        {
            if (_gameManager == null || Keyboard.current == null)
                return;

            // End turn (Space key)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _gameManager.EndCurrentTurn();
            }
        }

        private void OnHoveredCellChanged(GridCoordinate coord)
        {
            // Could add hover effects here
        }

        private void OnCellClicked(GridCoordinate coord)
        {
            if (_gameManager == null)
                return;

            // Try to move the active entity to the clicked position
            if (_gameManager.TryMoveActiveEntity(coord))
            {
                // Move succeeded - refresh movement indicators
                ShowMovementRange();
            }
        }

        private void OnCellRightClicked(GridCoordinate coord)
        {
            if (_gameManager == null)
                return;

            // Try to attack entity at the clicked position
            if (_gameManager.TryAttackAtPosition(coord))
            {
                // Attack succeeded - refresh indicators (attack range may have changed)
                ShowMovementRange();
            }
        }

        private void HandleActiveEntityChanged(IEntity entity)
        {
            // Always show movement range when a new entity becomes active
            if (entity != null)
            {
                ShowMovementRange();
            }
            else
            {
                HideMovementRange();
            }
        }

        /// <summary>
        /// Shows movement range indicators for the active entity.
        /// </summary>
        public void ShowMovementRange()
        {
            ClearMovementIndicators();

            if (_gameManager == null)
                return;

            var reachable = _gameManager.GetReachablePositions();
            float cellSize = _gameManager.CellSize;

            foreach (var coord in reachable)
            {
                Vector3 worldPos = _gameManager.GridToWorldPosition(coord);
                // Position at ground level with slight offset
                worldPos.y = 0.05f;

                GameObject indicator = CreateIndicator(worldPos, cellSize, _validMoveColor);
                _movementIndicators.Add(indicator);
            }

            // Also show attack range indicators (in red/orange) - positioned above entities
            var attackable = _gameManager.GetAttackableTargets();
            foreach (var target in attackable)
            {
                Vector3 worldPos = _gameManager.GridToWorldPosition(target.Position);
                worldPos.y = 1.5f; // Above entity heads so it's visible

                GameObject indicator = CreateIndicator(worldPos, cellSize * 0.6f, _invalidMoveColor);
                _movementIndicators.Add(indicator);
            }

            _showingMovementRange = true;
        }

        private GameObject CreateIndicator(Vector3 position, float size, Color color)
        {
            GameObject indicator;

            if (_movementIndicatorPrefab != null)
            {
                indicator = Instantiate(_movementIndicatorPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create a simple quad indicator dynamically
                indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
                indicator.transform.position = position;
                indicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Flat on ground
                indicator.transform.localScale = new Vector3(size * 0.8f, size * 0.8f, 1f);

                // Remove collider so it doesn't block raycasts
                var collider = indicator.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            indicator.transform.SetParent(transform);

            // Set color with transparency
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create a new material with transparency
                var mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color = color;
                renderer.material = mat;
            }

            return indicator;
        }

        /// <summary>
        /// Hides movement range indicators.
        /// </summary>
        public void HideMovementRange()
        {
            ClearMovementIndicators();
            _showingMovementRange = false;
        }

        /// <summary>
        /// Toggles movement range display.
        /// </summary>
        public void ToggleMovementRange()
        {
            if (_showingMovementRange)
            {
                HideMovementRange();
            }
            else
            {
                ShowMovementRange();
            }
        }

        private void ClearMovementIndicators()
        {
            foreach (var indicator in _movementIndicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }
            _movementIndicators.Clear();
        }
    }
}
