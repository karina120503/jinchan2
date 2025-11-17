using UnityEngine.UI;
using System.Collections;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostMarket : MonoBehaviour
{

    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charVillageGirl;
    public OverlayFade6 overlayFade;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    public GameObject chapterTitle;

// Cinematic timing settings (used in synchronized fade)
public float blackHoldTime = 1.5f;         // Black screen fully opaque before fading
public float blackFadeDuration = 2.5f;     // Duration for black screen fade
public float titleFadeInDuration = 2.5f;   // Duration for chapter title fade-in
public float titleDisplayTime = 3f;        // Chapter title fully visible
public float titleFadeOutDuration = 2.5f;  // Duration for chapter title fade-out
public float narrationSpeed = 0.01f; // fast running text
public float dialogueSpeed = 0.03f;  // normal speed


    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;
    [SerializeField] private Sprite background2;
    [SerializeField] AudioSource whisperStirs;
[SerializeField] AudioSource bgMusic;

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

void Start()
{
    charName.gameObject.SetActive(false);
    splitter.SetActive(false);
    StartCoroutine(EventStarter());
}

void Update()
{
    // Keep your old update code
    textLength = TextCreator.charCount;

}


    IEnumerator FadeInRawImage(GameObject obj, float duration)
    {
        RawImage img = obj.GetComponent<RawImage>();
        if (img == null) yield break; // No RawImage found

        Color c = img.color;
        c.a = 0f;
        img.color = c;
        obj.SetActive(true);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(time / duration);
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

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(1 - (time / duration));
            img.color = c;
            yield return null;
        }

        obj.SetActive(false);
    }

IEnumerator CrossfadeBackground(Sprite newSprite, float duration)
{
    // Put the new sprite into overlay and set alpha = 0
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

        // Fade in overlay
        overlayColor.a = alpha;
        backgroundOverlayImage.color = overlayColor;

        yield return null;
    }

    // After fade, copy overlay sprite into main background
    backgroundImage.sprite = newSprite;

    // Hide overlay again
    backgroundOverlayImage.gameObject.SetActive(false);
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

IEnumerator GlowPulse(Image img, float pulseSpeed = 2f, float maxAlphaStrength = 0.5f, float fadeInTime = 1f)
{
    Color originalColor = img.color;

    // 1️⃣ Gradually increase glow to sync with fade-in
    float t = 0f;
    while (t < fadeInTime)
    {
        t += Time.deltaTime;
        float alpha = Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, t / fadeInTime);
        img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        yield return null;
    }

    // 2️⃣ Continue normal pulsing after fade-in
    while (img.gameObject.activeSelf)
    {
        float glow = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0 → 1
        img.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                              Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, glow));
        yield return null;
    }

    // Restore original alpha when done
    img.color = originalColor;
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

            // First trigger
            if (!string.IsNullOrEmpty(triggerPhrase1) && objectToActivate1 != null)
            {
                if (i >= fullText.IndexOf(triggerPhrase1) && !objectToActivate1.activeSelf)
                {
                    StartCoroutine(FadeInRawImage(objectToActivate1, fadeDuration1));
                }
            }

            // Second trigger
            if (!string.IsNullOrEmpty(triggerPhrase2) && objectToActivate2 != null)
            {
                if (i >= fullText.IndexOf(triggerPhrase2) && !objectToActivate2.activeSelf)
                {
                    StartCoroutine(FadeInRawImage(objectToActivate2, fadeDuration2));
                }
            }

            yield return new WaitForSeconds(delay);
        }

        textLength = fullText.Length;
    }

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


    IEnumerator TypewriterEffectParagraphs(TMP_Text textComponent, string fullText, float delay, float sentencePause)
    {
        textComponent.text = "";
        string[] sentences = fullText.Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int s = 0; s < sentences.Length; s++)
        {
            string sentence = sentences[s].Trim();

            // Add back the period if missing
            if (!sentence.EndsWith(".")) sentence += ".";

            foreach (char c in sentence)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(delay);
            }

            // Only add newline if it’s not the last sentence
            if (s < sentences.Length - 1)
            {
                textComponent.text += "\n";
                yield return new WaitForSeconds(sentencePause);
            }
        }
    }

void SetSpeaker(string name)
{
    if (!string.IsNullOrEmpty(name))
    {
        // Set the text
        charName.GetComponent<TMPro.TMP_Text>().text = name;

        // Show both
        charName.gameObject.SetActive(true);
        splitter.SetActive(true);
    }
    else
    {
        // Clear the text
        charName.GetComponent<TMPro.TMP_Text>().text = "";

        // Hide both
        charName.gameObject.SetActive(false);
        splitter.SetActive(false);
    }
}



