using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseMenuUI : MonoBehaviour
{

     [Header("UI Elements")]
    public GameObject pauseMenuPanel; // ссылка на PauseMenu
    public Button pauseButton;        // верхняя кнопка
    public Button resumeButton;       
    public Button replayButton;              
    public Button mainMenuButton;     

    [Header("Audio")]
    public AudioSource musicSource;   // ссылка на музыку

    private bool isPaused = false;


    void Start()
    {
        pauseMenuPanel.SetActive(false);

        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(ResumeGame);
        replayButton.onClick.AddListener(ReplayGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        var song = FindFirstObjectByType<SongManager>();
        if (song != null)
        song.musicSource.Pause();

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        gm.isPaused = true;

        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        var song = FindFirstObjectByType<SongManager>();
        if (song != null)
        song.musicSource.UnPause();

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.isPaused = false;
        
        pauseMenuPanel.SetActive(false);
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Main Menu link in the comment");
        // SceneManager.LoadScene("MainMenu"); — потом заменишь на сцену меню
    }

    
}
