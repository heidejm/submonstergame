using System;
using SubGame.Core.Config;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Represents a player-controlled submarine entity.
    /// Submarines are typically faster but less durable than monsters.
    /// </summary>
    public class Submarine : Entity
    {
        /// <inheritdoc/>
        public override EntityType EntityType => EntityType.Submarine;

        /// <summary>
        /// Creates a new submarine at the specified position with default configuration.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        public Submarine(GridCoordinate position)
            : this(Guid.NewGuid(), position, new SubmarineConfig())
        {
        }

        /// <summary>
        /// Creates a new submarine at the specified position with a custom name.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="name">Display name for this submarine</param>
        public Submarine(GridCoordinate position, string name)
            : this(Guid.NewGuid(), position, new SubmarineConfig(name))
        {
        }

        /// <summary>
        /// Creates a new submarine at the specified position with custom configuration.
        /// </summary>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="config">Configuration defining submarine stats</param>
        public Submarine(GridCoordinate position, SubmarineConfig config)
            : this(Guid.NewGuid(), position, config)
        {
        }

        /// <summary>
        /// Creates a new submarine with a specific ID (useful for deserialization/networking).
        /// </summary>
        /// <param name="id">Unique identifier for this submarine</param>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="config">Configuration defining submarine stats</param>
        /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
        /// <exception cref="ArgumentException">Thrown if config is invalid</exception>
        public Submarine(Guid id, GridCoordinate position, SubmarineConfig config)
            : base(
                id,
                config?.Name ?? throw new ArgumentNullException(nameof(config)),
                position,
                config.MaxHealth,
                config.MovementRange,
                config.AttackRange,
                config.AttackDamage,
                config.Size)
        {
            if (!config.IsValid())
            {
                throw new ArgumentException("Configuration is invalid", nameof(config));
            }
        }
    }
}
