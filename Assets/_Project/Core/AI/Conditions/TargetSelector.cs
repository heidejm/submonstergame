namespace SubGame.Core.AI.Conditions
{
    /// <summary>
    /// Defines how targets are selected from a list of valid options.
    /// </summary>
    public enum TargetSelector
    {
        /// <summary>
        /// Select the closest target by distance.
        /// </summary>
        Nearest,

        /// <summary>
        /// Select the furthest target by distance.
        /// </summary>
        Furthest,

        /// <summary>
        /// Select the target with lowest current health.
        /// </summary>
        Weakest,

        /// <summary>
        /// Select the target with highest current health.
        /// </summary>
        Strongest,

        /// <summary>
        /// Select a random valid target.
        /// </summary>
        Random,

        /// <summary>
        /// Select the entity that last attacked this monster.
        /// </summary>
        LastAttacker
    }
}
