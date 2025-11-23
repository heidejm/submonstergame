using System.Linq;
using SubGame.Core.Commands;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Core.AI.Conditions
{
    /// <summary>
    /// Condition that checks if the monster can move toward any target.
    /// Sets the selected target and movement path in context.
    /// </summary>
    public class CanReachTargetCondition : IAICondition
    {
        private readonly TargetSelector _selector;

        public string Description => "Can reach target";

        /// <summary>
        /// Creates condition with specified target selection strategy.
        /// </summary>
        /// <param name="selector">How to select from valid targets</param>
        public CanReachTargetCondition(TargetSelector selector = TargetSelector.Nearest)
        {
            _selector = selector;
        }

        public bool Evaluate(IGameState state, IEntity monster, AIContext context)
        {
            var reachablePositions = state.GetReachablePositions(monster);
            if (reachablePositions.Count == 0)
                return false;

            // Find all submarines
            var submarines = state.GetSubmarines().Where(s => s.IsAlive).ToList();
            if (submarines.Count == 0)
                return false;

            // Find the best target and position to move to
            IEntity bestTarget = null;
            GridCoordinate? bestPosition = null;
            int bestNewDistance = int.MaxValue;

            foreach (var submarine in submarines)
            {
                // For each reachable position, calculate distance to this submarine
                foreach (var position in reachablePositions)
                {
                    // Calculate distance from potential new position to target
                    // We need to consider entity size, so check distance from new position cells
                    int distanceFromNewPos = CalculateDistanceFromPosition(position, monster.Size, submarine, state);

                    if (distanceFromNewPos < bestNewDistance)
                    {
                        bestNewDistance = distanceFromNewPos;
                        bestTarget = submarine;
                        bestPosition = position;
                    }
                }
            }

            if (bestTarget == null || !bestPosition.HasValue)
                return false;

            // Verify we're actually getting closer
            int currentDistance = state.GetDistanceBetweenEntities(monster, bestTarget);
            if (bestNewDistance >= currentDistance)
                return false; // Can't get closer, don't move

            context.SelectedTarget = bestTarget;
            context.TargetPosition = bestPosition;

            return true;
        }

        private int CalculateDistanceFromPosition(GridCoordinate position, EntitySize size, IEntity target, IGameState state)
        {
            int minDistance = int.MaxValue;

            // Calculate distance from all cells the monster would occupy at the new position
            foreach (var monsterCell in size.GetOccupiedCells(position))
            {
                foreach (var targetCell in target.GetOccupiedCells())
                {
                    int distance = GridCoordinate.Distance(monsterCell, targetCell);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
        }
    }
}
