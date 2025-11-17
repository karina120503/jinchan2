using UnityEngine.UI;
using System.Collections;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene01Eevents: MonoBehaviour
{

    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charLiuHai;
    public GameObject charJinChan;
    public OverlayFade3 overlayFade;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    public AudioSource waterDropAudio; // drag your water drop AudioSource here in Inspector
    public AudioSource faintWind;
    public AudioSource bgMusic; // assign the background music AudioSource

    public GameObject chapterTitle;   // assign the chapter title image

    // Cinematic timing settings (used in synchronized fade)
public float narrationSpeed = 0.01f;  // faster running text
public float dialogueSpeed = 0.03f;   // dialogue stays same
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


    IEnumerator EventStarter()
{
    // 1️⃣ Show black screen and chapter title immediately
    fadeScreenIn.SetActive(true);
    chapterTitle.SetActive(true);

    RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
    CanvasGroup titleGroup = chapterTitle.GetComponent<CanvasGroup>();
    Image titleImg = chapterTitle.GetComponent<Image>();

    if (fadeImg != null && titleGroup != null)
    {
        // Set initial state
        fadeImg.color = new Color(0f, 0f, 0f, 1f); // fully opaque
        titleGroup.alpha = 1f;                     // fully visible

        // Optional glow
        if (titleImg != null)
            StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

        // 2️⃣ Hold fully visible for a moment, but start fade slightly before hold ends
        float holdTime = 2.5f;   // how long title stays fully visible
        float fadeDuration = 3f; // duration of fade out
        float overlap = 0.5f;    // start fade 0.5s before hold ends

        yield return new WaitForSeconds(holdTime - overlap);

        // 3️⃣ Fade out both smoothly with SmoothStep easing
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

        // Ensure fully invisible at the end
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        titleGroup.alpha = 0f;
        fadeScreenIn.SetActive(false);
        chapterTitle.SetActive(false);
    }

    // 4️⃣ Continue with main scene content
    if (mainTextObject != null)
        mainTextObject.SetActive(true);

        // ✅ Assign your full paragraph
        textToSpeak = "At the bottom of the well lay a toad. The groundwater had long abandoned this place. Neither sun nor moon dared to glance inside. On a rare night, one curious star might twinkle overhead, only to quickly hide behind the clouds, as if averting its gaze from the abomination hidden behind the high stone walls.";
    if (textBox != null)
    {
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    }

    if (nextButton != null)
        nextButton.SetActive(true);

    eventPos = 1;
}

IEnumerator EventOne()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    textToSpeak = "The skies were endlessly far away. Only the coolness of the earth occasionally took pity on the creature, wrapping his thick, scarred skin in gentle moisture. Skin marked by wounds long healed from a battle long past.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(RunNarration(tmpText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 2;
}


IEnumerator EventTwo()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    if (faintWind != null)
    {
        faintWind.loop = true;
        faintWind.Play();
    }

    textToSpeak = "This toad was lame. His hind leg had been lost in the same battle that left him here. Sometimes, when the thick darkness erased the outline of his twisted body, the missing limb would ache where nothing remained.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(RunNarration(tmpText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 3;
}

IEnumerator EventThree()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);
    overlayFade.FadeIn();
    StartCoroutine(FadeInRawImage(charJinChan, 2f));

    textToSpeak = "This toad’s name was Jin Chan. And once, his body had loomed larger than houses and trees, swollen with consumed treasures. The devouring might of the monster sowed chaos, crushed villages, and robbed people of everything they held dear. The weight of gold in his gut would bring brief moments of peace, before hunger consumed his mind once more. That was how it had been, until jewels and gold came spilling from his enormous belly beneath the arc of a wandering monk’s sword.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(RunNarration(tmpText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 4;
}

IEnumerator EventFour()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);
    StartCoroutine(FadeOutRawImage(charJinChan, 3f));

    textToSpeak = "Liu Hai. His name was Liu Hai — a merciful and compassionate monk who answered the cries of the people terrorized by the ravaging toad. He fought Jin Chan and brought him down, turning an age-old threat into a nightmare of the past. In the moment of defeat, Jin Chan had accepted his end.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(RunNarration(tmpText, textToSpeak, "was Liu Hai", charLiuHai, 1f));

    nextButton.SetActive(true);
    eventPos = 5;
}

IEnumerator EventFive()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    textToSpeak = "But it never came. Instead, the monk sheathed his sword and said something that Jin Chang had never heard in his long life.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(RunNarration(tmpText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 6;
}

IEnumerator EventSix()
{
    nextButton.SetActive(false);
    ShowDialogue("Liu Hai", "");

    textToSpeak = "\"<i>I forgive you.</i>\"";
    yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 7;
}

IEnumerator EventSeven()
{
    nextButton.SetActive(false);
    ShowNarration("");

    textToSpeak = "And so Jin Chan fled. It wasn't fear of the monk that drove him forward, but fear of himself. As he ran, he heard the monk call after him.";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 8;
}

IEnumerator EventEight()
{
    nextButton.SetActive(false);
    ShowDialogue("Liu Hai", "");

    textToSpeak = "\"<i>Find me when you’ve forgiven yourself.</i>\"";
    yield return StartCoroutine(RunDialogue(dialogueText, textToSpeak));

    yield return new WaitForSeconds(1f);

    nextButton.SetActive(true);
    eventPos = 9;
}

IEnumerator EventNine()
{
    nextButton.SetActive(false);
    ShowNarration("");
    StartCoroutine(FadeOutRawImage(charLiuHai, 1f));
    overlayFade.FadeOut();

    textToSpeak = "Stripped of his treasures, exhausted by the slow repair of his flesh, the toad’s body had grown light, empty. So too had his soul felt: for the first time freed from hunger and resentful energy.";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 10;
}

IEnumerator EventTen()
{
    yield return StartCoroutine(CrossfadeBackground(background2, 2f));
    nextButton.SetActive(false);
    textBox.SetActive(true);

    ShowNarration("");
    textToSpeak = "A webbed paw pressed against the wet stone..";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    nextButton.SetActive(true);
    eventPos = 11;
}

IEnumerator EventEleven()
{
    nextButton.SetActive(false);
    textBox.SetActive(true);

    // 1️⃣ Show narration
    ShowNarration("");
    textToSpeak = "Jin Chan began to climb...";
    yield return StartCoroutine(RunNarration(narrationText, textToSpeak));

    // 2️⃣ Pause briefly for dramatic effect
    yield return new WaitForSeconds(1f);

    // 3️⃣ Fade out background music (if assigned)
    if (bgMusic != null)
        StartCoroutine(FadeOutAudio(bgMusic, 2f)); // 2 seconds fade

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
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration); // smoother easing
            fadeImg.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImg.color = new Color(0f, 0f, 0f, 1f); // ensure fully black
    }

    // 5️⃣ Hold black screen for a moment
    yield return new WaitForSeconds(0.5f);

    // 6️⃣ Load next scene
    SceneManager.LoadScene("GhostMarket");
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
            StartCoroutine(EventTen());
        }
    }
}
