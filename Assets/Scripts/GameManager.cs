using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public SongManager songManager;
    public AudioSource musicSource;

    public static GameManager instance;
    public TMP_Text scoreText;

    public GameObject resultsPanel;
    public TMP_Text normalsText, goodText, perfectText, missedText;
    public TMP_Text percentHitText, rankText, finalScoreText;
    public Button nextLevelButton;
    public bool songEnded = false;
    public bool isPaused = false;


    private int _currentScore;
    public int currentScore
    {
        get => _currentScore;
        set
        {
            _currentScore = value;
            if (scoreText != null)
                scoreText.text = "Score: " + _currentScore;
        }
    }
    public int scorePerNote = 100;

    public int normalHits, goodHits, perfectHits, missedHits, totalNotes;

    void Start()
    {
        songManager.PlaySong();
    }
    void Awake()
    {
        instance = this;
    }

    public void SetTotalNotes(int count)
    {
    totalNotes = count;
    }

    void Update()
    {
    if (!isPaused && !FindFirstObjectByType<SongManager>().musicSource.isPlaying && !resultsPanel.activeInHierarchy)
        {
            ShowResults();
        }
    }

    public void NormalHit()
    {
        normalHits++;
        currentScore += scorePerNote;
        Debug.Log($"Normal hit. Score: {currentScore}");
    }

    public void GoodHit()
    {
        goodHits++;
        currentScore += scorePerNote * 2;
        Debug.Log($"Good hit. Score: {currentScore}");
    }

    public void PerfectHit()
    {
        perfectHits++;
        currentScore += scorePerNote * 3;
        Debug.Log($"Perfect hit. Score: {currentScore}");
    }

    public void NoteMissed()
    {
        missedHits++;
        Debug.Log($"Missed note.");
    }
    
    public float GetAccuracy()
    {
    float totalHit = normalHits + goodHits + perfectHits;
    return (totalNotes > 0) ? (totalHit / totalNotes) : 0f;
    }

    public void ShowResults()
    {
        resultsPanel.SetActive(true);

        normalsText.text = "Normal Hits: " + normalHits;
        goodText.text = "Good Hits: " + goodHits;
        perfectText.text = "Perfect Hits: " + perfectHits;
        missedText.text = "Missed: " + missedHits;

        float totalHit = normalHits + goodHits + perfectHits;
        float percentHit = (totalNotes > 0) ? (totalHit / totalNotes) * 100f : 0f;
        percentHitText.text = "Accuracy: " + percentHit.ToString("F1") + "%";

        string rankVal = "F";
        if (percentHit > 40) rankVal = "D";
        if (percentHit > 55) rankVal = "C";
        if (percentHit > 70) rankVal = "B";
        if (percentHit > 85) rankVal = "A";
        if (percentHit > 95) rankVal = "S";

        rankText.text = "Rank: " + rankVal;
        finalScoreText.text = "Score: " + currentScore;
    }
    
    public void GoToNextLevel()
    {
        Debug.Log("Next Level placeholder, see in the comments");
        // SceneManager.LoadScene("PoisonedVillage2"); // ← потом вставишь реальное имя сцены
    }

}
