using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public int scoreMultiplier = 10;
    public int targetScore = 0;
    public UnityEvent onTargetScoreReached;

    private float baseHeight;
    private float highestY;
    private int score;
    private int bestScore;

    public LevelGenerator levelGenerator;

    void Start()
    {
        if (player == null) Debug.LogError("ScoreManagerTMP: player не назначен!");
        baseHeight = player != null ? player.position.y : 0f;
        highestY = baseHeight;
        score = 0;
        bestScore = PlayerPrefs.GetInt("HighScore", 0);
        if (bestScoreText != null) bestScoreText.text = "Best: " + bestScore;
        UpdateScoreText();
    }

    void Update()
    {
        if (player == null) return;
        if (player.position.y > highestY)
        {
            highestY = player.position.y;
            int newScore = Mathf.FloorToInt((highestY - baseHeight) * scoreMultiplier);
            if (newScore != score)
            {
                score = newScore;
                UpdateScoreText();
                if (score > bestScore)
                {
                    bestScore = score;
                    PlayerPrefs.SetInt("HighScore", bestScore);
                    if (bestScoreText != null) bestScoreText.text = "Best: " + bestScore;
                }

                if (targetScore > 0 && score >= targetScore)
                {
                    if (onTargetScoreReached != null) onTargetScoreReached.Invoke();
                    targetScore = 0;

                    if (levelGenerator != null)
                    {
                    levelGenerator.StopGenerationAndSpawnFinal();
                    }
                }
            }
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public int GetCurrentScore()
    {
        return score;
    }
}
