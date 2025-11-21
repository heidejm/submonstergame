using System;
using NUnit.Framework;
using SubGame.Core;
using SubGame.Core.Commands;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for Command classes (MoveCommand, EndTurnCommand).
    /// </summary>
    [TestFixture]
    public class CommandTests
    {
        private GameState _gameState;
        private Submarine _submarine;

        [SetUp]
        public void Setup()
        {
            _gameState = new GameState(10, 10, 10);
            _submarine = new Submarine(new GridCoordinate(5, 5, 5));
            _gameState.AddEntity(_submarine);
            _gameState.StartGame();
            _gameState.AdvancePhase(); // Move to PlayerAction phase
        }

        #region MoveCommand Validation Tests

        [Test]
        public void MoveCommand_ValidMove_Succeeds()
        {
            var target = new GridCoordinate(6, 5, 5); // Adjacent cell
            var command = new MoveCommand(_submarine, target);

            var result = command.Validate(_gameState);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void MoveCommand_GameNotStarted_Fails()
        {
            var freshState = new GameState(10, 10, 10);
            var sub = new Submarine(new GridCoordinate(5, 5, 5));
            freshState.AddEntity(sub);
            // Don't start game

            var command = new MoveCommand(sub, new GridCoordinate(6, 5, 5));

            var result = command.Validate(freshState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not started"));
        }

        [Test]
        public void MoveCommand_EntityNotFound_Fails()
        {
            var command = new MoveCommand(Guid.NewGuid(), new GridCoordinate(6, 5, 5));

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not found"));
        }

        [Test]
        public void MoveCommand_NotEntitysTurn_Fails()
        {
            // Add another submarine - first sub is active from Setup
            var sub2 = new Submarine(new GridCoordinate(3, 3, 3));
            _gameState.AddEntity(sub2);

            // First sub (_submarine) is active, try to move second sub
            var command = new MoveCommand(sub2, new GridCoordinate(4, 3, 3));

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not this entity's turn"));
        }

        [Test]
        public void MoveCommand_TargetOutOfBounds_Fails()
        {
            var command = new MoveCommand(_submarine, new GridCoordinate(100, 100, 100));

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("outside grid bounds"));
        }

        [Test]
        public void MoveCommand_TargetOccupied_Fails()
        {
            var occupiedPosition = new GridCoordinate(6, 5, 5);
            var monster = new Monster(occupiedPosition);
            _gameState.AddEntity(monster);

            var command = new MoveCommand(_submarine, occupiedPosition);

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("occupied"));
        }

        [Test]
        public void MoveCommand_AlreadyAtTarget_Fails()
        {
            var command = new MoveCommand(_submarine, _submarine.Position);

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("already at"));
        }

        [Test]
        public void MoveCommand_OutOfRange_Fails()
        {
            // Submarine has movement range of 3
            var farAway = new GridCoordinate(9, 9, 9); // Way outside range

            var command = new MoveCommand(_submarine, farAway);

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not reachable"));
        }

        [Test]
        public void MoveCommand_DeadEntity_Fails()
        {
            _submarine.TakeDamage(_submarine.MaxHealth); // Kill submarine

            var command = new MoveCommand(_submarine, new GridCoordinate(6, 5, 5));

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("dead"));
        }

        #endregion

        #region MoveCommand Execution Tests

        [Test]
        public void MoveCommand_Execute_UpdatesPosition()
        {
            var target = new GridCoordinate(6, 5, 5);
            var command = new MoveCommand(_submarine, target);

            command.Execute(_gameState);

            Assert.AreEqual(target, _submarine.Position);
        }

        [Test]
        public void MoveCommand_Execute_FiresEvent()
        {
            var target = new GridCoordinate(6, 5, 5);
            var command = new MoveCommand(_submarine, target);
            GridCoordinate receivedOld = default;
            GridCoordinate receivedNew = default;

            _gameState.OnEntityMoved += (entity, oldPos, newPos) =>
            {
                receivedOld = oldPos;
                receivedNew = newPos;
            };

            command.Execute(_gameState);

            Assert.AreEqual(new GridCoordinate(5, 5, 5), receivedOld);
            Assert.AreEqual(target, receivedNew);
        }

        #endregion

        #region EndTurnCommand Validation Tests

        [Test]
        public void EndTurnCommand_ValidEntity_Succeeds()
        {
            var command = new EndTurnCommand(_submarine);

            var result = command.Validate(_gameState);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void EndTurnCommand_GameNotStarted_Fails()
        {
            var freshState = new GameState(10, 10, 10);
            var sub = new Submarine(new GridCoordinate(5, 5, 5));
            freshState.AddEntity(sub);

            var command = new EndTurnCommand(sub);

            var result = command.Validate(freshState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not started"));
        }

        [Test]
        public void EndTurnCommand_NotEntitysTurn_Fails()
        {
            var otherSub = new Submarine(new GridCoordinate(3, 3, 3));
            _gameState.AddEntity(otherSub);

            // First sub is active
            var command = new EndTurnCommand(otherSub);

            var result = command.Validate(_gameState);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("not this entity's turn"));
        }

        #endregion

        #region EndTurnCommand Execution Tests

        [Test]
        public void EndTurnCommand_Execute_AdvancesTurn()
        {
            // Create fresh game state with two submarines added BEFORE starting
            var gameState = new GameState(10, 10, 10);
            var sub1 = new Submarine(new GridCoordinate(5, 5, 5));
            var sub2 = new Submarine(new GridCoordinate(3, 3, 3));
            gameState.AddEntity(sub1);
            gameState.AddEntity(sub2);
            gameState.StartGame();
            gameState.AdvancePhase(); // Move to PlayerAction

            // First sub is active
            Assert.AreEqual(sub1.Id, gameState.ActiveEntity.Id);

            var command = new EndTurnCommand(sub1);
            command.Execute(gameState);

            // Now second sub should be active
            Assert.AreEqual(sub2.Id, gameState.ActiveEntity.Id);
        }

        #endregion

        #region GameState.ExecuteCommand Tests

        [Test]
        public void ExecuteCommand_ValidCommand_ReturnsSuccess()
        {
            var command = new MoveCommand(_submarine, new GridCoordinate(6, 5, 5));

            var result = _gameState.ExecuteCommand(command);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ExecuteCommand_InvalidCommand_ReturnsFailure()
        {
            var command = new MoveCommand(_submarine, new GridCoordinate(100, 100, 100));

            var result = _gameState.ExecuteCommand(command);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ExecuteCommand_NullCommand_ReturnsFailure()
        {
            var result = _gameState.ExecuteCommand(null);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("null"));
        }

        [Test]
        public void ExecuteCommand_FiresOnCommandExecuted()
        {
            var command = new MoveCommand(_submarine, new GridCoordinate(6, 5, 5));
            ICommand receivedCommand = null;

            _gameState.OnCommandExecuted += cmd => receivedCommand = cmd;
            _gameState.ExecuteCommand(command);

            Assert.AreEqual(command, receivedCommand);
        }

        #endregion
    }
}
