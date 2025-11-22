using SubGame.Core.Entities;

namespace SubGame.Core.Config
{
    /// <summary>
    /// Base configuration for all entities.
    /// Defines the stats and properties that can be configured.
    /// </summary>
    public abstract class EntityConfig
    {
        /// <summary>
        /// Display name for the entity.
        /// </summary>
        public string Name { get; set; } = "Entity";

        /// <summary>
        /// Maximum health points.
        /// </summary>
        public int MaxHealth { get; set; } = 100;

        /// <summary>
        /// Movement range in grid cells per turn.
        /// </summary>
        public int MovementRange { get; set; } = 3;

        /// <summary>
        /// Attack range in grid cells.
        /// </summary>
        public int AttackRange { get; set; } = 2;

        /// <summary>
        /// Base damage dealt by attacks.
        /// </summary>
        public int AttackDamage { get; set; } = 20;

        /// <summary>
        /// Size of the entity in grid cells (width, height, depth).
        /// </summary>
        public EntitySize Size { get; set; } = EntitySize.One;

        /// <summary>
        /// Validates the configuration values.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   MaxHealth > 0 &&
                   MovementRange >= 0 &&
                   AttackRange >= 0 &&
                   AttackDamage >= 0 &&
                   Size.TotalCells > 0;
        }
    }

    /// <summary>
    /// Configuration for submarine entities.
    /// </summary>
    public class SubmarineConfig : EntityConfig
    {
        /// <summary>
        /// Creates a default submarine configuration.
        /// </summary>
        public SubmarineConfig()
        {
            Name = "Submarine";
            MaxHealth = 100;
            MovementRange = 4;
            AttackRange = 2;
            AttackDamage = 25;
        }

        /// <summary>
        /// Creates a submarine configuration with a specific name.
        /// </summary>
        /// <param name="name">Display name for the submarine</param>
        public SubmarineConfig(string name) : this()
        {
            Name = name;
        }
    }

    /// <summary>
    /// Configuration for monster entities.
    /// </summary>
    public class MonsterConfig : EntityConfig
    {
        /// <summary>
        /// Creates a default monster configuration.
        /// Monsters are typically tougher but slower than submarines.
        /// </summary>
        public MonsterConfig()
        {
            Name = "Sea Monster";
            MaxHealth = 200;
            MovementRange = 4;
            AttackRange = 1;
            AttackDamage = 40;
            Size = new EntitySize(2, 2, 2); // Monsters are larger (2x2x2)
        }

        /// <summary>
        /// Creates a monster configuration with a specific name.
        /// </summary>
        /// <param name="name">Display name for the monster</param>
        public MonsterConfig(string name) : this()
        {
            Name = name;
        }
    }
}
