using UnityEngine;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Renders grid lines at runtime using GL.Lines.
    /// Visible in Game view, not just Scene view.
    /// </summary>
    public class RuntimeGridRenderer : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _width = 10;
        [SerializeField] private int _height = 1;
        [SerializeField] private int _depth = 10;
        [SerializeField] private float _cellSize = 1f;

        [Header("Visual Settings")]
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 1f, 0.5f);
        [SerializeField] private Color _boundaryColor = new Color(1f, 1f, 0f, 0.8f);
        [SerializeField] private float _lineYOffset = 0.02f;

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

            // Draw grid lines
            DrawGridLines();

            // Draw boundary
            DrawBoundary();

            GL.PopMatrix();
        }

        private void DrawGridLines()
        {
            GL.Begin(GL.LINES);
            GL.Color(_gridColor);

            float y = _lineYOffset;

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

        private void DrawBoundary()
        {
            GL.Begin(GL.LINES);
            GL.Color(_boundaryColor);

            float y = _lineYOffset + 0.01f;
            float w = _width * _cellSize;
            float d = _depth * _cellSize;

            // Bottom boundary
            GL.Vertex3(0, y, 0);
            GL.Vertex3(w, y, 0);

            GL.Vertex3(w, y, 0);
            GL.Vertex3(w, y, d);

            GL.Vertex3(w, y, d);
            GL.Vertex3(0, y, d);

            GL.Vertex3(0, y, d);
            GL.Vertex3(0, y, 0);

            GL.End();
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
