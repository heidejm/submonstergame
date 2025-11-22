using System;
using UnityEngine;
using SubGame.Core.Grid;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Base class for visual representation of game entities.
    /// Handles position updates, animations, and health display.
    /// </summary>
    public abstract class EntityView : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] protected float _moveSpeed = 5f;
        [SerializeField] protected float _cellSize = 1f;

        [Header("References")]
        [SerializeField] protected Transform _modelTransform;

        protected Guid _entityId;
        protected GridCoordinate _currentGridPosition;
        protected Vector3 _targetWorldPosition;
        protected bool _isMoving;

        /// <summary>
        /// Gets the entity ID this view represents.
        /// </summary>
        public Guid EntityId => _entityId;

        /// <summary>
        /// Gets whether this view is currently animating movement.
        /// </summary>
        public bool IsMoving => _isMoving;

        /// <summary>
        /// Event fired when movement animation completes.
        /// </summary>
        public event Action<EntityView> OnMoveComplete;

        /// <summary>
        /// Initializes the view with an entity ID.
        /// </summary>
        /// <param name="entityId">The entity this view represents</param>
        /// <param name="startPosition">Initial grid position</param>
        /// <param name="cellSize">Size of each grid cell in world units</param>
        public virtual void Initialize(Guid entityId, GridCoordinate startPosition, float cellSize)
        {
            _entityId = entityId;
            _cellSize = cellSize;
            _currentGridPosition = startPosition;
            _targetWorldPosition = GridToWorldPosition(startPosition);
            transform.position = _targetWorldPosition;
        }

        /// <summary>
        /// Updates the entity's position with smooth animation.
        /// </summary>
        /// <param name="newPosition">New grid position</param>
        public virtual void UpdatePosition(GridCoordinate newPosition)
        {
            _currentGridPosition = newPosition;
            _targetWorldPosition = GridToWorldPosition(newPosition);
            _isMoving = true;
        }

        /// <summary>
        /// Instantly teleports to a position without animation.
        /// </summary>
        /// <param name="position">Grid position to teleport to</param>
        public virtual void TeleportTo(GridCoordinate position)
        {
            _currentGridPosition = position;
            _targetWorldPosition = GridToWorldPosition(position);
            transform.position = _targetWorldPosition;
            _isMoving = false;
        }

        /// <summary>
        /// Updates the health display.
        /// </summary>
        /// <param name="currentHealth">Current health value</param>
        /// <param name="maxHealth">Maximum health value</param>
        public abstract void UpdateHealth(int currentHealth, int maxHealth);

        /// <summary>
        /// Called when this entity takes damage.
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        public virtual void OnDamageTaken(int damage)
        {
            // Override in subclasses for damage effects
        }

        /// <summary>
        /// Called when this entity dies.
        /// </summary>
        public virtual void OnDeath()
        {
            // Override in subclasses for death effects
        }

        /// <summary>
        /// Sets visual selection state (e.g., when it's this entity's turn).
        /// </summary>
        /// <param name="selected">Whether this entity is selected/active</param>
        public virtual void SetSelected(bool selected)
        {
            // Override in subclasses for selection effects
        }

        /// <summary>
        /// Converts a grid coordinate to world position.
        /// </summary>
        protected Vector3 GridToWorldPosition(GridCoordinate coord)
        {
            return new Vector3(
                coord.X * _cellSize + _cellSize / 2f,
                coord.Y * _cellSize + _cellSize / 2f,
                coord.Z * _cellSize + _cellSize / 2f
            );
        }

        /// <summary>
        /// Unity Update - handles smooth movement animation.
        /// </summary>
        protected virtual void Update()
        {
            if (_isMoving)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _targetWorldPosition,
                    _moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, _targetWorldPosition) < 0.01f)
                {
                    transform.position = _targetWorldPosition;
                    _isMoving = false;
                    OnMoveComplete?.Invoke(this);
                }
            }
        }
    }
}
