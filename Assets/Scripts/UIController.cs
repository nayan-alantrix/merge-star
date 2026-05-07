using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button          pauseButton;

    [Header("Game Over Panel")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private Button          gameOverRestartButton;
    [SerializeField] private Button          gameOverQuitButton;

    [Header("Pause Panel")]
    [SerializeField] private TextMeshProUGUI pauseScoreText;
    [SerializeField] private Button          pauseResumeButton;
    [SerializeField] private Button          pauseRestartButton;
    [SerializeField] private Button          pauseQuitButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Wrap each button listener to also play click sound

        pauseButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.TogglePause();
        });

        gameOverRestartButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.RestartGame();
        });

        gameOverQuitButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.QuitGame();
        });

        pauseResumeButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.ResumeGame();
        });

        pauseRestartButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.RestartGame();
        });

        pauseQuitButton.onClick.AddListener(() => {
            AudioService.Instance.PlaySFX(AudioType.ButtonClick);
            GameManager.Instance.QuitGame();
        });

        // Listen to score events
        ScoreManager.Instance.OnScoreChanged     += UpdateScore;
        ScoreManager.Instance.OnHighScoreChanged += UpdateHighScore;
        ScoreManager.Instance.OnComboText        += ShowComboText;

        // Init display
        UpdateScore(ScoreManager.Instance.CurrentScore);
        UpdateHighScore(ScoreManager.Instance.HighScore);
    }

    void OnDestroy()
    {
        ScoreManager.Instance.OnScoreChanged     -= UpdateScore;
        ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScore;
        ScoreManager.Instance.OnComboText        -= ShowComboText;
    }

    // -------------------------------------------------------
    // HUD
    // -------------------------------------------------------

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString("N0");

        // Keep pause panel score in sync too
        if (pauseScoreText != null)
            pauseScoreText.text = $"Score: {score:N0}";
    }

    private void UpdateHighScore(int score)
    {
        if (highScoreText != null)
            highScoreText.text = score.ToString("N0");
    }

    // -------------------------------------------------------
    // Game Over
    // -------------------------------------------------------

    public void ShowGameOver()
    {
        if (gameOverScoreText != null)
            gameOverScoreText.text = ScoreManager.Instance.CurrentScore.ToString("N0");

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = ScoreManager.Instance.HighScore.ToString("N0");
    }

    // -------------------------------------------------------
    // Combo Text
    // -------------------------------------------------------

    private void ShowComboText(string text)
    {
        // Hook this up to an animated TMP label if you have one
        Debug.Log($"COMBO: {text}");
    }
}