using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoisonedVillage2 : MonoBehaviour
{
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charElder;
    public OverlayFade8 overlayFade;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;
    [SerializeField] AudioSource lightRain;

    // Speed controls (as you requested)
    public float narrationSpeed = 0.01f;  // faster running text
    public float dialogueSpeed = 0.03f;   // dialogue stays same

    // Optional background music (will be faded out on scene transition if assigned)
    public AudioSource bgMusic; // assign the background music AudioSource

    void Start()
    {
        charName.gameObject.SetActive(false);
        splitter.SetActive(false);
        StartCoroutine(EventStarter());
    }

    void Update()
    {
        textLength = TextCreator.charCount;
    }

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

    IEnumerator FadeInRawImage(GameObject obj, float duration)
    {
        RawImage raw = obj.GetComponent<RawImage>();
        if (raw == null) yield break;

        Color c = raw.color;
        c.a = 0f;
        raw.color = c;
        obj.SetActive(true);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(time / duration);
            raw.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOutRawImage(GameObject obj, float duration)
    {
        RawImage raw = obj.GetComponent<RawImage>();
        if (raw == null) yield break;

        Color c = raw.color;
        c.a = 1f;
        raw.color = c;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(1 - (time / duration));
            raw.color = c;
            yield return null;
        }

        obj.SetActive(false);
    }

    IEnumerator CrossfadeBackground(Sprite newSprite, float duration)
    {
        if (backgroundOverlayImage == null || backgroundImage == null) yield break;

        backgroundOverlayImage.sprite = newSprite;
        Color overlayColor = backgroundOverlayImage.color;
        overlayColor.a = 0f;
        backgroundOverlayImage.color = overlayColor;
        backgroundOverlayImage.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duration);
            overlayColor.a = alpha;
            backgroundOverlayImage.color = overlayColor;
            yield return null;
        }

        backgroundImage.sprite = newSprite;
        backgroundOverlayImage.gameObject.SetActive(false);
    }

    IEnumerator TypewriterEffect(
        TMP_Text textComponent,
        string fullText,
        float delay,
        string triggerPhrase1 = "", GameObject objectToActivate1 = null, float fadeDuration1 = 1f,
        string triggerPhrase2 = "", GameObject objectToActivate2 = null, float fadeDuration2 = 1f)
    {
        textComponent.maxVisibleCharacters = 0;
        textComponent.text = fullText;

        for (int i = 1; i <= fullText.Length; i++)
        {
            textComponent.maxVisibleCharacters = i;

            if (!string.IsNullOrEmpty(triggerPhrase1) && objectToActivate1 != null)
            {
                int idx = fullText.IndexOf(triggerPhrase1);
                if (idx >= 0 && i >= idx && !objectToActivate1.activeSelf)
                {
                    StartCoroutine(FadeInRawImage(objectToActivate1, fadeDuration1));
                }
            }

            if (!string.IsNullOrEmpty(triggerPhrase2) && objectToActivate2 != null)
            {
                int idx2 = fullText.IndexOf(triggerPhrase2);
                if (idx2 >= 0 && i >= idx2 && !objectToActivate2.activeSelf)
                {
                    StartCoroutine(FadeInRawImage(objectToActivate2, fadeDuration2));
                }
            }

            yield return new WaitForSeconds(delay);
        }

        textLength = fullText.Length;
    }

    // Helpers that use the public speed variables
    IEnumerator RunNarration(TMP_Text textComp, string text,
                             string triggerPhrase1 = "", GameObject objectToActivate1 = null, float fadeDuration1 = 1f,
                             string triggerPhrase2 = "", GameObject objectToActivate2 = null, float fadeDuration2 = 1f)
    {
        yield return StartCoroutine(TypewriterEffect(textComp, text, narrationSpeed,
                                                     triggerPhrase1, objectToActivate1, fadeDuration1,
                                                     triggerPhrase2, objectToActivate2, fadeDuration2));
    }

    IEnumerator RunDialogue(TMP_Text textComp, string text,
                            string triggerPhrase1 = "", GameObject objectToActivate1 = null, float fadeDuration1 = 1f,
                            string triggerPhrase2 = "", GameObject objectToActivate2 = null, float fadeDuration2 = 1f)
    {
        yield return StartCoroutine(TypewriterEffect(textComp, text, dialogueSpeed,
                                                     triggerPhrase1, objectToActivate1, fadeDuration1,
                                                     triggerPhrase2, objectToActivate2, fadeDuration2));
    }

    void SetSpeaker(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            charName.text = name;
            charName.gameObject.SetActive(true);
            splitter.SetActive(true);
        }
        else
        {
            charName.text = "";
            charName.gameObject.SetActive(false);
            splitter.SetActive(false);
        }
    }

    // Fade out audio (with null check and reset like you posted)
    IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // reset if needed
    }

    // Fade-to-black that uses RawImage where possible and falls back to Image
    IEnumerator FadeToBlackAndHold(float duration, float hold = 0.5f)
    {
        if (fadeScreenIn == null)
        {
            yield return new WaitForSeconds(duration + hold);
            yield break;
        }

        // Try RawImage first
        RawImage raw = fadeScreenIn.GetComponent<RawImage>();
        if (raw != null)
        {
            Color c = raw.color;
            c.a = 0f;
            raw.color = c;
            fadeScreenIn.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                raw.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }

            raw.color = new Color(c.r, c.g, c.b, 1f);
            yield return new WaitForSeconds(hold);
            yield break;
        }

        // Fallback to Image
        Image img = fadeScreenIn.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
            fadeScreenIn.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                img.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }

            img.color = new Color(c.r, c.g, c.b, 1f);
            yield return new WaitForSeconds(hold);
            yield break;
        }

        // If no RawImage or Image, just wait
        yield return new WaitForSeconds(duration + hold);
    }

