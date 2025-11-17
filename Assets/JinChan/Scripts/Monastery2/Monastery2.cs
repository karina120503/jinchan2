using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Monastery2 : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charMonk;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    public GameObject mainTextObject;
    public GameObject nextButton;

    [Header("Overlay & Audio")]
    public OverlayFade9 overlayFade;
public AudioSource bgMusic; // your music source

    [Header("Settings")]
    public float narrationSpeed = 0.01f;
    public float dialogueSpeed = 0.03f;
    public float fadeDuration = 2.5f;

    private int eventPos = 0;
    private string textToSpeak;

void Start()
{
    charName.gameObject.SetActive(false);
    splitter.SetActive(false);
    StartCoroutine(InitialFadeIn());
}

    #region UI Functions
    public void ShowNarration(string text)
    {
        charName.gameObject.SetActive(false);
        splitter.SetActive(false);
        narrationText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(false);

        narrationText.text = text;
    }

    public void ShowDialogue(string speaker, string text)
    {
        charName.gameObject.SetActive(true);
        splitter.SetActive(true);
        narrationText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(true);

        charName.text = speaker;
        dialogueText.text = text;
    }
    #endregion

    #region Fade Helpers
    IEnumerator FadeInRawImage(GameObject obj, float duration)
    {
        RawImage img = obj.GetComponent<RawImage>();
        if (img == null) yield break;

        Color c = img.color;
        c.a = 0f;
        img.color = c;
        obj.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration);
            img.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOutRawImage(GameObject obj, float duration)
    {
        RawImage img = obj.GetComponent<RawImage>();
        if (img == null) yield break;

        Color c = img.color;
        c.a = 1f;
        img.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1 - t / duration);
            img.color = c;
            yield return null;
        }

        obj.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        group.alpha = end;
    }

    IEnumerator FadeAudio(AudioSource audio, float startVol, float endVol, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
            yield return null;
        }
        audio.volume = endVol;
    }
    #endregion

    #region Typewriter
    IEnumerator TypewriterEffect(TMP_Text textComponent, string fullText, float delay)
    {
        textComponent.maxVisibleCharacters = 0;
        textComponent.text = fullText;
        for (int i = 1; i <= fullText.Length; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }
    #endregion

    #region Events


// Fade in from black at the very start
IEnumerator InitialFadeIn(float fadeDuration = 2.5f)
{
    if (fadeScreenIn == null) yield break;

    fadeScreenIn.SetActive(true);
    Image fadeImg = fadeScreenIn.GetComponent<Image>();
    if (fadeImg == null) yield break;

    fadeImg.color = new Color(0f, 0f, 0f, 1f); // fully black

    float elapsed = 0f;
    while (elapsed < fadeDuration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(1 - (elapsed / fadeDuration));
        fadeImg.color = new Color(0f, 0f, 0f, alpha);
        yield return null;
    }

    fadeImg.color = new Color(0f, 0f, 0f, 0f);
    fadeScreenIn.SetActive(false);

    // Now start your EventStarter
    StartCoroutine(EventStarter());
}

// Restored EventStarter coroutine
IEnumerator EventStarter()
{
    // initial brief pause before narration
    yield return new WaitForSeconds(0.5f);

    if (mainTextObject != null)
        mainTextObject.SetActive(true);

    // narration text
    textToSpeak = "The scent of ink still hung in the air. Broad brushstrokes shimmered faintly, not yet dried upon the paper. Jin Chan gently set the brush on its stand, beside the unrolled scroll. It felt as though everything inside had been poured into it — every thought, every memory, every fragment of guilt. Within him, as outside him — nothing remained. Toad flinched slightly at the foreign voice that broke the quiet space.";

    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.01f));  // narration fast

    nextButton.SetActive(true);
    eventPos = 1;
}


    IEnumerator EventOne()
    {
        nextButton.SetActive(false);
        ShowDialogue("Monk", "");
        StartCoroutine(FadeInRawImage(charMonk, 1f));
        overlayFade.FadeIn();

        textToSpeak = "\"<i>There is one last step to take...</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowNarration("");

        textToSpeak = "A wide sleeve moved gently through the air as the monk gestured upward, to where the cliffs vanished into clouds.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        nextButton.SetActive(true);
        eventPos = 3;
    }

    IEnumerator EventThree()
    {
        nextButton.SetActive(false);
        ShowDialogue("Monk", "");
        textToSpeak = "\"<i>“His dwelling lies where no stairs lead. But you know the way. Liu Hai awaits you.”</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

        nextButton.SetActive(true);
        eventPos = 4;
    }

IEnumerator EventFour()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);
    ShowNarration("");
    StartCoroutine(FadeOutRawImage(charMonk, 4f));
    overlayFade.FadeOut();

    textToSpeak = "Toad nodded. Thanking the monks, he stepped outside the hall, webbed paws treading the damp ground as the fog around him grew thicker and thicker. A white veil obscured his gaze like another membrane. And soon he realized: he was no longer walking on his own.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.01f)); // fast narration

    // Show the last Next button, but **do NOT load the scene yet**
    nextButton.SetActive(true);

    eventPos = 5; // mark as last event
}


IEnumerator FadeOutToBlackAndLoadScene(string sceneName, float duration = 2.5f)
{
    if (fadeScreenIn == null) yield break;

    fadeScreenIn.SetActive(true);
    Image fadeImg = fadeScreenIn.GetComponent<Image>();
    if (fadeImg == null) yield break;

    // Start transparent
    fadeImg.color = new Color(0f, 0f, 0f, 0f);

    float elapsed = 0f;
    float startVolume = bgMusic.volume; // get current BGM volume

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(elapsed / duration);
        fadeImg.color = new Color(0f, 0f, 0f, alpha);

        // Fade out BGM at the same time
        if (bgMusic != null)
        {
            bgMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
        }

        yield return null;
    }

    fadeImg.color = new Color(0f, 0f, 0f, 1f);

    // Stop music completely
    if (bgMusic != null) bgMusic.Stop();

    SceneManager.LoadScene(sceneName);
}


    #endregion

public void NextButton()
{
    switch (eventPos)
    {
        case 1: StartCoroutine(EventOne()); break;
        case 2: StartCoroutine(EventTwo()); break;
        case 3: StartCoroutine(EventThree()); break;
        case 4: StartCoroutine(EventFour()); break;
        case 5: StartCoroutine(FadeOutToBlackAndLoadScene("HeavenlyCourt")); break; // now triggered **after click**
    }
}

}
