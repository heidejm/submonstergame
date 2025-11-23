using System;
using System.Linq;
using UnityEngine;
using SubGame.Core;
using SubGame.Core.Entities;

namespace SubGame.GameManagement
{
    /// <summary>
    /// Manages game flow including win/lose conditions and game state transitions.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        [Header("UI References")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;

        private GameState _gameState;
        private bool _gameEnded;

        /// <summary>
        /// Current state of the game flow.
        /// </summary>
        public GameFlowState CurrentState { get; private set; } = GameFlowState.Playing;

        /// <summary>
        /// Event fired when the game ends.
        /// Parameters: won (true = victory, false = defeat)
        /// </summary>
        public event Action<bool> OnGameEnded;

        private void Start()
        {
            if (_gameManager == null)
            {
                _gameManager = GetComponent<GameManager>() ?? FindFirstObjectByType<GameManager>();
            }

            if (_gameManager != null)
            {
                _gameState = _gameManager.GameState;

                // Subscribe to attack events to check win/lose after combat
                _gameState.OnEntityAttacked += HandleEntityAttacked;

                // Subscribe to existing entity death events
                foreach (var entity in _gameState.GetSubmarines())
                {
                    entity.OnDeath += HandleEntityDeath;
                }
                foreach (var entity in _gameState.GetMonsters())
                {
                    entity.OnDeath += HandleEntityDeath;
                }

                // Subscribe to new entities being added
                _gameState.OnEntityAdded += HandleEntityAdded;
            }

            // Hide UI panels at start
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_defeatPanel != null) _defeatPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_gameState != null)
            {
                _gameState.OnEntityAttacked -= HandleEntityAttacked;
                _gameState.OnEntityAdded -= HandleEntityAdded;
            }
        }

        private void HandleEntityAdded(IEntity entity)
        {
            entity.OnDeath += HandleEntityDeath;
        }

        private void HandleEntityAttacked(IEntity attacker, IEntity target, int damage)
        {
            // Check win/lose after any attack
            CheckGameEndConditions();
        }

        private void HandleEntityDeath(IEntity entity)
        {
            // Don't check if game already ended
            if (_gameEnded) return;

            Debug.Log($"{entity.Name} has been destroyed!");
            CheckGameEndConditions();
        }

        /// <summary>
        /// Checks if the game has ended (all subs dead or all monsters dead).
        /// </summary>
        public void CheckGameEndConditions()
        {
            if (_gameEnded || _gameState == null) return;

            // Count living entities
            var livingSubmarines = _gameState.GetSubmarines().Where(s => s.IsAlive).ToList();
            var livingMonsters = _gameState.GetMonsters().Where(m => m.IsAlive).ToList();

            // Check defeat: all submarines dead
            if (livingSubmarines.Count == 0)
            {
                EndGame(victory: false);
                return;
            }

            // Check victory: all monsters dead
            if (livingMonsters.Count == 0)
            {
                EndGame(victory: true);
                return;
            }
        }

        private void EndGame(bool victory)
        {
            _gameEnded = true;
            CurrentState = victory ? GameFlowState.Victory : GameFlowState.Defeat;

            Debug.Log(victory ? "=== VICTORY ===" : "=== DEFEAT ===");

            // Show appropriate UI
            if (victory && _victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }
            else if (!victory && _defeatPanel != null)
            {
                _defeatPanel.SetActive(true);
            }

            OnGameEnded?.Invoke(victory);
        }

        /// <summary>
        /// Restarts the game by reloading the current scene.
        /// </summary>
        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        /// <summary>
        /// Returns to main menu (if one exists).
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Load main menu scene - adjust name as needed
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            Debug.Log("Return to main menu - not implemented yet");
        }
    }

    /// <summary>
    /// Possible states of the game flow.
    /// </summary>
    public enum GameFlowState
    {
        Playing,
        Victory,
        Defeat,
        Paused
    }
}
