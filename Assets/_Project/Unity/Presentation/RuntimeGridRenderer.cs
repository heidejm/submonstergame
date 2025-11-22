using UnityEngine;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Renders grid lines at runtime using GL.Lines.
    /// Visible in Game view, not just Scene view.
    /// Supports multiple Y levels for 3D grid visualization.
    /// </summary>
    public class RuntimeGridRenderer : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _width = 15;
        [SerializeField] private int _height = 15;
        [SerializeField] private int _depth = 15;
        [SerializeField] private float _cellSize = 2f; // Larger cells for better visibility

        [Header("Visual Settings")]
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        [SerializeField] private Color _boundaryColor = new Color(1f, 1f, 0f, 0.8f);
        [SerializeField] private Color _verticalLineColor = new Color(0.3f, 0.7f, 1f, 0.2f);
        [SerializeField] private float _lineYOffset = 0.02f;

        [Header("3D Display Options")]
        [SerializeField] private bool _showAllLevels = false; // Only show highlighted level by default
        [SerializeField] private bool _showVerticalLines = false; // Cleaner look
        [SerializeField] private int _highlightedLevel = 0;
        [SerializeField] private Color _highlightedLevelColor = new Color(0f, 1f, 0.5f, 0.6f);

        private Material _lineMaterial;

        /// <summary>
        /// Sets the grid dimensions.
        /// </summary>
        public void SetDimensions(int width, int height, int depth, float cellSize = 1f)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _cellSize = cellSize;
        }

        private void Awake()
        {
            CreateLineMaterial();
        }

        private void CreateLineMaterial()
        {
            // Unity's built-in shader for drawing simple lines
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader);
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMaterial.SetInt("_ZWrite", 0);
        }

        private void OnRenderObject()
        {
            if (_lineMaterial == null)
                return;

            _lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            // Draw grid lines at each level
            if (_showAllLevels)
            {
                for (int level = 0; level < _height; level++)
                {
                    DrawGridLinesAtLevel(level);
                }
            }
            else
            {
                DrawGridLinesAtLevel(_highlightedLevel);
            }

            // Draw vertical lines connecting levels
            if (_showVerticalLines && _height > 1)
            {
                DrawVerticalLines();
            }

            // Draw boundary at bottom
            DrawBoundary();

            GL.PopMatrix();
        }

        private void DrawGridLinesAtLevel(int level)
        {
            GL.Begin(GL.LINES);

            // Use highlighted color for highlighted level, dimmer color for others
            bool isHighlighted = level == _highlightedLevel;
            Color levelColor = isHighlighted ? _highlightedLevelColor : _gridColor;

            // Fade out higher levels slightly to reduce visual clutter
            if (!isHighlighted && level > 0)
            {
                float fadeFactor = 1f - (level * 0.15f);
                levelColor.a *= Mathf.Max(0.2f, fadeFactor);
            }

            GL.Color(levelColor);

            float y = level * _cellSize + _lineYOffset;

            // Draw lines along X axis (going in Z direction)
            for (int x = 0; x <= _width; x++)
            {
                GL.Vertex3(x * _cellSize, y, 0);
                GL.Vertex3(x * _cellSize, y, _depth * _cellSize);
            }

            // Draw lines along Z axis (going in X direction)
            for (int z = 0; z <= _depth; z++)
            {
                GL.Vertex3(0, y, z * _cellSize);
                GL.Vertex3(_width * _cellSize, y, z * _cellSize);
            }

            GL.End();
        }

        private void DrawVerticalLines()
        {
            GL.Begin(GL.LINES);
            GL.Color(_verticalLineColor);

            float maxY = (_height - 1) * _cellSize;

            // Draw vertical lines at corners of each cell
            for (int x = 0; x <= _width; x++)
            {
                for (int z = 0; z <= _depth; z++)
                {
                    // Only draw at cell corners for less visual clutter
                    GL.Vertex3(x * _cellSize, _lineYOffset, z * _cellSize);
                    GL.Vertex3(x * _cellSize, maxY + _lineYOffset, z * _cellSize);
                }
            }

            GL.End();
        }

        private void DrawBoundary()
        {
            GL.Begin(GL.LINES);
            GL.Color(_boundaryColor);

            float w = _width * _cellSize;
            float d = _depth * _cellSize;

            // Draw boundary at each level
            for (int level = 0; level < _height; level++)
            {
                float y = level * _cellSize + _lineYOffset + 0.01f;

                // Only draw full boundary at bottom and top, lighter at middle levels
                if (level > 0 && level < _height - 1)
                {
                    GL.Color(new Color(_boundaryColor.r, _boundaryColor.g, _boundaryColor.b, _boundaryColor.a * 0.3f));
                }
                else
                {
                    GL.Color(_boundaryColor);
                }

                GL.Vertex3(0, y, 0);
                GL.Vertex3(w, y, 0);

                GL.Vertex3(w, y, 0);
                GL.Vertex3(w, y, d);

                GL.Vertex3(w, y, d);
                GL.Vertex3(0, y, d);

                GL.Vertex3(0, y, d);
                GL.Vertex3(0, y, 0);
            }

            GL.End();
        }

        /// <summary>
        /// Sets which level is highlighted (0-indexed).
        /// </summary>
        public void SetHighlightedLevel(int level)
        {
            _highlightedLevel = Mathf.Clamp(level, 0, _height - 1);
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                DestroyImmediate(_lineMaterial);
            }
        }
    }
}
