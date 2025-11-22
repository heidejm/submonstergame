using System;
using System.Collections.Generic;
using UnityEngine;
using SubGame.Core.Grid;
using SubGame.Core.Entities;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Base class for visual representation of game entities.
    /// Handles position updates, animations, and health display.
    /// Supports path-based movement through multiple waypoints.
    /// </summary>
    public abstract class EntityView : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] protected float _moveSpeed = 5f;
        [SerializeField] protected float _cellSize = 1f;
        [SerializeField] protected bool _rotateTowardsMoveDirection = true;
        [SerializeField] protected float _rotationSpeed = 10f;

        [Header("References")]
        [SerializeField] protected Transform _modelTransform;

        protected Guid _entityId;
        protected GridCoordinate _currentGridPosition;
        protected Vector3 _targetWorldPosition;
        protected bool _isMoving;

        // Path-based movement
        protected Queue<GridCoordinate> _movementPath = new Queue<GridCoordinate>();
        protected GridCoordinate? _currentWaypoint;

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
        /// <param name="entitySize">Size of the entity in grid cells</param>
        public virtual void Initialize(Guid entityId, GridCoordinate startPosition, float cellSize, EntitySize entitySize = default)
        {
            _entityId = entityId;
            _cellSize = cellSize;
            _currentGridPosition = startPosition;

            // Store entity size
            _entitySize = entitySize.TotalCells > 0 ? entitySize : EntitySize.One;

            // Scale the entity based on its size
            if (_entitySize.TotalCells > 1)
            {
                transform.localScale = new Vector3(
                    _entitySize.Width * cellSize * 0.9f,
                    _entitySize.Height * cellSize * 0.9f,
                    _entitySize.Depth * cellSize * 0.9f
                );
            }

            _targetWorldPosition = GridToWorldPositionForSize(startPosition, _entitySize);
            transform.position = _targetWorldPosition;
        }

        /// <summary>
        /// The size of this entity in grid cells (set during Initialize).
        /// </summary>
        protected EntitySize _entitySize = EntitySize.One;

        /// <summary>
        /// Converts a grid coordinate to world position, accounting for entity size.
        /// Multi-tile entities are centered on their occupied space.
        /// </summary>
        protected Vector3 GridToWorldPositionForSize(GridCoordinate coord, EntitySize size)
        {
            var offset = size.GetCenterOffset();
            return new Vector3(
                coord.X * _cellSize + _cellSize / 2f + offset.x * _cellSize,
                coord.Y * _cellSize + _cellSize / 2f + offset.y * _cellSize,
                coord.Z * _cellSize + _cellSize / 2f + offset.z * _cellSize
            );
        }

        /// <summary>
        /// Updates the entity's position with smooth animation.
        /// For single-step movement (legacy compatibility).
        /// </summary>
        /// <param name="newPosition">New grid position</param>
        public virtual void UpdatePosition(GridCoordinate newPosition)
        {
            // Clear any existing path and move directly
            _movementPath.Clear();
            _currentWaypoint = newPosition;
            _currentGridPosition = newPosition;
            _targetWorldPosition = GridToWorldPositionForSize(newPosition, _entitySize);
            _isMoving = true;
        }

        /// <summary>
        /// Moves the entity along a path of waypoints.
        /// The entity will move through each waypoint in sequence.
        /// </summary>
        /// <param name="path">List of grid coordinates to move through (including start position)</param>
        public virtual void MoveAlongPath(IReadOnlyList<GridCoordinate> path)
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            // Clear existing path
            _movementPath.Clear();

            // Skip the first position (current position) and queue the rest
            for (int i = 1; i < path.Count; i++)
            {
                _movementPath.Enqueue(path[i]);
            }

            // Start moving to first waypoint
            MoveToNextWaypoint();
        }

        /// <summary>
        /// Moves to the next waypoint in the path.
        /// </summary>
        protected virtual void MoveToNextWaypoint()
        {
            if (_movementPath.Count > 0)
            {
                _currentWaypoint = _movementPath.Dequeue();
                _currentGridPosition = _currentWaypoint.Value;
                _targetWorldPosition = GridToWorldPositionForSize(_currentWaypoint.Value, _entitySize);
                _isMoving = true;
            }
            else
            {
                _currentWaypoint = null;
                _isMoving = false;
                OnMoveComplete?.Invoke(this);
            }
        }

        /// <summary>
        /// Instantly teleports to a position without animation.
        /// </summary>
        /// <param name="position">Grid position to teleport to</param>
        public virtual void TeleportTo(GridCoordinate position)
        {
            _movementPath.Clear();
            _currentWaypoint = null;
            _currentGridPosition = position;
            _targetWorldPosition = GridToWorldPositionForSize(position, _entitySize);
            transform.position = _targetWorldPosition;
            _isMoving = false;
        }

        /// <summary>
        /// Gets the number of waypoints remaining in the current path.
        /// </summary>
        public int RemainingWaypoints => _movementPath.Count + (_isMoving ? 1 : 0);

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
        /// Unity Update - handles smooth movement animation along path.
        /// </summary>
        protected virtual void Update()
        {
            if (_isMoving)
            {
                // Move towards current target
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _targetWorldPosition,
                    _moveSpeed * Time.deltaTime
                );

                // Rotate towards movement direction
                if (_rotateTowardsMoveDirection)
                {
                    Vector3 moveDirection = (_targetWorldPosition - transform.position).normalized;
                    if (moveDirection.sqrMagnitude > 0.001f)
                    {
                        // Only rotate on Y axis (horizontal plane)
                        moveDirection.y = 0;
                        if (moveDirection.sqrMagnitude > 0.001f)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                            transform.rotation = Quaternion.Slerp(
                                transform.rotation,
                                targetRotation,
                                _rotationSpeed * Time.deltaTime
                            );
                        }
                    }
                }

                // Check if we've reached the current waypoint
                if (Vector3.Distance(transform.position, _targetWorldPosition) < 0.01f)
                {
                    transform.position = _targetWorldPosition;

                    // Move to next waypoint or complete
                    MoveToNextWaypoint();
                }
            }
        }
    }
}
