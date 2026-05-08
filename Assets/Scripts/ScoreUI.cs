using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged     += UpdateScore;
        ScoreManager.Instance.OnHighScoreChanged += UpdateHighScore;
    }

    void OnDisable()
    {
        ScoreManager.Instance.OnScoreChanged     -= UpdateScore;
        ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScore;
    }

    void Start()
    {
        UpdateScore(ScoreManager.Instance.CurrentScore);
        UpdateHighScore(ScoreManager.Instance.HighScore);
    }

    private void UpdateScore(int score)     => scoreText.text     = score.ToString();
    private void UpdateHighScore(int score) => highScoreText.text = score.ToString();
}