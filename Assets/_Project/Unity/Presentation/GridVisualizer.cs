using UnityEngine;
using SubGame.Core.Grid;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Visualizes the 3D grid in the Unity scene.
    /// Uses Gizmos for editor visualization and optional runtime debug rendering.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [SerializeField]
        [Tooltip("Width of the grid (X axis)")]
        private int _width = 10;

        [SerializeField]
        [Tooltip("Height of the grid (Y axis - depth in ocean)")]
        private int _height = 10;

        [SerializeField]
        [Tooltip("Depth of the grid (Z axis)")]
        private int _depth = 10;

        [Header("Visualization Settings")]
        [SerializeField]
        [Tooltip("Size of each grid cell in world units")]
        private float _cellSize = 1f;

        [SerializeField]
        [Tooltip("Color of the grid lines")]
        private Color _gridColor = new Color(0.5f, 0.5f, 1f, 0.3f);

        [SerializeField]
        [Tooltip("Color of the grid boundary")]
        private Color _boundaryColor = new Color(1f, 1f, 0f, 0.5f);

        [SerializeField]
        [Tooltip("Show grid cells as wireframe cubes")]
        private bool _showCells = true;

        [SerializeField]
        [Tooltip("Show the outer boundary of the grid")]
        private bool _showBoundary = true;

        [SerializeField]
        [Tooltip("Only show every Nth layer to reduce visual clutter")]
        [Range(1, 10)]
        private int _layerStep = 1;

        private IGridSystem _gridSystem;

        /// <summary>
        /// Gets or sets the grid system to visualize.
        /// If null, creates a new one based on inspector dimensions.
        /// </summary>
        public IGridSystem GridSystem
        {
            get => _gridSystem;
            set => _gridSystem = value;
        }

        /// <summary>
        /// The size of each cell in world units.
        /// </summary>
        public float CellSize => _cellSize;

        private void Awake()
        {
            // Create default grid if none is set
            if (_gridSystem == null)
            {
                _gridSystem = new GridSystem(_width, _height, _depth);
            }
        }

        /// <summary>
        /// Converts a grid coordinate to world position.
        /// </summary>
        /// <param name="coord">Grid coordinate</param>
        /// <returns>World position (center of the cell)</returns>
        public Vector3 GridToWorldPosition(GridCoordinate coord)
        {
            return transform.position + new Vector3(
                coord.X * _cellSize + _cellSize / 2f,
                coord.Y * _cellSize + _cellSize / 2f,
                coord.Z * _cellSize + _cellSize / 2f
            );
        }

        /// <summary>
        /// Converts a world position to the nearest grid coordinate.
        /// </summary>
        /// <param name="worldPos">World position</param>
        /// <returns>Nearest grid coordinate</returns>
        public GridCoordinate WorldToGridCoordinate(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - transform.position;
            return new GridCoordinate(
                Mathf.FloorToInt(localPos.x / _cellSize),
                Mathf.FloorToInt(localPos.y / _cellSize),
                Mathf.FloorToInt(localPos.z / _cellSize)
            );
        }

        /// <summary>
        /// Checks if a world position is within the grid bounds.
        /// </summary>
        public bool IsWorldPositionInGrid(Vector3 worldPos)
        {
            var coord = WorldToGridCoordinate(worldPos);
            return _gridSystem?.IsValidCoordinate(coord) ?? false;
        }

        private void OnDrawGizmos()
        {
            DrawGrid();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw more detailed grid when selected
            DrawGrid();
        }

        private void DrawGrid()
        {
            int width = _gridSystem?.Width ?? _width;
            int height = _gridSystem?.Height ?? _height;
            int depth = _gridSystem?.Depth ?? _depth;

            // Draw boundary
            if (_showBoundary)
            {
                Gizmos.color = _boundaryColor;
                Vector3 size = new Vector3(width * _cellSize, height * _cellSize, depth * _cellSize);
                Vector3 center = transform.position + size / 2f;
                Gizmos.DrawWireCube(center, size);
            }

            // Draw grid cells
            if (_showCells)
            {
                Gizmos.color = _gridColor;

                for (int x = 0; x < width; x += _layerStep)
                {
                    for (int y = 0; y < height; y += _layerStep)
                    {
                        for (int z = 0; z < depth; z += _layerStep)
                        {
                            Vector3 cellCenter = transform.position + new Vector3(
                                x * _cellSize + _cellSize / 2f,
                                y * _cellSize + _cellSize / 2f,
                                z * _cellSize + _cellSize / 2f
                            );

                            Gizmos.DrawWireCube(cellCenter, Vector3.one * _cellSize * 0.95f);
                        }
                    }
                }
            }

            // Draw axis indicators at origin
            DrawAxisIndicators();
        }

        private void DrawAxisIndicators()
        {
            float axisLength = _cellSize * 2f;
            Vector3 origin = transform.position;

            // X axis - Red
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.right * axisLength);

            // Y axis - Green (up/down in ocean)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.up * axisLength);

            // Z axis - Blue
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + Vector3.forward * axisLength);
        }

        /// <summary>
        /// Sets the grid dimensions at runtime.
        /// </summary>
        /// <param name="width">Width of the grid (X axis)</param>
        /// <param name="height">Height of the grid (Y axis)</param>
        /// <param name="depth">Depth of the grid (Z axis)</param>
        public void SetDimensions(int width, int height, int depth)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _depth = Mathf.Max(1, depth);
            _gridSystem = new GridSystem(_width, _height, _depth);
        }

        /// <summary>
        /// Highlights a specific cell in the grid.
        /// Call from other scripts to show selected cells.
        /// </summary>
        /// <param name="coord">Coordinate to highlight</param>
        /// <param name="color">Highlight color</param>
        public void HighlightCell(GridCoordinate coord, Color color)
        {
            if (_gridSystem == null || !_gridSystem.IsValidCoordinate(coord))
                return;

            Gizmos.color = color;
            Vector3 cellCenter = GridToWorldPosition(coord);
            Gizmos.DrawCube(cellCenter, Vector3.one * _cellSize * 0.9f);
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Updates grid dimensions from inspector.
        /// Called automatically when values change in inspector.
        /// </summary>
        private void OnValidate()
        {
            _width = Mathf.Max(1, _width);
            _height = Mathf.Max(1, _height);
            _depth = Mathf.Max(1, _depth);
            _cellSize = Mathf.Max(0.1f, _cellSize);

            // Recreate grid system if dimensions change
            if (_gridSystem == null ||
                _gridSystem.Width != _width ||
                _gridSystem.Height != _height ||
                _gridSystem.Depth != _depth)
            {
                _gridSystem = new GridSystem(_width, _height, _depth);
            }
        }
        #endif
    }
}
