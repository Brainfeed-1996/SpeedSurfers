using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Met à jour l'UI en fonction des événements du GameManager.
/// Affiche le score en temps réel et l'écran de fin de partie.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Score en jeu")]
    public TMP_Text scoreText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public Button restartButton;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnGameOver += HandleGameOver;
            HandleScoreChanged(GameManager.Instance.Score);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = $"Score final: {GameManager.Instance.Score}";
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}


