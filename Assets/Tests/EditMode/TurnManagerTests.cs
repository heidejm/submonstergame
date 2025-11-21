using System;
using NUnit.Framework;
using SubGame.Core.Entities;
using SubGame.Core.Grid;
using SubGame.Core.TurnManagement;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for TurnManager class.
    /// </summary>
    [TestFixture]
    public class TurnManagerTests
    {
        private IGridSystem _gridSystem;
        private EntityManager _entityManager;
        private TurnManager _turnManager;

        [SetUp]
        public void Setup()
        {
            _gridSystem = new GridSystem(10, 10, 10);
            _entityManager = new EntityManager(_gridSystem);
            _turnManager = new TurnManager(_entityManager);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithNullEntityManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnManager(null));
        }

        [Test]
        public void Constructor_InitializesWithTurnZero()
        {
            Assert.AreEqual(0, _turnManager.CurrentTurn);
            Assert.IsFalse(_turnManager.HasStarted);
        }

        #endregion

        #region StartGame Tests

        [Test]
        public void StartGame_SetsTurnToOne()
        {
            _turnManager.StartGame();

            Assert.AreEqual(1, _turnManager.CurrentTurn);
            Assert.IsTrue(_turnManager.HasStarted);
        }

        [Test]
        public void StartGame_FiresOnTurnStartedEvent()
        {
            int receivedTurn = 0;
            _turnManager.OnTurnStarted += turn => receivedTurn = turn;

            _turnManager.StartGame();

            Assert.AreEqual(1, receivedTurn);
        }

        [Test]
        public void StartGame_CalledTwice_ThrowsInvalidOperationException()
        {
            _turnManager.StartGame();

            Assert.Throws<InvalidOperationException>(() => _turnManager.StartGame());
        }

        [Test]
        public void StartGame_SetsPhaseToTurnStart()
        {
            _turnManager.StartGame();

            Assert.AreEqual(TurnPhase.TurnStart, _turnManager.CurrentPhase);
        }

        #endregion

        #region AdvancePhase Tests

        [Test]
        public void AdvancePhase_BeforeStartGame_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _turnManager.AdvancePhase());
        }

        [Test]
        public void AdvancePhase_FromTurnStart_GoesToPlayerAction()
        {
            _entityManager.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _turnManager.StartGame();

            _turnManager.AdvancePhase();

            Assert.AreEqual(TurnPhase.PlayerAction, _turnManager.CurrentPhase);
        }

        [Test]
        public void AdvancePhase_SetsActiveEntityToFirstSubmarine()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            _turnManager.StartGame();

            _turnManager.AdvancePhase();

            Assert.AreEqual(submarine, _turnManager.ActiveEntity);
        }

        [Test]
        public void AdvancePhase_FiresOnPhaseChangedEvent()
        {
            TurnPhase receivedPhase = TurnPhase.TurnStart;
            _turnManager.OnPhaseChanged += phase => receivedPhase = phase;
            _turnManager.StartGame();

            _turnManager.AdvancePhase();

            Assert.AreEqual(TurnPhase.PlayerAction, receivedPhase);
        }

        [Test]
        public void AdvancePhase_FiresOnActiveEntityChangedEvent()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            IEntity receivedEntity = null;
            _turnManager.OnActiveEntityChanged += entity => receivedEntity = entity;
            _turnManager.StartGame();

            _turnManager.AdvancePhase();

            Assert.AreEqual(submarine, receivedEntity);
        }

        [Test]
        public void AdvancePhase_ThroughMultipleSubmarines()
        {
            var sub1 = new Submarine(new GridCoordinate(0, 0, 0));
            var sub2 = new Submarine(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(sub1);
            _entityManager.AddEntity(sub2);
            _turnManager.StartGame();

            _turnManager.AdvancePhase(); // TurnStart -> PlayerAction, active = sub1
            Assert.AreEqual(sub1, _turnManager.ActiveEntity);

            _turnManager.AdvancePhase(); // Still PlayerAction, active = sub2
            Assert.AreEqual(sub2, _turnManager.ActiveEntity);
        }

        [Test]
        public void AdvancePhase_FromPlayerAction_GoesToEnemyAction_WhenNoMoreSubmarines()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            var monster = new Monster(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(submarine);
            _entityManager.AddEntity(monster);
            _turnManager.StartGame();

            _turnManager.AdvancePhase(); // TurnStart -> PlayerAction
            _turnManager.AdvancePhase(); // PlayerAction -> EnemyAction

            Assert.AreEqual(TurnPhase.EnemyAction, _turnManager.CurrentPhase);
            Assert.AreEqual(monster, _turnManager.ActiveEntity);
        }

        [Test]
        public void AdvancePhase_FromEnemyAction_GoesToTurnEnd_WhenNoMoreMonsters()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            var monster = new Monster(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(submarine);
            _entityManager.AddEntity(monster);
            _turnManager.StartGame();

            _turnManager.AdvancePhase(); // TurnStart -> PlayerAction
            _turnManager.AdvancePhase(); // PlayerAction -> EnemyAction
            _turnManager.AdvancePhase(); // EnemyAction -> TurnEnd

            Assert.AreEqual(TurnPhase.TurnEnd, _turnManager.CurrentPhase);
        }

        [Test]
        public void AdvancePhase_FromTurnEnd_StartsNewTurn()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            _turnManager.StartGame();

            // Complete a full turn cycle
            _turnManager.AdvancePhase(); // TurnStart -> PlayerAction
            _turnManager.AdvancePhase(); // PlayerAction -> EnemyAction (no monsters)
            _turnManager.AdvancePhase(); // EnemyAction -> TurnEnd

            int turnEndedCount = 0;
            _turnManager.OnTurnEnded += turn => turnEndedCount++;
            _turnManager.AdvancePhase(); // TurnEnd -> TurnStart (new turn)

            Assert.AreEqual(2, _turnManager.CurrentTurn);
            Assert.AreEqual(TurnPhase.TurnStart, _turnManager.CurrentPhase);
        }

        #endregion

        #region IsPlayerPhase / IsEnemyPhase Tests

        [Test]
        public void IsPlayerPhase_DuringPlayerAction_ReturnsTrue()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            _turnManager.StartGame();
            _turnManager.AdvancePhase();

            Assert.IsTrue(_turnManager.IsPlayerPhase);
            Assert.IsFalse(_turnManager.IsEnemyPhase);
        }

        [Test]
        public void IsEnemyPhase_DuringEnemyAction_ReturnsTrue()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            var monster = new Monster(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(submarine);
            _entityManager.AddEntity(monster);
            _turnManager.StartGame();
            _turnManager.AdvancePhase(); // TurnStart -> PlayerAction
            _turnManager.AdvancePhase(); // PlayerAction -> EnemyAction

            Assert.IsTrue(_turnManager.IsEnemyPhase);
            Assert.IsFalse(_turnManager.IsPlayerPhase);
        }

        #endregion

        #region EndCurrentEntityTurn Tests

        [Test]
        public void EndCurrentEntityTurn_WithNoActiveEntity_ThrowsInvalidOperationException()
        {
            _turnManager.StartGame(); // Phase is TurnStart, no active entity

            Assert.Throws<InvalidOperationException>(() => _turnManager.EndCurrentEntityTurn());
        }

        [Test]
        public void EndCurrentEntityTurn_AdvancesToNextEntity()
        {
            var sub1 = new Submarine(new GridCoordinate(0, 0, 0));
            var sub2 = new Submarine(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(sub1);
            _entityManager.AddEntity(sub2);
            _turnManager.StartGame();
            _turnManager.AdvancePhase(); // Active = sub1

            _turnManager.EndCurrentEntityTurn();

            Assert.AreEqual(sub2, _turnManager.ActiveEntity);
        }

        #endregion

        #region Dead Entity Handling Tests

        [Test]
        public void AdvancePhase_SkipsDeadEntities()
        {
            var sub1 = new Submarine(new GridCoordinate(0, 0, 0));
            var sub2 = new Submarine(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(sub1);
            _entityManager.AddEntity(sub2);

            sub1.TakeDamage(sub1.MaxHealth); // Kill sub1

            _turnManager.StartGame();
            _turnManager.AdvancePhase();

            Assert.AreEqual(sub2, _turnManager.ActiveEntity);
        }

        #endregion

        #region GetTurnOrder Tests

        [Test]
        public void GetTurnOrder_ReturnsSubmarinesThenMonsters()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            var monster = new Monster(new GridCoordinate(1, 0, 0));
            _entityManager.AddEntity(monster);
            _entityManager.AddEntity(submarine);

            _turnManager.StartGame();
            var turnOrder = _turnManager.GetTurnOrder();

            Assert.AreEqual(2, turnOrder.Count);
            Assert.AreEqual(EntityType.Submarine, turnOrder[0].EntityType);
            Assert.AreEqual(EntityType.Monster, turnOrder[1].EntityType);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void Reset_ResetsAllState()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _entityManager.AddEntity(submarine);
            _turnManager.StartGame();
            _turnManager.AdvancePhase();

            _turnManager.Reset();

            Assert.AreEqual(0, _turnManager.CurrentTurn);
            Assert.IsFalse(_turnManager.HasStarted);
            Assert.IsNull(_turnManager.ActiveEntity);
            Assert.AreEqual(TurnPhase.TurnStart, _turnManager.CurrentPhase);
        }

        #endregion
    }
}
