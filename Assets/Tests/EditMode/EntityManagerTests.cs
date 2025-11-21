using System;
using System.Linq;
using NUnit.Framework;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for EntityManager class.
    /// </summary>
    [TestFixture]
    public class EntityManagerTests
    {
        private IGridSystem _gridSystem;
        private EntityManager _entityManager;

        [SetUp]
        public void Setup()
        {
            _gridSystem = new GridSystem(10, 10, 10);
            _entityManager = new EntityManager(_gridSystem);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithNullGridSystem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityManager(null));
        }

        [Test]
        public void Constructor_CreatesEmptyManager()
        {
            Assert.AreEqual(0, _entityManager.Count);
        }

        #endregion

        #region AddEntity Tests

        [Test]
        public void AddEntity_IncreasesCount()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));

            _entityManager.AddEntity(submarine);

            Assert.AreEqual(1, _entityManager.Count);
        }

        [Test]
        public void AddEntity_SetsGridOccupied()
        {
            var position = new GridCoordinate(5, 5, 5);
            var submarine = new Submarine(position);

            _entityManager.AddEntity(submarine);

            Assert.IsTrue(_gridSystem.IsOccupied(position));
        }

        [Test]
        public void AddEntity_FiresOnEntityAddedEvent()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            IEntity receivedEntity = null;

            _entityManager.OnEntityAdded += entity => receivedEntity = entity;
            _entityManager.AddEntity(submarine);

            Assert.AreEqual(submarine, receivedEntity);
        }

        [Test]
        public void AddEntity_WithNullEntity_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _entityManager.AddEntity(null));
        }

        [Test]
        public void AddEntity_WithDuplicateId_ThrowsArgumentException()
        {
            var id = Guid.NewGuid();
            var sub1 = new Submarine(id, new GridCoordinate(0, 0, 0), new SubGame.Core.Config.SubmarineConfig());
            var sub2 = new Submarine(id, new GridCoordinate(1, 0, 0), new SubGame.Core.Config.SubmarineConfig());

            _entityManager.AddEntity(sub1);

            Assert.Throws<ArgumentException>(() => _entityManager.AddEntity(sub2));
        }

        [Test]
        public void AddEntity_AtOccupiedPosition_ThrowsInvalidOperationException()
        {
            var position = new GridCoordinate(0, 0, 0);
            var sub1 = new Submarine(position);
            var sub2 = new Submarine(position);

            _entityManager.AddEntity(sub1);

            Assert.Throws<InvalidOperationException>(() => _entityManager.AddEntity(sub2));
        }

        [Test]
        public void AddEntity_OutsideGridBounds_ThrowsInvalidOperationException()
        {
            var invalidPosition = new GridCoordinate(100, 100, 100);
            var submarine = new Submarine(invalidPosition);

            Assert.Throws<InvalidOperationException>(() => _entityManager.AddEntity(submarine));
        }

        #endregion

        #region RemoveEntity Tests

        [Test]
        public void RemoveEntity_DecreasesCount()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);

            bool result = _entityManager.RemoveEntity(submarine.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _entityManager.Count);
        }

        [Test]
        public void RemoveEntity_ClearsGridOccupied()
        {
            var position = new GridCoordinate(5, 5, 5);
            var submarine = new Submarine(position);
            _entityManager.AddEntity(submarine);

            _entityManager.RemoveEntity(submarine.Id);

            Assert.IsFalse(_gridSystem.IsOccupied(position));
        }

        [Test]
        public void RemoveEntity_FiresOnEntityRemovedEvent()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            IEntity receivedEntity = null;

            _entityManager.OnEntityRemoved += entity => receivedEntity = entity;
            _entityManager.RemoveEntity(submarine.Id);

            Assert.AreEqual(submarine, receivedEntity);
        }

        [Test]
        public void RemoveEntity_WithInvalidId_ReturnsFalse()
        {
            bool result = _entityManager.RemoveEntity(Guid.NewGuid());

            Assert.IsFalse(result);
        }

        #endregion

        #region GetEntity Tests

        [Test]
        public void GetEntity_WithValidId_ReturnsEntity()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);

            var result = _entityManager.GetEntity(submarine.Id);

            Assert.AreEqual(submarine, result);
        }

        [Test]
        public void GetEntity_WithInvalidId_ReturnsNull()
        {
            var result = _entityManager.GetEntity(Guid.NewGuid());

            Assert.IsNull(result);
        }

        #endregion

        #region GetEntityAtPosition Tests

        [Test]
        public void GetEntityAtPosition_WithEntityPresent_ReturnsEntity()
        {
            var position = new GridCoordinate(3, 3, 3);
            var submarine = new Submarine(position);
            _entityManager.AddEntity(submarine);

            var result = _entityManager.GetEntityAtPosition(position);

            Assert.AreEqual(submarine, result);
        }

        [Test]
        public void GetEntityAtPosition_WithNoEntity_ReturnsNull()
        {
            var result = _entityManager.GetEntityAtPosition(new GridCoordinate(5, 5, 5));

            Assert.IsNull(result);
        }

        #endregion

        #region GetEntitiesByType Tests

        [Test]
        public void GetSubmarines_ReturnsOnlySubmarines()
        {
            var sub1 = new Submarine(new GridCoordinate(0, 0, 0));
            var sub2 = new Submarine(new GridCoordinate(1, 0, 0));
            var monster = new Monster(new GridCoordinate(2, 0, 0));

            _entityManager.AddEntity(sub1);
            _entityManager.AddEntity(sub2);
            _entityManager.AddEntity(monster);

            var submarines = _entityManager.GetSubmarines().ToList();

            Assert.AreEqual(2, submarines.Count);
            Assert.That(submarines, Does.Contain(sub1));
            Assert.That(submarines, Does.Contain(sub2));
            Assert.IsFalse(submarines.Contains(monster));
        }

        [Test]
        public void GetMonsters_ReturnsOnlyMonsters()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            var monster1 = new Monster(new GridCoordinate(1, 0, 0));
            var monster2 = new Monster(new GridCoordinate(2, 0, 0));

            _entityManager.AddEntity(submarine);
            _entityManager.AddEntity(monster1);
            _entityManager.AddEntity(monster2);

            var monsters = _entityManager.GetMonsters().ToList();

            Assert.AreEqual(2, monsters.Count);
            Assert.That(monsters, Does.Contain(monster1));
            Assert.That(monsters, Does.Contain(monster2));
            Assert.IsFalse(monsters.Contains(submarine));
        }

        #endregion

        #region GetLivingEntities Tests

        [Test]
        public void GetLivingEntities_ExcludesDeadEntities()
        {
            var sub1 = new Submarine(new GridCoordinate(0, 0, 0));
            var sub2 = new Submarine(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(sub1);
            _entityManager.AddEntity(sub2);

            sub2.TakeDamage(sub2.MaxHealth); // Kill sub2

            var living = _entityManager.GetLivingEntities().ToList();

            Assert.AreEqual(1, living.Count);
            Assert.That(living, Does.Contain(sub1));
        }

        #endregion

        #region GetEntitiesInRange Tests

        [Test]
        public void GetEntitiesInRange_ReturnsEntitiesWithinRange()
        {
            var center = new GridCoordinate(5, 5, 5);
            var nearby = new Submarine(new GridCoordinate(6, 5, 5)); // Distance 1
            var farAway = new Monster(new GridCoordinate(0, 0, 0)); // Distance 15

            _entityManager.AddEntity(nearby);
            _entityManager.AddEntity(farAway);

            var inRange = _entityManager.GetEntitiesInRange(center, 3).ToList();

            Assert.AreEqual(1, inRange.Count);
            Assert.That(inRange, Does.Contain(nearby));
        }

        #endregion

        #region Movement Tracking Tests

        [Test]
        public void EntityMoved_UpdatesGridOccupancy()
        {
            var oldPosition = new GridCoordinate(0, 0, 0);
            var newPosition = new GridCoordinate(1, 1, 1);
            var submarine = new Submarine(oldPosition);
            _entityManager.AddEntity(submarine);

            submarine.SetPosition(newPosition);

            Assert.IsFalse(_gridSystem.IsOccupied(oldPosition));
            Assert.IsTrue(_gridSystem.IsOccupied(newPosition));
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_RemovesAllEntities()
        {
            _entityManager.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _entityManager.AddEntity(new Monster(new GridCoordinate(1, 0, 0)));

            _entityManager.Clear();

            Assert.AreEqual(0, _entityManager.Count);
        }

        #endregion

        #region IsValidMovePosition Tests

        [Test]
        public void IsValidMovePosition_UnoccupiedValidPosition_ReturnsTrue()
        {
            Assert.IsTrue(_entityManager.IsValidMovePosition(new GridCoordinate(5, 5, 5)));
        }

        [Test]
        public void IsValidMovePosition_OccupiedPosition_ReturnsFalse()
        {
            var position = new GridCoordinate(5, 5, 5);
            _entityManager.AddEntity(new Submarine(position));

            Assert.IsFalse(_entityManager.IsValidMovePosition(position));
        }

        [Test]
        public void IsValidMovePosition_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(_entityManager.IsValidMovePosition(new GridCoordinate(100, 100, 100)));
        }

        #endregion
    }
}
