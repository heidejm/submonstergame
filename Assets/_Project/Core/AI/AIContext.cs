using System.Collections.Generic;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Shared context for AI decision-making within a single turn.
    /// Allows conditions to pass data to actions (e.g., selected target).
    /// </summary>
    public class AIContext
    {
        /// <summary>
        /// The primary target selected during condition evaluation.
        /// </summary>
        public IEntity SelectedTarget { get; set; }

        /// <summary>
        /// The position to move to, if determined during evaluation.
        /// </summary>
        public GridCoordinate? TargetPosition { get; set; }

        /// <summary>
        /// Path to follow for movement.
        /// </summary>
        public IReadOnlyList<GridCoordinate> MovementPath { get; set; }

        /// <summary>
        /// All valid targets found during evaluation.
        /// </summary>
        public List<IEntity> ValidTargets { get; set; } = new List<IEntity>();

        /// <summary>
        /// Generic data storage for custom conditions/actions.
        /// </summary>
        public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Resets the context for a new evaluation.
        /// </summary>
        public void Reset()
        {
            SelectedTarget = null;
            TargetPosition = null;
            MovementPath = null;
            ValidTargets.Clear();
            Data.Clear();
        }
    }
}
