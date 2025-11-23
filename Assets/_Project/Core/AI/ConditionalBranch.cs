using System.Collections.Generic;

namespace SubGame.Core.AI
{
    /// <summary>
    /// A condition-action pair used in behavior cards.
    /// If the condition evaluates to true, the actions are executed in sequence.
    /// </summary>
    public class ConditionalBranch
    {
        /// <summary>
        /// The condition to evaluate.
        /// </summary>
        public IAICondition Condition { get; }

        /// <summary>
        /// Actions to execute if the condition is true.
        /// </summary>
        public IReadOnlyList<IAIAction> Actions { get; }

        /// <summary>
        /// Creates a new conditional branch.
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="actions">Actions to execute if condition is met</param>
        public ConditionalBranch(IAICondition condition, params IAIAction[] actions)
        {
            Condition = condition;
            Actions = actions;
        }

        /// <summary>
        /// Creates a new conditional branch with a list of actions.
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="actions">Actions to execute if condition is met</param>
        public ConditionalBranch(IAICondition condition, IReadOnlyList<IAIAction> actions)
        {
            Condition = condition;
            Actions = actions;
        }
    }
}
