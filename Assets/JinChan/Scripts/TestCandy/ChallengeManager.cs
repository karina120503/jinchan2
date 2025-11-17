using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance { get; private set; }

    [SerializeField] private int startingMoves = 10;
    [SerializeField] private int targetScore = 100;

    [Header("UI")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text targetText;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;

    public int MovesLeft { get; private set; }
    public int TargetScore => targetScore;
    public bool IsOver { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        MovesLeft = startingMoves;
        RefreshUI();

        // Hide result panel on start
        if (resultPanel != null) resultPanel.SetActive(false);

        // Hook up buttons
        if (nextButton != null) nextButton.onClick.AddListener(LoadNextScene);
        if (retryButton != null) retryButton.onClick.AddListener(ReloadScene);
    }

    public void RegisterMove()
    {
        if (IsOver) return;

        MovesLeft--;
        RefreshUI();

        CheckWinLose();
    }

    private void RefreshUI()
    {
        if (movesText != null) movesText.text = $"Moves: {MovesLeft}";
        if (targetText != null) targetText.text = $"Target: {targetScore}";
    }

    private void CheckWinLose()
    {
        if (ScoreCounter.Instance.Score >= targetScore)
            Win();
        else if (MovesLeft <= 0)
            Lose();
    }

    private void Win()
    {
        IsOver = true;
        ShowResult("You Win!", showNext: true);
    }

    private void Lose()
    {
        IsOver = true;
        ShowResult("You Lose!", showNext: false);
    }

    private void ShowResult(string message, bool showNext)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = message;

        if (nextButton != null) nextButton.gameObject.SetActive(showNext);
        if (retryButton != null) retryButton.gameObject.SetActive(!showNext);
    }

private void LoadNextScene()
{
    // Try to find your background music AudioSource in the scene
AudioSource bgMusic = FindFirstObjectByType<AudioSource>();

    if (bgMusic != null)
    {
        StartCoroutine(FadeOutAndLoadNextScene(bgMusic, 2f)); // 2 seconds fade
    }
    else
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}


    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

private IEnumerator FadeOutAndLoadNextScene(AudioSource bgMusic, float duration)
{
    if (bgMusic != null)
    {
        float startVolume = bgMusic.volume;

        while (bgMusic.volume > 0f)
        {
            bgMusic.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgMusic.Stop();
        bgMusic.volume = startVolume;
    }

    yield return new WaitForSeconds(0.3f); // small pause before transition
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
}

}
