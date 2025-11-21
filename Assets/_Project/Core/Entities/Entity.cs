using System;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Abstract base class for all game entities.
    /// Provides common implementation for IEntity interface.
    /// </summary>
    public abstract class Entity : IEntity
    {
        private int _health;
        private GridCoordinate _position;

        /// <inheritdoc/>
        public Guid Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public GridCoordinate Position => _position;

        /// <inheritdoc/>
        public int Health => _health;

        /// <inheritdoc/>
        public int MaxHealth { get; }

        /// <inheritdoc/>
        public int MovementRange { get; }

        /// <inheritdoc/>
        public int AttackRange { get; }

        /// <inheritdoc/>
        public int AttackDamage { get; }

        /// <inheritdoc/>
        public bool IsAlive => _health > 0;

        /// <inheritdoc/>
        public abstract EntityType EntityType { get; }

        /// <inheritdoc/>
        public event Action<IEntity, int> OnDamageTaken;

        /// <inheritdoc/>
        public event Action<IEntity> OnDeath;

        /// <inheritdoc/>
        public event Action<IEntity, GridCoordinate, GridCoordinate> OnMoved;

        /// <summary>
        /// Creates a new entity with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier (use Guid.NewGuid() if not specified)</param>
        /// <param name="name">Display name</param>
        /// <param name="position">Starting position on the grid</param>
        /// <param name="maxHealth">Maximum health points</param>
        /// <param name="movementRange">Movement range in grid cells</param>
        /// <param name="attackRange">Attack range in grid cells</param>
        /// <param name="attackDamage">Base attack damage</param>
        protected Entity(
            Guid id,
            string name,
            GridCoordinate position,
            int maxHealth,
            int movementRange,
            int attackRange,
            int attackDamage)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            if (maxHealth <= 0)
                throw new ArgumentException("MaxHealth must be positive", nameof(maxHealth));
            if (movementRange < 0)
                throw new ArgumentException("MovementRange cannot be negative", nameof(movementRange));
            if (attackRange < 0)
                throw new ArgumentException("AttackRange cannot be negative", nameof(attackRange));
            if (attackDamage < 0)
                throw new ArgumentException("AttackDamage cannot be negative", nameof(attackDamage));

            Id = id;
            Name = name;
            _position = position;
            MaxHealth = maxHealth;
            _health = maxHealth;
            MovementRange = movementRange;
            AttackRange = attackRange;
            AttackDamage = attackDamage;
        }

        /// <inheritdoc/>
        public void TakeDamage(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Damage amount cannot be negative", nameof(amount));

            if (!IsAlive)
                return;

            int previousHealth = _health;
            _health = Math.Max(0, _health - amount);

            OnDamageTaken?.Invoke(this, amount);

            if (previousHealth > 0 && _health <= 0)
            {
                OnDeath?.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Heal amount cannot be negative", nameof(amount));

            if (!IsAlive)
                return;

            _health = Math.Min(MaxHealth, _health + amount);
        }

        /// <inheritdoc/>
        public void SetPosition(GridCoordinate newPosition)
        {
            var oldPosition = _position;
            _position = newPosition;
            OnMoved?.Invoke(this, oldPosition, newPosition);
        }

        /// <summary>
        /// Returns a string representation of the entity.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({EntityType}) at {Position} - HP: {Health}/{MaxHealth}";
        }
    }
}
