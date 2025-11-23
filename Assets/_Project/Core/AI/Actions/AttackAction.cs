using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI.Actions
{
    /// <summary>
    /// Action that attacks the target selected in context.
    /// </summary>
    public class AttackAction : IAIAction
    {
        public string Description => "Attack target";

        public void Execute(IGameState state, IEntity monster, AIContext context)
        {
            if (context.SelectedTarget == null)
                return;

            if (!context.SelectedTarget.IsAlive)
                return;

            // Apply damage directly through game state
            state.ApplyDamage(context.SelectedTarget, monster.AttackDamage);
        }
    }
}
