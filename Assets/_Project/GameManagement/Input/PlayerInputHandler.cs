using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SubGame.Core.Grid;
using SubGame.Core.Entities;
using System.Linq;

namespace SubGame.GameManagement.Input
{
    /// <summary>
    /// Handles player input for the game.
    /// Converts mouse clicks to game commands.
    /// Uses 3D movement indicators that players click to move to positions.
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
        [SerializeField] private Color _validMoveColor = new Color(0f, 1f, 0f, 0.6f);
        [SerializeField] private Color _invalidMoveColor = new Color(1f, 0f, 0f, 0.6f);

        [Header("3D Indicator Settings")]
        [SerializeField] private float _indicatorSize = 0.6f; // Larger for better visibility with 2x cells
        [SerializeField] private bool _useSpheresForIndicators = true;

        [Header("Path Preview")]
        [SerializeField] private Color _pathPreviewColor = new Color(0f, 0.8f, 1f, 0.8f);
        [SerializeField] private float _pathNodeSize = 0.35f; // Slightly larger path nodes

        private List<GameObject> _movementIndicators = new List<GameObject>();
        private List<GameObject> _pathPreviewObjects = new List<GameObject>();
        private GridCoordinate? _hoveredCell;
        private bool _showingMovementRange;
        private MovementIndicator _hoveredIndicator;

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
            ClearPathPreview();
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

            // First, check if we're hovering over a movement indicator
            MovementIndicator currentHovered = null;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Check if we hit a movement indicator
                currentHovered = hit.collider.GetComponent<MovementIndicator>();

