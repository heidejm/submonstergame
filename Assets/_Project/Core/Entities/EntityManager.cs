using System;
using System.Collections.Generic;
using System.Linq;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Manages all entities in the game.
    /// Provides methods for adding, removing, and querying entities.
    /// </summary>
    public class EntityManager
    {
        private readonly Dictionary<Guid, IEntity> _entities = new Dictionary<Guid, IEntity>();
        private readonly IGridSystem _gridSystem;

        /// <summary>
        /// Event fired when an entity is added to the manager.
        /// </summary>
        public event Action<IEntity> OnEntityAdded;

        /// <summary>
        /// Event fired when an entity is removed from the manager.
        /// </summary>
        public event Action<IEntity> OnEntityRemoved;

        /// <summary>
        /// Gets the number of entities currently managed.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Creates a new EntityManager associated with a grid system.
        /// </summary>
        /// <param name="gridSystem">The grid system for position validation</param>
        /// <exception cref="ArgumentNullException">Thrown if gridSystem is null</exception>
        public EntityManager(IGridSystem gridSystem)
        {
            _gridSystem = gridSystem ?? throw new ArgumentNullException(nameof(gridSystem));
        }

        /// <summary>
        /// Adds an entity to the manager.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
        /// <exception cref="ArgumentException">Thrown if entity with same ID already exists</exception>
        /// <exception cref="InvalidOperationException">Thrown if entity position is invalid or occupied</exception>
        public void AddEntity(IEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (_entities.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"Entity with ID {entity.Id} already exists", nameof(entity));
            }

            if (!_gridSystem.IsValidCoordinate(entity.Position))
            {
                throw new InvalidOperationException(
                    $"Entity position {entity.Position} is outside grid bounds");
            }

            if (_gridSystem.IsOccupied(entity.Position))
            {
                throw new InvalidOperationException(
                    $"Position {entity.Position} is already occupied");
            }

            _entities.Add(entity.Id, entity);
            _gridSystem.SetOccupied(entity.Position);

            // Subscribe to entity events
            entity.OnDeath += HandleEntityDeath;
            entity.OnMoved += HandleEntityMoved;

            OnEntityAdded?.Invoke(entity);
        }

        /// <summary>
        /// Removes an entity from the manager.
        /// </summary>
        /// <param name="entityId">The ID of the entity to remove</param>
        /// <returns>True if entity was removed, false if not found</returns>
        public bool RemoveEntity(Guid entityId)
        {
            if (!_entities.TryGetValue(entityId, out var entity))
            {
                return false;
            }

            // Unsubscribe from entity events
            entity.OnDeath -= HandleEntityDeath;
            entity.OnMoved -= HandleEntityMoved;

            _gridSystem.ClearOccupied(entity.Position);
            _entities.Remove(entityId);

            OnEntityRemoved?.Invoke(entity);
            return true;
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="entityId">The entity ID to look up</param>
        /// <returns>The entity, or null if not found</returns>
        public IEntity GetEntity(Guid entityId)
        {
            _entities.TryGetValue(entityId, out var entity);
            return entity;
        }

        /// <summary>
        /// Gets the entity at a specific grid position.
        /// </summary>
        /// <param name="position">The grid position to check</param>
        /// <returns>The entity at that position, or null if empty</returns>
        public IEntity GetEntityAtPosition(GridCoordinate position)
        {
            return _entities.Values.FirstOrDefault(e => e.Position.Equals(position));
        }

        /// <summary>
        /// Gets all entities of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entities to retrieve</param>
        /// <returns>Collection of entities of the specified type</returns>
        public IEnumerable<IEntity> GetEntitiesByType(EntityType entityType)
        {
            return _entities.Values.Where(e => e.EntityType == entityType);
        }

        /// <summary>
        /// Gets all submarines.
        /// </summary>
        /// <returns>Collection of all submarine entities</returns>
        public IEnumerable<IEntity> GetSubmarines()
        {
            return GetEntitiesByType(EntityType.Submarine);
        }

        /// <summary>
        /// Gets all monsters.
        /// </summary>
        /// <returns>Collection of all monster entities</returns>
        public IEnumerable<IEntity> GetMonsters()
        {
            return GetEntitiesByType(EntityType.Monster);
        }

        /// <summary>
        /// Gets all living entities.
        /// </summary>
        /// <returns>Collection of all entities that are still alive</returns>
        public IEnumerable<IEntity> GetLivingEntities()
        {
            return _entities.Values.Where(e => e.IsAlive);
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>Collection of all entities</returns>
        public IEnumerable<IEntity> GetAllEntities()
        {
            return _entities.Values;
        }

        /// <summary>
        /// Gets all entities within range of a position.
        /// </summary>
        /// <param name="center">The center position</param>
        /// <param name="range">Maximum distance (Manhattan) to include</param>
        /// <returns>Collection of entities within range</returns>
        public IEnumerable<IEntity> GetEntitiesInRange(GridCoordinate center, int range)
        {
            return _entities.Values.Where(e =>
                GridCoordinate.Distance(center, e.Position) <= range);
        }

        /// <summary>
        /// Checks if a position is valid for movement (in bounds and unoccupied).
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is valid for movement</returns>
        public bool IsValidMovePosition(GridCoordinate position)
        {
            return _gridSystem.IsValidCoordinate(position) && !_gridSystem.IsOccupied(position);
        }

        /// <summary>
        /// Clears all entities from the manager.
        /// </summary>
        public void Clear()
        {
            var entitiesToRemove = _entities.Keys.ToList();
            foreach (var entityId in entitiesToRemove)
            {
                RemoveEntity(entityId);
            }
        }

        /// <summary>
        /// Handles entity death by removing it from management.
        /// </summary>
        private void HandleEntityDeath(IEntity entity)
        {
            // Clear the occupied position but keep entity in collection
            // This allows the game to reference dead entities if needed
            _gridSystem.ClearOccupied(entity.Position);
        }

        /// <summary>
        /// Handles entity movement by updating grid occupancy.
        /// </summary>
        private void HandleEntityMoved(IEntity entity, GridCoordinate oldPosition, GridCoordinate newPosition)
        {
            _gridSystem.ClearOccupied(oldPosition);
            _gridSystem.SetOccupied(newPosition);
        }
    }
}
