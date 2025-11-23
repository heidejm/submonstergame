using System.Linq;
using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI.Conditions
{
    /// <summary>
    /// Condition that checks if any valid target is within attack range.
    /// Sets the nearest target in context if found.
    /// </summary>
    public class TargetInAttackRangeCondition : IAICondition
    {
        private readonly TargetSelector _selector;

        public string Description => "Target in attack range";

        /// <summary>
        /// Creates condition with specified target selection strategy.
        /// </summary>
        /// <param name="selector">How to select from valid targets</param>
        public TargetInAttackRangeCondition(TargetSelector selector = TargetSelector.Nearest)
        {
            _selector = selector;
        }

        public bool Evaluate(IGameState state, IEntity monster, AIContext context)
        {
            context.ValidTargets.Clear();

            // Find all submarines within attack range
            foreach (var submarine in state.GetSubmarines())
            {
                if (!submarine.IsAlive)
                    continue;

                int distance = state.GetDistanceBetweenEntities(monster, submarine);
                if (distance <= monster.AttackRange)
                {
                    context.ValidTargets.Add(submarine);
                }
            }

            if (context.ValidTargets.Count == 0)
                return false;

            // Select target based on strategy
            context.SelectedTarget = SelectTarget(context.ValidTargets, monster, state);
            return context.SelectedTarget != null;
        }

        private IEntity SelectTarget(System.Collections.Generic.List<IEntity> targets, IEntity monster, IGameState state)
        {
            if (targets.Count == 0)
                return null;

            switch (_selector)
            {
                case TargetSelector.Nearest:
                    return targets.OrderBy(t => state.GetDistanceBetweenEntities(monster, t)).First();

                case TargetSelector.Weakest:
                    return targets.OrderBy(t => t.Health).First();

                case TargetSelector.Strongest:
                    return targets.OrderByDescending(t => t.Health).First();

                case TargetSelector.Random:
                    return targets[new System.Random().Next(targets.Count)];

                default:
                    return targets[0];
            }
        }
    }
}
