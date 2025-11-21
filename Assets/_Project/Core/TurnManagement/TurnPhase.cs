namespace SubGame.Core.TurnManagement
{
    /// <summary>
    /// Defines the different phases within a turn.
    /// </summary>
    public enum TurnPhase
    {
        /// <summary>
        /// Start of the turn, before any actions.
        /// Use for start-of-turn effects, status updates, etc.
        /// </summary>
        TurnStart,

        /// <summary>
        /// Player's submarine movement and action phase.
        /// </summary>
        PlayerAction,

        /// <summary>
        /// Enemy/monster movement and action phase.
        /// </summary>
        EnemyAction,

        /// <summary>
        /// End of the turn, after all actions.
        /// Use for end-of-turn effects, cleanup, etc.
        /// </summary>
        TurnEnd
    }
}