                if (currentHovered != null)
                {
                    // Handle clicking on movement/attack indicators
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        if (currentHovered.IsAttackIndicator)
                        {
                            Debug.Log($"Clicked attack indicator at {currentHovered.TargetCoordinate}");
                            OnCellRightClicked(currentHovered.TargetCoordinate);
                        }
                        else
                        {
                            Debug.Log($"Clicked movement indicator at {currentHovered.TargetCoordinate}");
                            OnCellClicked(currentHovered.TargetCoordinate);
                        }
                    }
                    // Right-click on attack indicator also attacks
                    else if (Mouse.current.rightButton.wasPressedThisFrame && currentHovered.IsAttackIndicator)
                    {
                        Debug.Log($"Right-clicked attack indicator at {currentHovered.TargetCoordinate}");
                        OnCellRightClicked(currentHovered.TargetCoordinate);
                    }
                }
                else
                {
                    // Clicked on something other than an indicator (ground, entity, etc.)
                    Vector3 hitPoint = hit.point;
                    GridCoordinate hoveredCoord = _gameManager.WorldToGridPosition(hitPoint);

                    // Update hovered cell
                    if (!_hoveredCell.HasValue || !_hoveredCell.Value.Equals(hoveredCoord))
                    {
                        _hoveredCell = hoveredCoord;
                        OnHoveredCellChanged(hoveredCoord);
                    }

                    // Handle right click - attack (can attack without clicking indicator)
                    if (Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        Debug.Log($"Right-click at world position: {hit.point}, grid: {hoveredCoord}");
                        OnCellRightClicked(hoveredCoord);
                    }
                }
            }
            else
            {
                _hoveredCell = null;
            }

            // Update hover visual feedback
            UpdateHoveredIndicator(currentHovered);
        }

        private void UpdateHoveredIndicator(MovementIndicator newHovered)
        {
            // Clear previous hover effect
            if (_hoveredIndicator != null && _hoveredIndicator != newHovered)
            {
                SetIndicatorHighlight(_hoveredIndicator, false);
                ClearPathPreview();
            }

            // Apply new hover effect and show path preview
            if (newHovered != null && newHovered != _hoveredIndicator)
            {
                SetIndicatorHighlight(newHovered, true);

                // Show path preview for movement indicators (not attack)
                if (!newHovered.IsAttackIndicator)
                {
                    ShowPathPreview(newHovered.TargetCoordinate);
                }
            }

            _hoveredIndicator = newHovered;
        }

        /// <summary>
        /// Shows a preview of the path to the target position.
        /// </summary>
        private void ShowPathPreview(GridCoordinate targetPosition)
        {
            ClearPathPreview();

            if (_gameManager == null)
                return;

            var path = _gameManager.GetPathTo(targetPosition);

            if (path == null || path.Count < 2)
                return;

            // Create small spheres along the path (skip first which is current position)
            for (int i = 1; i < path.Count - 1; i++) // Skip first and last (destination has indicator)
            {
                Vector3 worldPos = _gameManager.GridToWorldPosition(path[i]);

                GameObject pathNode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pathNode.transform.position = worldPos;
                pathNode.transform.localScale = new Vector3(_pathNodeSize, _pathNodeSize, _pathNodeSize);
                pathNode.transform.SetParent(transform);
                pathNode.name = $"PathNode_{i}";

                // Remove collider so it doesn't interfere with selection
                var collider = pathNode.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                // Set color and disable shadows
                var renderer = pathNode.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = CreateTransparentMaterial(_pathPreviewColor);
                    renderer.material = mat;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }

                _pathPreviewObjects.Add(pathNode);
            }

            // Also draw lines connecting path nodes (optional visual enhancement)
            CreatePathLines(path);
        }

        /// <summary>
        /// Creates line renderers to connect path nodes.
        /// </summary>
        private void CreatePathLines(IReadOnlyList<GridCoordinate> path)
        {
            if (path.Count < 2)
                return;

            GameObject lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(transform);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.positionCount = path.Count;
            line.startWidth = 0.15f;
            line.endWidth = 0.15f;

            // Create a simple line material
            var lineMat = new Material(Shader.Find("Sprites/Default"));
            lineMat.color = _pathPreviewColor;
            line.material = lineMat;

            // Set positions
            for (int i = 0; i < path.Count; i++)
            {
                line.SetPosition(i, _gameManager.GridToWorldPosition(path[i]));
            }

            _pathPreviewObjects.Add(lineObj);
        }

        /// <summary>
        /// Clears the path preview visualization.
        /// </summary>
        private void ClearPathPreview()
        {
            foreach (var obj in _pathPreviewObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _pathPreviewObjects.Clear();
        }

        private void SetIndicatorHighlight(MovementIndicator indicator, bool highlighted)
        {
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color baseColor = indicator.IsAttackIndicator ? _invalidMoveColor : _validMoveColor;
                if (highlighted)
                {
                    // Brighter when hovered
                    renderer.material.color = new Color(
                        Mathf.Min(1f, baseColor.r + 0.3f),
                        Mathf.Min(1f, baseColor.g + 0.3f),
                        Mathf.Min(1f, baseColor.b + 0.3f),
                        0.9f
                    );
                }
                else
                {
                    renderer.material.color = baseColor;
                }
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
        /// Creates 3D indicators at all reachable positions including vertical movement.
        /// </summary>
        public void ShowMovementRange()
        {
            ClearMovementIndicators();

            if (_gameManager == null)
                return;

            var reachable = _gameManager.GetReachablePositions();

            foreach (var coord in reachable)
            {
                // Get actual 3D world position (includes Y coordinate)
                Vector3 worldPos = _gameManager.GridToWorldPosition(coord);

                GameObject indicator = CreateMovementIndicator(worldPos, coord, false);
                _movementIndicators.Add(indicator);
            }

            // Also show attack range indicators (in red)
            var attackable = _gameManager.GetAttackableTargets();
            foreach (var target in attackable)
            {
                Vector3 worldPos = _gameManager.GridToWorldPosition(target.Position);
                // Position slightly above the entity
                worldPos.y += 0.5f;

                GameObject indicator = CreateMovementIndicator(worldPos, target.Position, true);
                _movementIndicators.Add(indicator);
            }

            _showingMovementRange = true;
        }

        /// <summary>
        /// Creates a 3D movement indicator at the specified position.
        /// </summary>
        /// <param name="position">World position for the indicator</param>
        /// <param name="coordinate">Grid coordinate this indicator represents</param>
        /// <param name="isAttack">True if this is an attack indicator</param>
        /// <returns>The created indicator GameObject</returns>
        private GameObject CreateMovementIndicator(Vector3 position, GridCoordinate coordinate, bool isAttack)
        {
            GameObject indicator;
            Color color = isAttack ? _invalidMoveColor : _validMoveColor;
            float size = isAttack ? _indicatorSize * 0.8f : _indicatorSize;

            if (_movementIndicatorPrefab != null)
            {
                indicator = Instantiate(_movementIndicatorPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create a sphere for 3D visibility
                if (_useSpheresForIndicators)
                {
                    indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    indicator.transform.localScale = new Vector3(size, size, size);
                }
                else
                {
                    // Create a cube for more precise grid alignment
                    indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    indicator.transform.localScale = new Vector3(size, size, size);
                }

                indicator.transform.position = position;

                // Keep collider for click detection - raycasts need non-trigger colliders
                // The collider is needed to detect clicks on the indicator
            }

            indicator.transform.SetParent(transform);
            indicator.name = isAttack ? $"AttackIndicator_{coordinate}" : $"MoveIndicator_{coordinate}";

            // Add MovementIndicator component for click detection
            var movementIndicator = indicator.AddComponent<MovementIndicator>();
            movementIndicator.Initialize(coordinate, isAttack);

            // Set color with transparency and disable shadows
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = CreateTransparentMaterial(color);
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return indicator;
        }

        /// <summary>
        /// Creates a transparent material with the specified color.
        /// </summary>
        private Material CreateTransparentMaterial(Color color)
        {
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
            return mat;
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
            _hoveredIndicator = null;
            ClearPathPreview();

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
