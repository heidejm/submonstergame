using System.Collections.Generic;
using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Represents a single AI behavior card.
    /// Contains conditional branches evaluated in order, with a fallback action.
    /// </summary>
    public class BehaviorCard
    {
        /// <summary>
        /// Display name of this card.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Optional description of what this card does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Conditional branches evaluated in order.
        /// First branch whose condition is true will have its actions executed.
        /// </summary>
        public IReadOnlyList<ConditionalBranch> Branches { get; }

        /// <summary>
        /// Fallback actions if no branch conditions are met.
        /// </summary>
        public IReadOnlyList<IAIAction> FallbackActions { get; }

        /// <summary>
        /// Creates a new behavior card.
        /// </summary>
        /// <param name="name">Card name</param>
        /// <param name="branches">Conditional branches</param>
        /// <param name="fallbackActions">Fallback actions if no conditions met</param>
        /// <param name="description">Optional description</param>
        public BehaviorCard(
            string name,
            IReadOnlyList<ConditionalBranch> branches,
            IReadOnlyList<IAIAction> fallbackActions = null,
            string description = null)
        {
            Name = name;
            Description = description;
            Branches = branches ?? new List<ConditionalBranch>();
            FallbackActions = fallbackActions ?? new List<IAIAction>();
        }

        /// <summary>
        /// Evaluates and executes this card's behavior.
        /// </summary>
        /// <param name="state">Current game state</param>
        /// <param name="monster">The monster executing this card</param>
        /// <param name="context">Shared AI context</param>
        /// <returns>True if any actions were executed</returns>
        public bool Execute(IGameState state, IEntity monster, AIContext context)
        {
            // Try each branch in order
            foreach (var branch in Branches)
            {
                if (branch.Condition.Evaluate(state, monster, context))
                {
                    ExecuteActions(branch.Actions, state, monster, context);
                    return true;
                }
            }

            // No branch matched, execute fallback
            if (FallbackActions.Count > 0)
            {
                ExecuteActions(FallbackActions, state, monster, context);
                return true;
            }

            return false;
        }

        private void ExecuteActions(IReadOnlyList<IAIAction> actions, IGameState state, IEntity monster, AIContext context)
        {
            foreach (var action in actions)
            {
                action.Execute(state, monster, context);
            }
        }
    }
}
