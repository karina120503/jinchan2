using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager current; // singleton reference
    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    public float fadeDuration = 3f;   // adjust as you like
    private bool isMuted = false;

void Awake()
{
if (current != null && current != this && current.GetType().Name != "MusicContinuation")
{
    StartCoroutine(FadeOutAndDestroy(current));
}



    current = this;

    if (audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();

    audioSource.loop = true;
    audioSource.playOnAwake = false;
    audioSource.clip = backgroundMusic;

    DontDestroyOnLoad(gameObject); // keeps music alive across scenes

    // start silent, then fade in
    audioSource.volume = 0f;
    audioSource.Play();
    StartCoroutine(FadeIn());
}



    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1f;
    }

    private IEnumerator FadeOutAndDestroy(MusicManager oldManager)
    {
        AudioSource oldSource = oldManager.audioSource;
        float startVolume = oldSource.volume;
        float elapsed = 0f;

        while (elapsed < oldManager.fadeDuration)
        {
            elapsed += Time.deltaTime;
            oldSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / oldManager.fadeDuration);
            yield return null;
        }

        oldSource.Stop();
        Destroy(oldManager.gameObject);
    }

public void PlayMusic(AudioClip newClip)
{
    if (audioSource.clip == newClip) return; // already playing
    StartCoroutine(SwitchTrack(newClip));
}

private IEnumerator SwitchTrack(AudioClip newClip)
{
    // Fade out current music
    float startVolume = audioSource.volume;
    float elapsed = 0f;

    while (elapsed < fadeDuration)
    {
        elapsed += Time.unscaledDeltaTime;
        audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
        yield return null;
    }

    audioSource.Stop();
    audioSource.clip = newClip;
    audioSource.Play();

    // Fade in
    elapsed = 0f;
    while (elapsed < fadeDuration)
    {
        elapsed += Time.unscaledDeltaTime;
        audioSource.volume = Mathf.Lerp(0f, startVolume, elapsed / fadeDuration);
        yield return null;
    }

    audioSource.volume = startVolume;
}

    public void ToggleMute()
    {
        isMuted = !isMuted;
        audioSource.mute = isMuted;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

public void PlayMapMenuMusic(AudioClip mapMenuClip)
{
    if (audioSource.clip == mapMenuClip && audioSource.isPlaying)
        return;

    StartCoroutine(SwitchTrack(mapMenuClip));
}

}
