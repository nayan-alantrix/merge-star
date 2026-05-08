using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    // UI Events
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;
    public event Action<string> OnComboText;

    private const string HighScoreKey = "HighScore";

    // Combo / Chain
    private int chainCount = 0;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    /// <summary>
    /// Call when a new block is placed.
    /// Resets combo chain.
    /// </summary>
    public void ResetChain()
    {
        chainCount = 0;
    }

    /// <summary>
    /// Call after every successful merge/pop.
    /// </summary>
    public void AddMergeScore(int groupSize, TileType tileType)
    {
        chainCount++;

        int baseScore      = GetBaseScore(tileType);
        int groupBonus     = GetGroupBonus(groupSize);
        int chainMultiplier = GetChainMultiplier(chainCount);

        // Final Formula
        int totalScore =
            (baseScore * groupSize + groupBonus)
            * chainMultiplier;

        CurrentScore += totalScore;

        // Combo text
        ShowComboText(groupSize, chainCount);

        Debug.Log(
            $"Merge Score: {totalScore} | " +
            $"Base:{baseScore} " +
            $"Group:{groupSize} " +
            $"Bonus:{groupBonus} " +
            $"Chain:x{chainMultiplier} " +
            $"Total:{CurrentScore}"
        );

        CheckHighScore();

        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>
    /// Bomb scoring
    /// </summary>
    public void AddBombScore(int tilesCleared)
    {
        int score = tilesCleared * 75;

        CurrentScore += score;

        Debug.Log(
            $"Bomb Clear: {tilesCleared} tiles | " +
            $"Score:{score} | Total:{CurrentScore}"
        );

        CheckHighScore();

        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>
    /// Reset everything
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
        chainCount = 0;

        OnScoreChanged?.Invoke(CurrentScore);
    }

    // ======================================================
    // SCORE TABLES
    // ======================================================

    private int GetBaseScore(TileType type)
    {
        return type switch
        {
            TileType.Star_1 => 10,
            TileType.Star_2 => 20,
            TileType.Star_3 => 40,
            TileType.Star_4 => 70,
            TileType.Star_5 => 120,
            TileType.Star_6 => 200,

            TileType.Boom_Block => 150,

            _ => 10
        };
    }

    /// <summary>
    /// Extra reward for bigger groups
    /// </summary>
    private int GetGroupBonus(int groupSize)
    {
        return groupSize switch
        {
            3 => 0,
            4 => 20,
            5 => 50,
            6 => 90,
            7 => 140,
            _ => 200 + ((groupSize - 8) * 50)
        };
    }

    /// <summary>
    /// Combo multiplier
    /// </summary>
    private int GetChainMultiplier(int chain)
    {
        return chain switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 4,
            _ => 5
        };
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private void CheckHighScore()
    {
        if (CurrentScore <= HighScore)
            return;

        HighScore = CurrentScore;

        PlayerPrefs.SetInt(HighScoreKey, HighScore);
        PlayerPrefs.Save();

        OnHighScoreChanged?.Invoke(HighScore);
    }

    private void ShowComboText(int groupSize, int chain)
    {
        string comboText = "";

        // Chain text
        if (chain >= 2)
        {
            comboText = chain switch
            {
                2 => "COMBO x2!",
                3 => "SUPER COMBO!",
                4 => "MEGA COMBO!",
                _ => "INSANE COMBO!"
            };
        }

        // Big merge text
        if (groupSize >= 6)
        {
            comboText += " MASSIVE MERGE!";
        }

        if (!string.IsNullOrEmpty(comboText))
        {
            Debug.Log(comboText);
            OnComboText?.Invoke(comboText);
        }
    }
}