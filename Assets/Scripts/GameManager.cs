using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenuPanel;

    public bool IsGameOver { get; private set; } = false;
    public bool IsPaused   { get; private set; } = false;

    void Awake()
    {
        Instance = this;
        gameOverPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
    }

    // -------------------------------------------------------
    // Game Over
    // -------------------------------------------------------

    public void OnGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        PlayerPrefs.Save();
        gameOverPanel.SetActive(true);
    }

    // -------------------------------------------------------
    // Pause
    // -------------------------------------------------------

    public void PauseGame()
    {
        if (IsGameOver) return;

        IsPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else          PauseGame();
    }

    // -------------------------------------------------------
    // Restart
    // -------------------------------------------------------

    public void RestartGame()
    {
        IsGameOver     = false;
        IsPaused       = false;
        Time.timeScale = 1f;

        ScoreManager.Instance.ResetScore();

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // -------------------------------------------------------
    // Quit
    // -------------------------------------------------------

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}