IEnumerator FadeFromBlack(float duration)
{
    if (fadeScreenIn == null) yield break;

    fadeScreenIn.SetActive(true);
    RawImage raw = fadeScreenIn.GetComponent<RawImage>();
    if (raw == null) yield break;

    Color c = raw.color;
    c.a = 1f; // start fully black
    raw.color = c;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (elapsed / duration));
        raw.color = new Color(c.r, c.g, c.b, alpha);
        yield return null;
    }

    raw.color = new Color(c.r, c.g, c.b, 0f);
    fadeScreenIn.SetActive(false); // hide after fade-out
}


    // ---------- Events (kept content & order intact) ----------
IEnumerator EventStarter()
{
    // start fully black
    if (fadeScreenIn != null)
    {
        RawImage raw = fadeScreenIn.GetComponent<RawImage>();
        if (raw != null) raw.color = new Color(0f,0f,0f,1f);
        fadeScreenIn.SetActive(true);
        yield return StartCoroutine(FadeFromBlack(1.8f)); // fade in lebih lambat

    }

    if (lightRain != null) lightRain.Play();
    if (mainTextObject != null) mainTextObject.SetActive(true);

    textToSpeak = "The ritual was complete. Jin Chan opened his eyes. ...";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    if (nextButton != null) nextButton.SetActive(true);
    eventPos = 1;
}

    IEnumerator EventOne()
    {
        // event 13
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        overlayFade?.FadeIn();
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>The rain will wash away the last of the dark energy. After the rain, flowers will bloom.</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak, "The rain", charJinChan, 1f));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        // event 2
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        overlayFade?.FadeOut();
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        ShowNarration("");
        textToSpeak = "The villagers, who had watched the ritual in silence, now looked up. Something in their faces had changed. Color had returned to their cheeks. Eyes once hollow with despair now glistened. Shoulders once curled by humility now straightened.";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 3;
    }

    IEnumerator EventThree()
    {
        // event 3
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        overlayFade?.FadeIn();
        StartCoroutine(FadeInRawImage(charElder, 1f));
        ShowDialogue("Elder Man", "");
        textToSpeak = "\"<i>I will wait for them.</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak, "I will", charElder, 1f));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 4;
    }

    IEnumerator EventFour()
    {
        // event 4
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charElder, 1f));
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowNarration("");
        textToSpeak = "A toad felt a gentle tug on his heart. A small dream. He wished Liu Hai could see them too.";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 5;
    }

    IEnumerator EventFive()
    {
        // event 17
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>Have you heard of Liu Hai?</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 6;
    }

    IEnumerator EventSix()
    {
        // event 18
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        StartCoroutine(FadeInRawImage(charElder, 1f));
        ShowNarration("");
        textToSpeak = "The man looked off into the distance for a moment.";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 7;
    }

    IEnumerator EventSeven()
    {
        // event 19
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        ShowDialogue("Elder Man", "");
        textToSpeak = "\"<i>Liu Hai came here many years ago. Back when this village was still breathing. He studied in the mountains, there is a monastery.</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 8;
    }

    IEnumerator EventEight()
    {
        // event 8
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charElder, 3f));
        ShowNarration("");
        textToSpeak = "Then that is where Jin Chan would go.";
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 9;
    }

    IEnumerator EventNine()
    {
        // event 21
        if (nextButton != null) nextButton.SetActive(false);
        if (textBox != null) textBox.SetActive(true);
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>Thank you.</i>\"";
        yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));
        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 10;
    }

IEnumerator EventTen()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    // Tampilkan narasi
    ShowNarration("");
    textToSpeak = "The elder said nothing more. He simply handed the bamboo umbrella to Jin Chan. The rain was already beginning to fade. It was time to move on.";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    // Fade audio
    if (bgMusic != null) yield return StartCoroutine(FadeOutAudio(bgMusic, 1f));
    if (lightRain != null) yield return StartCoroutine(FadeOutAudio(lightRain, 1f));

    // Langsung fade ke black tanpa delay
    yield return StartCoroutine(FadeToBlackAndHold(1f, 0f)); // fade 1 detik, hold 0 detik

    // Hide semua objek
    textBox.SetActive(false);
    charName.gameObject.SetActive(false);
    splitter.SetActive(false);
    charJinChan.SetActive(false);
    charElder.SetActive(false);
    backgroundImage.gameObject.SetActive(false);
    backgroundOverlayImage.gameObject.SetActive(false);

    // Load scene baru
    SceneManager.LoadScene("CalligraphyMonastery");
}


    // ---------- NextButton (clean switch) ----------
    public void NextButton()
    {
        switch (eventPos)
        {
            case 1: StartCoroutine(EventOne()); break;
            case 2: StartCoroutine(EventTwo()); break;
            case 3: StartCoroutine(EventThree()); break;
            case 4: StartCoroutine(EventFour()); break;
            case 5: StartCoroutine(EventFive()); break;
            case 6: StartCoroutine(EventSix()); break;
            case 7: StartCoroutine(EventSeven()); break;
            case 8: StartCoroutine(EventEight()); break;
            case 9: StartCoroutine(EventNine()); break;
            case 10: StartCoroutine(EventTen()); break;
            default: break;
        }
    }
}
