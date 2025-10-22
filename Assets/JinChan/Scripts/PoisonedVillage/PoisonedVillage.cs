using System.Collections;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoisonedVillage : MonoBehaviour
{

    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charVillager1;
    public GameObject charVillager2;
    public GameObject charVillager3;
    public GameObject charVillager4;
    public OverlayFade overlayFade;
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
    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;
    [SerializeField] private Sprite background2;
    [SerializeField] AudioSource slowSteps;
    [SerializeField] private AudioSource bgmSource;
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

    // Ensure fadeScreenIn and chapterTitle are active for the intro
    if (fadeScreenIn) fadeScreenIn.SetActive(true);
    if (chapterTitle) chapterTitle.SetActive(true);

    // Show textbox only after fade
    textBox.SetActive(false);

    // Start the main scene start sequence
    StartCoroutine(EventStarter());
}


    IEnumerator FadeFromBlack(float duration)
{
    if (fadeScreenIn == null) yield break;

    RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
    if (fadeImg == null) yield break;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (elapsed / duration));
        fadeImg.color = new Color(0f, 0f, 0f, alpha);
        yield return null;
    }

    // Fully transparent at end
    fadeImg.color = new Color(0f, 0f, 0f, 0f);
}

IEnumerator FadeToBlack(float duration)
{
    if (fadeScreenIn == null) yield break;
    RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
    if (fadeImg == null) yield break;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(elapsed / duration);
        fadeImg.color = new Color(0f, 0f, 0f, alpha);
        yield return null;
    }

    fadeImg.color = new Color(0f, 0f, 0f, 1f);
}

    void Update()
    {
        // Keep your old update code
        textLength = TextCreator.charCount;

    }

IEnumerator FadeOutAudio(AudioSource source, float duration)
{
    if (source == null) yield break;

    float startVolume = source.volume;
    float time = 0f;

    while (time < duration)
    {
        time += Time.deltaTime;
        source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
        yield return null;
    }

    source.Stop();
    source.volume = startVolume; // reset for next use
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
    // 0️⃣ Small delay before starting
    yield return new WaitForSeconds(1f);

    // 1️⃣ Ensure fade screen and chapter title start visible
    if (fadeScreenIn != null) fadeScreenIn.SetActive(true);
    if (chapterTitle != null) chapterTitle.SetActive(true);

    RawImage fadeImg = fadeScreenIn?.GetComponent<RawImage>();
    CanvasGroup titleGroup = chapterTitle?.GetComponent<CanvasGroup>();
    Image titleImg = chapterTitle?.GetComponent<Image>();

    // Make sure both are fully visible
    if (fadeImg != null) fadeImg.color = new Color(0f, 0f, 0f, 1f);
    if (titleGroup != null) titleGroup.alpha = 1f;

    // Add soft glow pulse effect for the title
    if (titleImg != null)
        StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

    // Hold the title for a few seconds
    yield return new WaitForSeconds(titleDisplayTime);

    // 2️⃣ Fade out both title and black screen together
    float fadeOutTimer = 0f;
    while (fadeOutTimer < titleFadeOutDuration)
    {
        fadeOutTimer += Time.deltaTime;
        float t = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(fadeOutTimer / titleFadeOutDuration));

        if (fadeImg != null)
            fadeImg.color = new Color(0f, 0f, 0f, t);

        if (titleGroup != null)
            titleGroup.alpha = t;

        yield return null;
    }

    // Make sure both are hidden
    if (fadeImg != null) fadeImg.color = new Color(0f, 0f, 0f, 0f);
    if (titleGroup != null) titleGroup.alpha = 0f;
    if (fadeScreenIn != null) fadeScreenIn.SetActive(false);
    if (chapterTitle != null) chapterTitle.SetActive(false);

    // 3️⃣ Show the main textbox and narration
    if (textBox != null) textBox.SetActive(true);

    if (narrationText != null)
    {
        narrationText.gameObject.SetActive(true);
        narrationText.text = "";
        narrationText.maxVisibleCharacters = 0;

        // Start typewriter effect ONCE
        textToSpeak = "Ancient trees arched overhead, their entwined branches guiding the toad as he hurried forward with his precious burden.The girl breathed faintly on his back like a melting shard of ice, slipping away with every heartbeat.At last, the forest opened.";

        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
    }

    // 4️⃣ Show the 'Next' button
    if (nextButton != null)
        nextButton.SetActive(true);

    eventPos = 1;
}


IEnumerator EventOne()
{
    // event 1 — Narration
    nextButton.SetActive(false);
    textBox.SetActive(true);

    ShowNarration(""); // make sure narration UI is active

    textToSpeak = "The village lay hidden, sunken into itself. The silence that had gathered here over the years was deafening. Half-empty homes. Windows without shutters. Tufts of dry grass pushing through broken tiles. No one had truly lived here for a long time, only lingered. Elderly women turned their heads slowly at his arrival, gray devouring their hair. Gaunt old men straightened by reflex, though their bones could no longer resist any threat.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 2;
}

IEnumerator EventTwo()
{
    // event 2 — Narration
    nextButton.SetActive(false);
    textBox.SetActive(true);

    overlayFade.FadeIn();
    ShowNarration("");

    textToSpeak = "Children, long abandoned by their parents, fled into the shadows, dropping scraps of cloth and clay, the only “toys” they had. A silent, unchanging heaviness settled over Jin Chan the moment he took a breath of the stale air. He knelt gently and lifted the girl from his back. Her bare feet touched the dry earth. One of the elder women stirred and rushed to them.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f, "A silent", charVillager1, 1f));

    nextButton.SetActive(true);
    eventPos = 3;
}

