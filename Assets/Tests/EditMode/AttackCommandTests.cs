using System.Linq;
using NUnit.Framework;
using SubGame.Core;
using SubGame.Core.Commands;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for the AttackCommand class.
    /// </summary>
    [TestFixture]
    public class AttackCommandTests
    {
        private GameState _gameState;
        private Submarine _submarine;
        private Monster _monster;

        [SetUp]
        public void SetUp()
        {
            _gameState = new GameState(10, 5, 10);

            // Place submarine and monster within attack range of each other
            // Submarine has attack range 2, Monster has attack range 1
            _submarine = new Submarine(new GridCoordinate(2, 0, 2), "Test Sub");
            _monster = new Monster(new GridCoordinate(3, 0, 2), "Test Monster"); // Distance 1 from sub

            _gameState.AddEntity(_submarine);
            _gameState.AddEntity(_monster);
            _gameState.StartGame();
            _gameState.AdvancePhase(); // Move to PlayerAction
        }

        [Test]
        public void Validate_ValidAttack_ReturnsSuccess()
        {
            var command = new AttackCommand(_submarine, _monster);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void Validate_GameNotStarted_Fails()
        {
            var freshState = new GameState(10, 5, 10);
            var sub = new Submarine(new GridCoordinate(0, 0, 0), "Sub");
            var mon = new Monster(new GridCoordinate(1, 0, 0), "Mon");
            freshState.AddEntity(sub);
            freshState.AddEntity(mon);
            // Don't start game

            var command = new AttackCommand(sub, mon);

            var result = command.Validate(freshState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not started"));
        }

        [Test]
        public void Validate_NotEntitysTurn_Fails()
        {
            // It's submarine's turn, try to attack with monster
            var command = new AttackCommand(_monster, _submarine);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not this entity's turn"));
        }

        [Test]
        public void Validate_TargetOutOfRange_Fails()
        {
            // Create a monster far away
            var farMonster = new Monster(new GridCoordinate(8, 0, 8), "Far Monster");
            _gameState.AddEntity(farMonster);

            var command = new AttackCommand(_submarine, farMonster);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("out of attack range"));
        }

        [Test]
        public void Validate_AttackingSelf_Fails()
        {
            var command = new AttackCommand(_submarine, _submarine);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Cannot attack self"));
        }

        [Test]
        public void Validate_TargetDead_Fails()
        {
            // Kill the monster first
            _monster.TakeDamage(1000);
            Assert.That(_monster.IsAlive, Is.False);

            var command = new AttackCommand(_submarine, _monster);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("already dead"));
        }

        [Test]
        public void Validate_AttackerDead_Fails()
        {
            // Kill the submarine first
            _submarine.TakeDamage(1000);
            Assert.That(_submarine.IsAlive, Is.False);

            var command = new AttackCommand(_submarine, _monster);

            var result = command.Validate(_gameState);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Attacker is dead"));
        }

        [Test]
        public void Execute_AppliesDamage()
        {
            int initialHealth = _monster.Health;
            var command = new AttackCommand(_submarine, _monster);

            // Validate first
            var validateResult = command.Validate(_gameState);
            Assert.That(validateResult.Success, Is.True);

            // Execute
            command.Execute(_gameState);

            Assert.That(_monster.Health, Is.EqualTo(initialHealth - _submarine.AttackDamage));
        }

        [Test]
        public void Execute_KillsTargetWhenDamageExceedsHealth()
        {
            // Damage monster until almost dead
            _monster.TakeDamage(_monster.Health - 1);
            Assert.That(_monster.IsAlive, Is.True);

            var command = new AttackCommand(_submarine, _monster);
            var result = command.Validate(_gameState);
            Assert.That(result.Success, Is.True);

            command.Execute(_gameState);

            Assert.That(_monster.IsAlive, Is.False);
        }

        [Test]
        public void TryAttack_ReturnsTrue_OnSuccess()
        {
            bool success = _gameState.TryAttack(_monster);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryAttack_ReturnsFalse_WhenOutOfRange()
        {
            var farMonster = new Monster(new GridCoordinate(8, 0, 8), "Far Monster");
            _gameState.AddEntity(farMonster);

            bool success = _gameState.TryAttack(farMonster);

            Assert.That(success, Is.False);
        }

        [Test]
        public void GetAttackableTargets_ReturnsTargetsInRange()
        {
            var targets = _gameState.GetAttackableTargets().ToList();

            Assert.That(targets.Contains(_monster), Is.True);
        }

        [Test]
        public void GetAttackableTargets_ExcludesOutOfRangeTargets()
        {
            var farMonster = new Monster(new GridCoordinate(8, 0, 8), "Far Monster");
            _gameState.AddEntity(farMonster);

            var targets = _gameState.GetAttackableTargets().ToList();

            Assert.That(targets.Contains(farMonster), Is.False);
        }

        [Test]
        public void GetAttackableTargets_ExcludesSelf()
        {
            var targets = _gameState.GetAttackableTargets().ToList();

            Assert.That(targets.Contains(_submarine), Is.False);
        }
    }
}
