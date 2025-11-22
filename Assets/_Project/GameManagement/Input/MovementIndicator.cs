using UnityEngine;
using SubGame.Core.Grid;

namespace SubGame.GameManagement.Input
{
    /// <summary>
    /// Component attached to movement indicator nodes.
    /// Stores the target coordinate for movement and enables click detection.
    /// </summary>
    public class MovementIndicator : MonoBehaviour
    {
        /// <summary>
        /// The grid coordinate this indicator represents.
        /// </summary>
        public GridCoordinate TargetCoordinate { get; private set; }

        /// <summary>
        /// Whether this is an attack indicator (vs movement indicator).
        /// </summary>
        public bool IsAttackIndicator { get; private set; }

        /// <summary>
        /// Initializes the movement indicator.
        /// </summary>
        /// <param name="coordinate">Target grid coordinate</param>
        /// <param name="isAttack">True if this is an attack indicator</param>
        public void Initialize(GridCoordinate coordinate, bool isAttack = false)
        {
            TargetCoordinate = coordinate;
            IsAttackIndicator = isAttack;
        }
    }
}
