using System;
using SubGame.Core.Config;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Represents an AI-controlled monster entity.
    /// Monsters are typically tougher but slower than submarines.
    /// </summary>
    public class Monster : Entity
    {
        /// <inheritdoc/>
        public override EntityType EntityType => EntityType.Monster;

        /// <summary>
        /// Creates a new monster at the specified position with default configuration.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        public Monster(GridCoordinate position)
            : this(Guid.NewGuid(), position, new MonsterConfig())
        {
        }

        /// <summary>
        /// Creates a new monster at the specified position with a custom name.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="name">Display name for this monster</param>
        public Monster(GridCoordinate position, string name)
            : this(Guid.NewGuid(), position, new MonsterConfig(name))
        {
        }

        /// <summary>
        /// Creates a new monster at the specified position with custom configuration.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="config">Configuration defining monster stats</param>
        public Monster(GridCoordinate position, MonsterConfig config)
            : this(Guid.NewGuid(), position, config)
        {
        }

        /// <summary>
        /// Creates a new monster with a specific ID (useful for deserialization/networking).
        /// </summary>
        /// <param name="id">Unique identifier for this monster</param>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="config">Configuration defining monster stats</param>
        /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
        /// <exception cref="ArgumentException">Thrown if config is invalid</exception>
        public Monster(Guid id, GridCoordinate position, MonsterConfig config)
            : base(
                id,
                config?.Name ?? throw new ArgumentNullException(nameof(config)),
                position,
                config.MaxHealth,
                config.MovementRange,
                config.AttackRange,
                config.AttackDamage)
        {
            if (!config.IsValid())
            {
                throw new ArgumentException("Configuration is invalid", nameof(config));
            }
        }
    }
}
