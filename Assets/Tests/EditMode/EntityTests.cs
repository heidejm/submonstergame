using System;
using NUnit.Framework;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for Entity base class functionality.
    /// Uses Submarine as concrete implementation for testing.
    /// </summary>
    [TestFixture]
    public class EntityTests
    {
        private GridCoordinate _defaultPosition;

        [SetUp]
        public void Setup()
        {
            _defaultPosition = new GridCoordinate(0, 0, 0);
        }

        #region Construction Tests

        [Test]
        public void Constructor_WithValidParameters_CreatesEntity()
        {
            var submarine = new Submarine(_defaultPosition);

            Assert.IsNotNull(submarine);
            Assert.AreEqual("Submarine", submarine.Name);
            Assert.AreEqual(100, submarine.MaxHealth);
            Assert.AreEqual(100, submarine.Health);
            Assert.AreEqual(_defaultPosition, submarine.Position);
            Assert.IsTrue(submarine.IsAlive);
        }

        [Test]
        public void Constructor_WithCustomName_SetsName()
        {
            var submarine = new Submarine(_defaultPosition, "USS Nautilus");

            Assert.AreEqual("USS Nautilus", submarine.Name);
        }

        [Test]
        public void Constructor_GeneratesUniqueId()
        {
            var sub1 = new Submarine(_defaultPosition);
            var sub2 = new Submarine(_defaultPosition);

            Assert.AreNotEqual(sub1.Id, sub2.Id);
        }

        [Test]
        public void Constructor_WithNullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Submarine(Guid.NewGuid(), _defaultPosition, null));
        }

        #endregion

        #region EntityType Tests

        [Test]
        public void EntityType_Submarine_ReturnsSubmarine()
        {
            var submarine = new Submarine(_defaultPosition);

            Assert.AreEqual(EntityType.Submarine, submarine.EntityType);
        }

        [Test]
        public void EntityType_Monster_ReturnsMonster()
        {
            var monster = new Monster(_defaultPosition);

            Assert.AreEqual(EntityType.Monster, monster.EntityType);
        }

        #endregion

        #region TakeDamage Tests

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            var submarine = new Submarine(_defaultPosition);
            int initialHealth = submarine.Health;

            submarine.TakeDamage(30);

            Assert.AreEqual(initialHealth - 30, submarine.Health);
        }

        [Test]
        public void TakeDamage_HealthCannotGoBelowZero()
        {
            var submarine = new Submarine(_defaultPosition);

            submarine.TakeDamage(999);

            Assert.AreEqual(0, submarine.Health);
        }

        [Test]
        public void TakeDamage_FiresOnDamageTakenEvent()
        {
            var submarine = new Submarine(_defaultPosition);
            IEntity receivedEntity = null;
            int receivedDamage = 0;

            submarine.OnDamageTaken += (entity, damage) =>
            {
                receivedEntity = entity;
                receivedDamage = damage;
            };

            submarine.TakeDamage(25);

            Assert.AreEqual(submarine, receivedEntity);
            Assert.AreEqual(25, receivedDamage);
        }

        [Test]
        public void TakeDamage_WhenKilled_FiresOnDeathEvent()
        {
            var submarine = new Submarine(_defaultPosition);
            bool deathEventFired = false;

            submarine.OnDeath += entity => deathEventFired = true;

            submarine.TakeDamage(submarine.MaxHealth);

            Assert.IsTrue(deathEventFired);
            Assert.IsFalse(submarine.IsAlive);
        }

        [Test]
        public void TakeDamage_WhenAlreadyDead_DoesNothing()
        {
            var submarine = new Submarine(_defaultPosition);
            submarine.TakeDamage(submarine.MaxHealth); // Kill it

            int healthAfterDeath = submarine.Health;
            submarine.TakeDamage(50);

            Assert.AreEqual(healthAfterDeath, submarine.Health);
        }

        [Test]
        public void TakeDamage_NegativeAmount_ThrowsArgumentException()
        {
            var submarine = new Submarine(_defaultPosition);

            Assert.Throws<ArgumentException>(() => submarine.TakeDamage(-10));
        }

        #endregion

        #region Heal Tests

        [Test]
        public void Heal_IncreasesHealth()
        {
            var submarine = new Submarine(_defaultPosition);
            submarine.TakeDamage(50);
            int healthAfterDamage = submarine.Health;

            submarine.Heal(20);

            Assert.AreEqual(healthAfterDamage + 20, submarine.Health);
        }

        [Test]
        public void Heal_CannotExceedMaxHealth()
        {
            var submarine = new Submarine(_defaultPosition);
            submarine.TakeDamage(10);

            submarine.Heal(100);

            Assert.AreEqual(submarine.MaxHealth, submarine.Health);
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            var submarine = new Submarine(_defaultPosition);
            submarine.TakeDamage(submarine.MaxHealth); // Kill it

            submarine.Heal(50);

            Assert.AreEqual(0, submarine.Health);
            Assert.IsFalse(submarine.IsAlive);
        }

        [Test]
        public void Heal_NegativeAmount_ThrowsArgumentException()
        {
            var submarine = new Submarine(_defaultPosition);

            Assert.Throws<ArgumentException>(() => submarine.Heal(-10));
        }

        #endregion

        #region SetPosition Tests

        [Test]
        public void SetPosition_UpdatesPosition()
        {
            var submarine = new Submarine(_defaultPosition);
            var newPosition = new GridCoordinate(5, 5, 5);

            submarine.SetPosition(newPosition);

            Assert.AreEqual(newPosition, submarine.Position);
        }

        [Test]
        public void SetPosition_FiresOnMovedEvent()
        {
            var submarine = new Submarine(_defaultPosition);
            GridCoordinate receivedOldPosition = default;
            GridCoordinate receivedNewPosition = default;

            submarine.OnMoved += (entity, oldPos, newPos) =>
            {
                receivedOldPosition = oldPos;
                receivedNewPosition = newPos;
            };

            var newPosition = new GridCoordinate(3, 2, 1);
            submarine.SetPosition(newPosition);

            Assert.AreEqual(_defaultPosition, receivedOldPosition);
            Assert.AreEqual(newPosition, receivedNewPosition);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            var submarine = new Submarine(_defaultPosition, "Test Sub");

            string result = submarine.ToString();

            Assert.That(result, Does.Contain("Test Sub"));
            Assert.That(result, Does.Contain("Submarine"));
            Assert.That(result, Does.Contain(submarine.Position.ToString()));
        }

        #endregion
    }
}
