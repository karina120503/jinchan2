using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuPanel; // ссылка на PauseMenu
    public Button pauseButton;        // верхняя кнопка
    public Button replayButton;
    public Button mainMenuButton;
    public Button muteButton;
    public Button resumeButton;

      [Header("Mute Button Icons")] 
    public Sprite soundOnIcon;     // иконка "звук включён"
    public Sprite soundOffIcon;    // иконка "звук выключен"

    private bool isMuted = false;
    private bool isPaused = false; 
    public MusicManager musicManager;
    private Image muteButtonImage; 

    void Start()
    {   // Назначаем события
        pauseButton.onClick.AddListener(OpenMenu);
        replayButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        muteButton.onClick.AddListener(ToggleMute);
        resumeButton.onClick.AddListener(CloseMenu);

        muteButtonImage = muteButton.GetComponent<Image>();

        if (muteButtonImage != null && soundOnIcon != null)
            muteButtonImage.sprite = soundOnIcon;

        // Убедимся, что меню скрыто
        pauseMenuPanel.SetActive(false);
    }

    void OpenMenu()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // ставим игру на паузу
        isPaused = true;
    }

    void CloseMenu()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // возвращаем время
        isPaused = false;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

void ReturnToMainMenu()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("MapMenu");

    // Resume TitleScreen music if it exists
    if (MusicContinuation.instance != null && MusicContinuation.instance.GetAudioSource() != null)
    {
        AudioSource music = MusicContinuation.instance.GetAudioSource();

        // Make sure it's playing
        if (!music.isPlaying)
            music.Play();

    }
}




    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (musicManager != null)
            musicManager.ToggleMute();

        if (muteButtonImage != null)
        {
            muteButtonImage.sprite = isMuted ? soundOffIcon : soundOnIcon;
        }
    }

    public void ShowDeathMenu()
    {
        pauseMenuPanel.SetActive(true);
        pauseButton.gameObject.SetActive(false); // убираем кнопку паузы
        resumeButton.gameObject.SetActive(false); // убираем "вернуться в игру"
        Time.timeScale = 0f;
    }

}
