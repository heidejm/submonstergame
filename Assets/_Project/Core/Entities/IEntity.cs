using System;
using System.Collections.Generic;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Interface for all game entities (submarines, monsters, etc.).
    /// Defines the common properties and behaviors for tactical units.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Display name of the entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Current position on the grid.
        /// </summary>
        GridCoordinate Position { get; }

        /// <summary>
        /// Current health points.
        /// </summary>
        int Health { get; }

        /// <summary>
        /// Maximum health points.
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// How many grid cells this entity can move per turn.
        /// </summary>
        int MovementRange { get; }

        /// <summary>
        /// How far this entity can attack (in grid cells).
        /// </summary>
        int AttackRange { get; }

        /// <summary>
        /// Base damage dealt by this entity's attacks.
        /// </summary>
        int AttackDamage { get; }

        /// <summary>
        /// Whether this entity is still alive (Health > 0).
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// The type of entity (for distinguishing submarines from monsters, etc.).
        /// </summary>
        EntityType EntityType { get; }

        /// <summary>
        /// The size of this entity in grid cells.
        /// </summary>
        EntitySize Size { get; }

        /// <summary>
        /// Gets all grid coordinates currently occupied by this entity.
        /// </summary>
        /// <returns>All coordinates the entity occupies</returns>
        IEnumerable<GridCoordinate> GetOccupiedCells();

        /// <summary>
        /// Gets all grid coordinates this entity would occupy at a given position.
        /// </summary>
        /// <param name="position">The anchor position to check</param>
        /// <returns>All coordinates the entity would occupy</returns>
        IEnumerable<GridCoordinate> GetOccupiedCellsAt(GridCoordinate position);

        /// <summary>
        /// Applies damage to this entity, reducing health.
        /// Health cannot go below 0.
        /// </summary>
        /// <param name="amount">Amount of damage to apply (must be non-negative)</param>
        void TakeDamage(int amount);

        /// <summary>
        /// Heals this entity, increasing health.
        /// Health cannot exceed MaxHealth.
        /// </summary>
        /// <param name="amount">Amount to heal (must be non-negative)</param>
        void Heal(int amount);

        /// <summary>
        /// Moves this entity to a new position.
        /// Does not validate if the move is legal.
        /// </summary>
        /// <param name="newPosition">The new grid position</param>
        void SetPosition(GridCoordinate newPosition);

        /// <summary>
        /// Event fired when this entity takes damage.
        /// </summary>
        event Action<IEntity, int> OnDamageTaken;

        /// <summary>
        /// Event fired when this entity dies (health reaches 0).
        /// </summary>
        event Action<IEntity> OnDeath;

        /// <summary>
        /// Event fired when this entity moves.
        /// </summary>
        event Action<IEntity, GridCoordinate, GridCoordinate> OnMoved;
    }

    /// <summary>
    /// Types of entities in the game.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// Player-controlled submarine.
        /// </summary>
        Submarine,

        /// <summary>
        /// AI-controlled monster.
        /// </summary>
        Monster
    }
}
