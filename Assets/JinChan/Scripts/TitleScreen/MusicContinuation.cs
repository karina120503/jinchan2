using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicContinuation : MonoBehaviour
{
    // Singleton instance accessible from other scripts
public static MusicContinuation instance { get; private set; }


    private AudioSource audioSource;

    [SerializeField] private float fadeDuration = 2f; // seconds to fade out

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.playOnAwake = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // List of all chapter scenes
        string[] chapterScenes = new string[]
        {
            "StartNarration",
            "GhostMarket",
            "TestCandy",
            "GhostMarket2",
            "PoisonedVillage",
            "RhytmGame",
            "PoisonedVillage2",
            "CalligraphyMonastery",
            "Main",
            "Monastery2",
            "HeavenlyCourt"
        };

        bool isChapter = false;
        foreach (string chapter in chapterScenes)
        {
            if (scene.name == chapter)
            {
                isChapter = true;
                break;
            }
        }

        if (isChapter)
        {
            // Fade out TitleScreen music when entering a chapter
            if (audioSource != null)
                StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // Continue playing in MapMenu or TitleScreen
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public IEnumerator FadeInMusic(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        Destroy(gameObject);
    }
}

