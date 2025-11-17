using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GhostMarket2 : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charVillageGirl;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    public GameObject mainTextObject;
    public GameObject nextButton;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource softRustle;
    public AudioSource whisperStirs;

    [Header("Overlay & Fade")]
    public OverlayFade7 overlayFade;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;

    [Header("Text Speeds")]
    public float narrationSpeed = 0.01f;
    public float dialogueSpeed = 0.03f;

    private string textToSpeak;
    private int eventPos = 0;

    // ============================ //
    // ==== INITIAL SETUP ========= //
    // ============================ //
    void Start()
    {
        charName.gameObject.SetActive(false);
        splitter.SetActive(false);
        StartCoroutine(SceneStartSequence());
    }

    IEnumerator SceneStartSequence()
    {
        // Start full black
        fadeScreenIn.SetActive(true);
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        if (fadeImg != null)
            fadeImg.color = new Color(0f, 0f, 0f, 1f);

        // Fade in BGM smoothly
        if (bgmSource != null)
        {
            bgmSource.volume = 0f;
            bgmSource.Play();
            StartCoroutine(FadeInAudio(bgmSource, 2f));
        }

        // Fade from black
        yield return StartCoroutine(FadeFromBlack(2.5f));

        // Continue to story
        StartCoroutine(EventStarter());
    }

    // ============================ //
    // ==== GENERAL HELPERS ======= //
    // ============================ //

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

    IEnumerator FadeFromBlack(float duration)
    {
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        if (fadeImg == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.SmoothStep(1f, 0f, elapsed / duration);
            fadeImg.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        fadeScreenIn.SetActive(false);
    }

    IEnumerator FadeToBlack(float duration)
    {
        fadeScreenIn.SetActive(true);
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        if (fadeImg == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            fadeImg.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImg.color = new Color(0f, 0f, 0f, 1f);
    }

    IEnumerator FadeInAudio(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float elapsed = 0f;
        source.volume = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        source.volume = 1f;
    }

    IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

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
            c.a = Mathf.Clamp01(1f - (t / duration));
            img.color = c;
            yield return null;
        }

        obj.SetActive(false);
    }

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

    IEnumerator RunNarration(TMP_Text textComponent, string text)
    {
        yield return StartCoroutine(TypewriterEffect(textComponent, text, narrationSpeed));
    }

    IEnumerator RunDialogue(TMP_Text textComponent, string text)
    {
        yield return StartCoroutine(TypewriterEffect(textComponent, text, dialogueSpeed));
    }

    // ============================ //
    // ======== SCENE EVENTS ====== //
    // ============================ //

    IEnumerator EventStarter()
    {
        mainTextObject.SetActive(true);
        ShowNarration("");

        textToSpeak = "Jin Chan gently placed the last stall back in place. The planks creaked beneath his webbed feet, then glowed faintly with soft light — the market, for the first time in years, seemed to breathe again. But just as he felt the stirrings of relief, he sensed it. A small hand tugged at the hem of his robe.";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

        nextButton.SetActive(true);
        eventPos = 1;
    }

    IEnumerator EventOne()
    {
        nextButton.SetActive(false);
        ShowNarration("");
        StartCoroutine(FadeInRawImage(charVillageGirl, 3f));
        overlayFade.FadeIn();

        if (softRustle != null)
        {
            softRustle.loop = false;
            softRustle.Play();
        }

        textToSpeak = "He turned his head. A tiny figure stood before him. A little girl — alive, but nearly translucent from weakness. Her lips were cracked with thirst, her cheeks hollow with hunger...";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        nextButton.SetActive(false);
        ShowDialogue("Girl", "");

        textToSpeak = "\"<i>Lost. Home… can’t walk. Village… past forest. I'm so scared…</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));

        nextButton.SetActive(true);
        eventPos = 3;
    }


    IEnumerator EventThree()
    {
        nextButton.SetActive(false);
        ShowNarration("");

        StartCoroutine(FadeOutRawImage(charVillageGirl, 2f));
        overlayFade.FadeOut();

        textToSpeak = "Then she fell silent. She would have cried if she had any strength left...";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

        nextButton.SetActive(true);
        eventPos = 3; // mark as ready for fade-on-click
    }

    // <-- This is where number 3 goes:
    IEnumerator FadeAndLoadNextScene()
    {
        nextButton.SetActive(false);

        // Fade out music
        if (bgmSource != null)
            StartCoroutine(FadeOutAudio(bgmSource, 3f));

        // Fade to black
        yield return StartCoroutine(FadeToBlack(2f));

        // Load next scene
        SceneManager.LoadScene("PoisonedVillage");
    }

    public void NextButton()
    {
        if (eventPos == 1) StartCoroutine(EventOne());
        else if (eventPos == 2) StartCoroutine(EventTwo());
        else if (eventPos == 3)
        {
            // fade + scene transition now happens on click
            StartCoroutine(FadeAndLoadNextScene());
        }
    }

    // ... rest of your code ...
}