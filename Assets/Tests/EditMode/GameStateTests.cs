using System.Linq;
using NUnit.Framework;
using SubGame.Core;
using SubGame.Core.Entities;
using SubGame.Core.Grid;
using SubGame.Core.TurnManagement;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for GameState facade class.
    /// </summary>
    [TestFixture]
    public class GameStateTests
    {
        private GameState _gameState;

        [SetUp]
        public void Setup()
        {
            _gameState = new GameState(10, 10, 10);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_CreatesValidGrid()
        {
            Assert.IsNotNull(_gameState.Grid);
            Assert.AreEqual(10, _gameState.Grid.Width);
            Assert.AreEqual(10, _gameState.Grid.Height);
            Assert.AreEqual(10, _gameState.Grid.Depth);
        }

        [Test]
        public void Constructor_StartsWithNoEntities()
        {
            Assert.AreEqual(0, _gameState.EntityCount);
        }

        [Test]
        public void Constructor_GameNotStarted()
        {
            Assert.IsFalse(_gameState.HasStarted);
        }

        #endregion

        #region Entity Management Tests

        [Test]
        public void AddEntity_IncreasesCount()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));

            _gameState.AddEntity(submarine);

            Assert.AreEqual(1, _gameState.EntityCount);
        }

        [Test]
        public void AddEntity_FiresEvent()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            IEntity received = null;

            _gameState.OnEntityAdded += entity => received = entity;
            _gameState.AddEntity(submarine);

            Assert.AreEqual(submarine, received);
        }

        [Test]
        public void RemoveEntity_DecreasesCount()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _gameState.AddEntity(submarine);

            _gameState.RemoveEntity(submarine.Id);

            Assert.AreEqual(0, _gameState.EntityCount);
        }

        [Test]
        public void GetEntity_ReturnsCorrectEntity()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _gameState.AddEntity(submarine);

            var result = _gameState.GetEntity(submarine.Id);

            Assert.AreEqual(submarine, result);
        }

        [Test]
        public void GetSubmarines_ReturnsOnlySubmarines()
        {
            var sub = new Submarine(new GridCoordinate(0, 0, 0));
            var monster = new Monster(new GridCoordinate(1, 0, 0));
            _gameState.AddEntity(sub);
            _gameState.AddEntity(monster);

            var submarines = _gameState.GetSubmarines().ToList();

            Assert.AreEqual(1, submarines.Count);
            Assert.AreEqual(sub, submarines[0]);
        }

        #endregion

        #region Turn Management Tests

        [Test]
        public void StartGame_SetsHasStarted()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));

            _gameState.StartGame();

            Assert.IsTrue(_gameState.HasStarted);
            Assert.AreEqual(1, _gameState.CurrentTurn);
        }

        [Test]
        public void StartGame_FiresOnTurnStarted()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            int receivedTurn = 0;

            _gameState.OnTurnStarted += turn => receivedTurn = turn;
            _gameState.StartGame();

            Assert.AreEqual(1, receivedTurn);
        }

        [Test]
        public void AdvancePhase_ChangesPhase()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _gameState.StartGame();

            _gameState.AdvancePhase();

            Assert.AreEqual(TurnPhase.PlayerAction, _gameState.CurrentPhase);
        }

        [Test]
        public void AdvancePhase_FiresOnPhaseChanged()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _gameState.StartGame();
            TurnPhase receivedPhase = TurnPhase.TurnStart;

            _gameState.OnPhaseChanged += phase => receivedPhase = phase;
            _gameState.AdvancePhase();

            Assert.AreEqual(TurnPhase.PlayerAction, receivedPhase);
        }

        [Test]
        public void IsPlayerPhase_DuringPlayerAction_ReturnsTrue()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _gameState.StartGame();
            _gameState.AdvancePhase();

            Assert.IsTrue(_gameState.IsPlayerPhase);
        }

        #endregion

        #region Pathfinding Tests

        [Test]
        public void FindPath_ReturnsValidPath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(3, 0, 0);

            var path = _gameState.FindPath(start, end);

            Assert.AreEqual(4, path.Count);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[3]);
        }

        [Test]
        public void GetReachablePositions_ReturnsValidPositions()
        {
            var submarine = new Submarine(new GridCoordinate(5, 5, 5));
            _gameState.AddEntity(submarine);

            var reachable = _gameState.GetReachablePositions(submarine);

            // Submarine has movement range of 3
            Assert.Greater(reachable.Count, 0);
            Assert.IsFalse(reachable.Contains(submarine.Position));
        }

        [Test]
        public void GetReachablePositions_DeadEntity_ReturnsEmpty()
        {
            var submarine = new Submarine(new GridCoordinate(5, 5, 5));
            _gameState.AddEntity(submarine);
            submarine.TakeDamage(submarine.MaxHealth);

            var reachable = _gameState.GetReachablePositions(submarine);

            Assert.AreEqual(0, reachable.Count);
        }

        [Test]
        public void GetPathDistance_ReturnsCorrectDistance()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(3, 0, 0);

            var distance = _gameState.GetPathDistance(start, end);

            Assert.AreEqual(3, distance);
        }

        #endregion

        #region Movement Tests

        [Test]
        public void MoveEntity_UpdatesPosition()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _gameState.AddEntity(submarine);
            var newPosition = new GridCoordinate(1, 1, 1);

            _gameState.MoveEntity(submarine, newPosition);

            Assert.AreEqual(newPosition, submarine.Position);
        }

        [Test]
        public void MoveEntity_FiresOnEntityMoved()
        {
            var submarine = new Submarine(new GridCoordinate(0, 0, 0));
            _gameState.AddEntity(submarine);
            var newPosition = new GridCoordinate(1, 1, 1);
            IEntity receivedEntity = null;
            GridCoordinate receivedOld = default;
            GridCoordinate receivedNew = default;

            _gameState.OnEntityMoved += (entity, oldPos, newPos) =>
            {
                receivedEntity = entity;
                receivedOld = oldPos;
                receivedNew = newPos;
            };

            _gameState.MoveEntity(submarine, newPosition);

            Assert.AreEqual(submarine, receivedEntity);
            Assert.AreEqual(new GridCoordinate(0, 0, 0), receivedOld);
            Assert.AreEqual(newPosition, receivedNew);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void Reset_ClearsEntities()
        {
            _gameState.AddEntity(new Submarine(new GridCoordinate(0, 0, 0)));
            _gameState.AddEntity(new Monster(new GridCoordinate(1, 0, 0)));
            _gameState.StartGame();

            _gameState.Reset();

            Assert.AreEqual(0, _gameState.EntityCount);
            Assert.IsFalse(_gameState.HasStarted);
        }

        #endregion
    }
}
