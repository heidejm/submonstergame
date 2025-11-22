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
                GridCoordinate hoveredCoord = _gameManager.WorldToGridPosition(hit.point);

                // Update hovered cell
                if (!_hoveredCell.HasValue || !_hoveredCell.Value.Equals(hoveredCoord))
                {
                    _hoveredCell = hoveredCoord;
                    OnHoveredCellChanged(hoveredCoord);
                }

                // Handle click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Debug.Log($"Clicked at world position: {hit.point}, grid: {hoveredCoord}");
                    OnCellClicked(hoveredCoord);
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
                // Move succeeded - refresh movement indicators after movement completes
                // The indicators will refresh when the active entity changes or turn ends
            }
        }

        private void HandleActiveEntityChanged(IEntity entity)
        {
            // Refresh movement range display when active entity changes
            if (_showingMovementRange)
            {
                ShowMovementRange();
            }
        }

        /// <summary>
        /// Shows movement range indicators for the active entity.
        /// </summary>
        public void ShowMovementRange()
        {
            ClearMovementIndicators();

            if (_gameManager == null || _movementIndicatorPrefab == null)
                return;

            var reachable = _gameManager.GetReachablePositions();

            foreach (var coord in reachable)
            {
                Vector3 worldPos = _gameManager.GridToWorldPosition(coord);
                // Offset slightly above ground to avoid z-fighting
                worldPos.y = 0.05f;

                GameObject indicator = Instantiate(_movementIndicatorPrefab, worldPos, Quaternion.identity);
                indicator.transform.SetParent(transform);

                // Set color
                var renderer = indicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = _validMoveColor;
                }

                _movementIndicators.Add(indicator);
            }

            _showingMovementRange = true;
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