IEnumerator EventThree()
{
    // event 3 — Narration
    nextButton.SetActive(false);
    textBox.SetActive(true);

    StartCoroutine(FadeOutRawImage(charVillager1, 3f));
    StartCoroutine(FadeInRawImage(charVillager2, 5f));

    if (slowSteps != null)
    {
        slowSteps.loop = true;
        slowSteps.volume = 0.5f;
        slowSteps.Play();
    }

    ShowNarration("");
    textToSpeak = "Old and brittle, she collapsed to her knees, throwing her arms around the girl. Then, in a flash of panic, she tried to rise and flee, as if the toad might still change his mind and reclaim the child. Jin Chan steadied her by the elbow. When she stood, she didn’t look back — she just ran, clutching the girl in silent fear, unaware that the monster before her no longer meant harm.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    if (slowSteps != null)
        StartCoroutine(FadeOutAudio(slowSteps, 1.5f));

    nextButton.SetActive(true);
    eventPos = 4;
}

IEnumerator EventFour()
{
    // Dialogue: Elder Man
    nextButton.SetActive(false);
    ShowDialogue("Elder Man", "");

    StartCoroutine(FadeOutRawImage(charVillager2, 1f));
    StartCoroutine(FadeInRawImage(charVillager3, 2f));

    var prevSize = dialogueText.fontSize;
    dialogueText.fontSize = 50;

    textToSpeak = "\"<i>You brought her back.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "You brought", charVillager3, 1f));

    dialogueText.fontSize = prevSize; // restore original size

    nextButton.SetActive(true);
    eventPos = 5;
}

IEnumerator EventFive()
{
    // Narration
    nextButton.SetActive(false);
    ShowNarration("");

    textToSpeak = "Jin Chan turned. An old man stood behind him. Frail, wrapped in a patched robe, sorrow and age carved deep beneath serious eyes.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 6;
}

IEnumerator EventSix()
{
    // Dialogue: Elder Man
    nextButton.SetActive(false);
    ShowDialogue("Elder Man", "");

    textToSpeak = "\"<i>I know who you are, Jin Chan the Devourer. But there is no gold here. Nothing remains since your dark qi began poisoning these lands. Please, leave.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

    yield return new WaitForSeconds(2f);
    nextButton.SetActive(true);
    eventPos = 7;
}

IEnumerator EventSeven()
{
    // Narration
    nextButton.SetActive(false);
    ShowNarration("");

    StartCoroutine(FadeOutRawImage(charVillager3, 1f));

    textToSpeak = "The toad bowed his head with respect, taking in every word as he should.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 8;
}

IEnumerator EventEight()
{
    // Dialogue: Jin Chan
    nextButton.SetActive(false);
    ShowDialogue("Jin Chan", "");

    StartCoroutine(FadeOutRawImage(charVillager3, 1f));
    StartCoroutine(FadeInRawImage(charVillager4, 1f));

    textToSpeak = "\"<i>For many years, my greed only took from you. Before I go, let me give something back.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 9;
}

IEnumerator EventNine()
{
    // Narration
    nextButton.SetActive(false);
    ShowNarration("");

    textToSpeak = "His voice caught in his throat. Loss, sorrow, and grief lingered in the air like smoke. And Jin Chan, perhaps, could clear it. The elder studied him in silence. Then he nodded slowly.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 10;
}

IEnumerator EventTen()
{
    // Dialogue: Elder Man
    nextButton.SetActive(false);
    ShowDialogue("Elder Man", "");

    StartCoroutine(FadeOutRawImage(charVillager4, 1f));
    StartCoroutine(FadeInRawImage(charVillager3, 2f));

    textToSpeak = "\"<i>The tree has already become a boat. You can only clear the river for it.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 11;
}


IEnumerator EventEleven()
{
    // Event 11
    nextButton.SetActive(false);
    textBox.SetActive(true);

    // Fade out character + overlay
    StartCoroutine(FadeOutRawImage(charVillager3, 1f));
    overlayFade.FadeOut();

    // Narration
    ShowNarration("");
    textToSpeak = "With that blessing, Jin Chan began to prepare the ritual of cleansing.";

    // Start typewriter
    Coroutine typewriter = StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    // Start fading music slightly before narration ends (optional)
    if (bgmSource != null && bgmSource.isPlaying)
    {
        yield return new WaitForSeconds(textToSpeak.Length * 0.03f - 2f); // fade ~2s before text ends
        StartCoroutine(FadeOutMusic(2f));
    }

    // Wait until typewriter finishes completely
    yield return typewriter;

    // Short dramatic pause
    yield return new WaitForSeconds(0.3f);

    // Fade to black
    yield return StartCoroutine(FadeToBlack(2f));

    // Load next scene
    SceneManager.LoadScene("RhytmGame");
}

IEnumerator FadeOutMusic(float duration)
{
    if (bgmSource == null || !bgmSource.isPlaying) yield break;

    float startVolume = bgmSource.volume;
    float time = 0f;

    while (time < duration)
    {
        time += Time.deltaTime;
        bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
        yield return null;
    }

    bgmSource.Stop();
    bgmSource.volume = startVolume; // reset for next scene
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
        if (eventPos == 8)
        {
            StartCoroutine(EventEight());
        }
        if (eventPos == 9)
        {
            StartCoroutine(EventNine());
        }
        if (eventPos == 10)
        {
            StartCoroutine(EventTen());
        }
        if (eventPos == 11)
        {
            StartCoroutine(EventEleven());
        }

    }


}