using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère l'état global du jeu: score, Game Over, et restart.
/// Expose des événements pour l'UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    public int Score { get; private set; }

    /// <summary>
    /// Événement invoqué lorsque le score change (nouveau score en paramètre).
    /// </summary>
    public event Action<int> OnScoreChanged;

    /// <summary>
    /// Événement invoqué lorsqu'on passe en état Game Over.
    /// </summary>
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetState();
    }

    private void ResetState()
    {
        IsGameOver = false;
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
            return;

        Score += Mathf.Max(0, amount);
        OnScoreChanged?.Invoke(Score);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        // Recharge la scène active et réinitialise l'état
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
        ResetState();
    }
}