IEnumerator EventStarter()
{
    // Optional brief delay before starting
    yield return new WaitForSeconds(1f);

    // Play background music if assigned
    if (bgMusic != null)
    {
        bgMusic.volume = 0f;
        bgMusic.Play();
        StartCoroutine(FadeInAudio(bgMusic, 2f)); // fade in 2s
    }

    if (fadeScreenIn != null && chapterTitle != null)
    {
        fadeScreenIn.SetActive(true);
        chapterTitle.SetActive(true);

        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        CanvasGroup titleGroup = chapterTitle.GetComponent<CanvasGroup>();
        Image titleImg = chapterTitle.GetComponent<Image>();

        if (fadeImg != null && titleGroup != null)
        {
            fadeImg.color = new Color(0f, 0f, 0f, 1f);
            titleGroup.alpha = 1f;

            if (titleImg != null)
                StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

            float holdTime = 2.5f;
            float fadeDuration = 3f;
            float overlap = 0.5f;
            yield return new WaitForSeconds(holdTime - overlap);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float smooth = Mathf.SmoothStep(1f, 0f, t);

                fadeImg.color = new Color(0f, 0f, 0f, smooth);
                titleGroup.alpha = smooth;
                yield return null;
            }

            fadeImg.color = new Color(0f, 0f, 0f, 0f);
            titleGroup.alpha = 0f;
            fadeScreenIn.SetActive(false);
            chapterTitle.SetActive(false);
        }
    }

    yield return new WaitForSeconds(0.2f);

    if (mainTextObject != null)
        mainTextObject.SetActive(true);

    textToSpeak = "The road ahead had no end, stretching deep into the night. Gradually, the soft earth beneath his feet turned to cracked stone. The bony fingers of black bushes gave way to gaunt buildings, and the lonely emptiness of the swamp was replaced by something else. A sinister presence.";

    if (textBox != null)
    {
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        if (tmpText != null)
            yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, narrationSpeed));
    }

    if (nextButton != null)
        nextButton.SetActive(true);

    eventPos = 1;
}

IEnumerator EventOne()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    textToSpeak = "Once, this had been a market. In that place, once brimming with life, everyone gave away something. Only the destroyer — the toad — had come to take everything. Now all that remained were twisted shadows of spirits, blinking at him from the darkness with eyes red from fear. Vendors, their clawed pale hands clutching the rotted remains of their stalls, terrified by the monster’s return, yet hesitant to let go of what had been precious to them.";

    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, narrationSpeed));

    nextButton.SetActive(true);
    eventPos = 2;
}

IEnumerator EventTwo()
{
    nextButton.SetActive(false);
    ShowDialogue("Jin Chan", "");
    overlayFade.FadeIn();
    StartCoroutine(FadeInRawImage(charJinChan, 1f));

    textToSpeak = "\"<i>These are the consequences of my madness.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

    nextButton.SetActive(true);
    eventPos = 3;
}

IEnumerator EventThree()
{
    nextButton.SetActive(false);
    ShowNarration("");

    textToSpeak = "At his words, the tremblind shades stirred.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

    nextButton.SetActive(true);
    eventPos = 4;
}

IEnumerator EventFour()
{
    nextButton.SetActive(false);
    ShowDialogue("Jin Chan", "");
    whisperStirs.loop = false;
    whisperStirs.Play();

    textToSpeak = "\"<i>The consequences of my atrocities.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

    nextButton.SetActive(true);
    eventPos = 5;
}

IEnumerator EventFive()
{
    nextButton.SetActive(false);
    ShowNarration("");

    textToSpeak = "The toad looked over the shattered rows, the lanterns trembling in the wind, their fire long since extinguished. This place was festering, like a wound left unhealed.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

    nextButton.SetActive(true);
    eventPos = 6;
}

IEnumerator EventSix()
{
    nextButton.SetActive(false);
    ShowDialogue("Jin Chan", "");

    textToSpeak = "\"<i>And I must mend it.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

    nextButton.SetActive(true);
    eventPos = 7;
}

IEnumerator EventSeven()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    // 1️⃣ Show narration
    ShowNarration("");
    StartCoroutine(FadeInRawImage(charJinChan, 1f));
    overlayFade.FadeOut();

    textToSpeak = "Even if it’s too late. Filled with a new resolve, the toad got to work.";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    // 2️⃣ Pause briefly for dramatic effect
    yield return new WaitForSeconds(1f);

    // 3️⃣ Fade out background music (if assigned)
    if (bgMusic != null)
        StartCoroutine(FadeOutAudio(bgMusic, 2f)); // smooth fade in 2 sec

    // 4️⃣ Fade screen to black
    fadeScreenIn.SetActive(true);
    RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
    if (fadeImg != null)
    {
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        float fadeDuration = 2f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            fadeImg.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImg.color = new Color(0f, 0f, 0f, 1f); // ensure fully black
    }

    // 5️⃣ Hold black screen for a moment
    yield return new WaitForSeconds(0.5f);

    // 6️⃣ Load next scene
    SceneManager.LoadScene("TestCandy");
}


// Smooth audio fade helpers
IEnumerator FadeInAudio(AudioSource audio, float duration)
{
    float startVol = 0f;
    float endVol = 1f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        audio.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
        yield return null;
    }
    audio.volume = endVol;
}

IEnumerator FadeOutAudio(AudioSource audio, float duration)
{
    float startVol = audio.volume;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        audio.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
        yield return null;
    }
    audio.volume = 0f;
    audio.Stop();
}

    public void NextButton()
    {
        if (eventPos == 1)
        {
            StartCoroutine(EventOne());
        }
        if (eventPos == 2)
        {
            StartCoroutine(EventTwo());
        }
        if (eventPos == 3)
        {
            StartCoroutine(EventThree());
        }
        if (eventPos == 4)
        {
            StartCoroutine(EventFour());
        }
        if (eventPos == 5)
        {
            StartCoroutine(EventFive());
        }
        if (eventPos == 6)
        {
            StartCoroutine(EventSix());
        }
        if (eventPos == 7)
        {
            StartCoroutine(EventSeven());
        }

    }


}

