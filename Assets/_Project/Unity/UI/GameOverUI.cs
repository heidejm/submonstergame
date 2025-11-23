using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SubGame.Unity.UI
{
    /// <summary>
    /// Handles the game over UI display for victory and defeat states.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Image _backgroundImage;

        [Header("Victory Settings")]
        [SerializeField] private string _victoryTitle = "VICTORY";
        [SerializeField] private string _victoryMessage = "All threats have been eliminated!";
        [SerializeField] private Color _victoryColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);

        [Header("Defeat Settings")]
        [SerializeField] private string _defeatTitle = "DEFEAT";
        [SerializeField] private string _defeatMessage = "Your submarine has been destroyed.";
        [SerializeField] private Color _defeatColor = new Color(0.6f, 0.2f, 0.2f, 0.9f);

        [Header("References")]
        [SerializeField] private GameManagement.GameFlowController _gameFlowController;

        private void Start()
        {
            // Hide panel at start
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            // Find GameFlowController if not assigned
            if (_gameFlowController == null)
            {
                _gameFlowController = FindFirstObjectByType<GameManagement.GameFlowController>();
            }

            // Subscribe to game end event
            if (_gameFlowController != null)
            {
                _gameFlowController.OnGameEnded += HandleGameEnded;
            }

            // Setup restart button
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnDestroy()
        {
            if (_gameFlowController != null)
            {
                _gameFlowController.OnGameEnded -= HandleGameEnded;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        private void HandleGameEnded(bool victory)
        {
            ShowGameOver(victory);
        }

        /// <summary>
        /// Shows the game over panel with appropriate victory/defeat styling.
        /// </summary>
        /// <param name="victory">True for victory, false for defeat</param>
        public void ShowGameOver(bool victory)
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
            }

            if (_titleText != null)
            {
                _titleText.text = victory ? _victoryTitle : _defeatTitle;
            }

            if (_messageText != null)
            {
                _messageText.text = victory ? _victoryMessage : _defeatMessage;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = victory ? _victoryColor : _defeatColor;
            }
        }

        /// <summary>
        /// Hides the game over panel.
        /// </summary>
        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        private void OnRestartClicked()
        {
            if (_gameFlowController != null)
            {
                _gameFlowController.RestartGame();
            }
        }
    }
}
