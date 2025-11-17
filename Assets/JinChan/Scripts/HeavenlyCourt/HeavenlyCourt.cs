using System.Collections;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class HeavenlyCourt : MonoBehaviour
{

    public GameObject fadeScreenIn;
    public GameObject fadeScreenInFinal;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charLiuHai;
    public OverlayFade4 overlayFade;
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
    public TMP_Text narrationFinalText;

    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;
    [SerializeField] private Sprite background2;
    [SerializeField] AudioSource lightRain;

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
    // 0️⃣ Initial brief delay
    yield return new WaitForSeconds(1f);

    if (fadeScreenIn != null && chapterTitle != null)
    {
        // Ensure objects are active
        fadeScreenIn.SetActive(true);
        chapterTitle.SetActive(true);

        // Get components safely
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        CanvasGroup titleGroup = chapterTitle.GetComponent<CanvasGroup>();
        Image titleImg = chapterTitle.GetComponent<Image>();

        if (fadeImg == null)
        {
            Debug.LogWarning("fadeScreenIn needs a RawImage component!");
            yield break;
        }

        if (titleGroup == null)
        {
            // Add CanvasGroup dynamically if missing
            titleGroup = chapterTitle.AddComponent<CanvasGroup>();
        }

        // Initial alpha setup
        fadeImg.color = new Color(0f, 0f, 0f, 1f);
        titleGroup.alpha = 1f;

        // Start title glow if it exists
        if (titleImg != null)
            StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

        // Hold time before fade
        float holdTime = 2.5f;
        float fadeDuration = 3f;
        float overlap = 0.5f;

        yield return new WaitForSeconds(holdTime - overlap);

        // Fade both black screen and title together
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

    // 1️⃣ Short pause before narration
    yield return new WaitForSeconds(0.2f);

    // 2️⃣ Show main text object
    if (mainTextObject != null)
        mainTextObject.SetActive(true);

    // 3️⃣ Assign full narration text
    textToSpeak = "The white veil released him slowly: first a cold, slippery stone fell beneath his paws, then a thin bamboo grew up, hugging the road gently. Close up, mighty boulders covered with soft moss appeared, and beyond them, an endless sheet of water, like a mosaic broken by the massive leaves of lilies and their pink delicate flowers, slightly raised above the waters, as if afraid to wet the skirts of their gowns.";

    if (textBox != null)
    {
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        if (tmpText != null)
            yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));
    }

    // 4️⃣ Show 'Next' button and update event state
    if (nextButton != null)
        nextButton.SetActive(true);

    eventPos = 1;
}


    IEnumerator EventOne()
    {
        // event 1
        nextButton.SetActive(false);
        textBox.SetActive(true);

        overlayFade.FadeIn();
        StartCoroutine(FadeInRawImage(charJinChan, 3f));
        textToSpeak = "Jin Chan stopped. The fog barely allowed him a glimpse of the other shore, where wide passageways and stone bridges spread out, leading to the white walls of a massive palace, as vast as an entire city, and just as ungraspable. But someone was sitting on the other bank, an indistinct figure frozen in white, head bowed over the flowers. Jin Chan held his breath. Could it be?";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        // event 3
        nextButton.SetActive(false);
        textBox.SetActive(true);

        textToSpeak = "He looked down. The toad's face was sharp in the reflection, as clear as a mirror. But the ripples blurred it as he put his paws down. A leap brought him down on one of the lily leaves. And so, leaf to leaf, he made his way across the lake, spending more and more time building up his courage with each jump bringing him closer to the man.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 3; 
    }

    IEnumerator EventThree()
    {
        // event 3
        nextButton.SetActive(false);
        textBox.SetActive(true);

        textToSpeak = "It was indeed Liu Hai. There could be no mistake. On the terrace, by the water’s edge, he sat in meditation, the very air around him hushed, as if hesitant to disturb the monk. Toad approached at arm's length, hesitating to peer into the seemingly sleeping face. Suddenly, the man's lips curled into a smile and Liu Hai opened his eyes. Jin Chan twitched back involuntarily.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 4;
    }

        IEnumerator EventFour()
    {
        // event 4
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Liu Hai", "");
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        // Fade in Villager2
        StartCoroutine(FadeInRawImage(charLiuHai, 3f));

        textToSpeak = "\"<i>You showed up.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 5; 
    }

        IEnumerator EventFive()
    {
        // event 5
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        textToSpeak = "He appeared to be glad to see him. Suddenly, all of the ethereal aura was stripped away from him, and a young boy was sitting in front of Jin Chan, as if waiting for his brother, not the monster he had barely defeated. Jin Chan didn't know how to reply. He had to prove to everyone he met along the way that he was not the same person he was before. But it was as if the monk already knew.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 6;
    }

        IEnumerator EventSix()
    {
        // event 6
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        StartCoroutine(FadeOutRawImage(charLiuHai, 1f));
        StartCoroutine(FadeInRawImage(charJinChan, 3f));

        textToSpeak = "\"<i>I...I want to understand.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 7; 
    }

        IEnumerator EventSeven()
    {
        // event 7
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        textToSpeak = "Suddenly, all the confusion in his soul began to take shape.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 8;
    }

        IEnumerator EventEight()
    {
        // event 4
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");

        textToSpeak = "\"<i>I want to know why you saved me.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 9; 
    }

        IEnumerator EventNine()
    {
        // event 9
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        textToSpeak = "The smile never left the monk's face";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 10;
    }

        IEnumerator EventTen()
    {
        // event 4
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Liu Hai", "");
        StartCoroutine(FadeInRawImage(charLiuHai, 2f));
        textToSpeak = "\"<i>I didn't save you. You saved yourself. The moment you were given a chance you chose to begin again. That choice was yours. The merit is yours.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 11; 
    }

        IEnumerator EventEleven()
    {
        // event 11
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charLiuHai, 3f));
        textToSpeak = "Merit? Jin Chan felt something inside resist. Reminded him of his old misdeeds. And yet Liu Hai forgave him, against all odds. Why? Toad jumped onto the bridge where the monk was sitting. Sitting down beside him, he looked at the beautiful lily pond. There, on the other shore, was the whole path he had taken.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 12;
    }

        IEnumerator EventTwelve()
    {
        // event 12
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        StartCoroutine(FadeInRawImage(charJinChan, 2f));
        textToSpeak = "\"<i>I feared forgiveness more than punishment. Because forgiveness requires you to live with yourself. Not run away. It means facing everything you’ve done and accepting it. That’s why you were stronger. Only a strong heart can forgive.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 13; 
    }

        IEnumerator EventThirteen()
    {
        // event 13
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        textToSpeak = "The monk didn't move, only stared into the distance, the same way Jin Chan was gazing.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 14;
    }

        IEnumerator EventFourteen()
    {
        // event 14
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Liu Hai", "");
        StartCoroutine(FadeInRawImage(charLiuHai, 1f));
        textToSpeak = "\"<i>And now you’ve grown strong, too. What will you do with that strength, Jin Chan?</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 15; 
    }

        IEnumerator EventFifthteen()
    {
        // event 15
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charLiuHai, 1f));
        textToSpeak = "He didn't think about it. Despite the road behind him, his journey is not over. And the unknown lies ahead. Dangers, difficulties, challenges. He wasn't sure if they wouldn’t break what he had just gained.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 16;
    }

        IEnumerator EventSixteen()
    {
        // event 14
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        textToSpeak = "\"<i>I still have more to learn.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 17; 
    }

        IEnumerator EventSeventeen()
    {
        // event 17
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Liu Hai", "");
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        StartCoroutine(FadeInRawImage(charLiuHai, 1f));
        textToSpeak = "\"<i>Then stay with me.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 18; 
    }

        IEnumerator EventEightteen()
    {
        // event 18
        nextButton.SetActive(false);
        textBox.SetActive(true);

        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charLiuHai, 1f));
        StartCoroutine(FadeInRawImage(charJinChan, 3f));
        textToSpeak = "Just like that. Jin Chan didn't need a second to think about it. He already had an answer in mind.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 19;
    }

        IEnumerator EventNineteen()
    {
        // event 19
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>I'll stay.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 20; 
    }

IEnumerator EventTwenty()
{
    // 1️⃣ Hide dialogue UI
    if (textBox != null) textBox.SetActive(false);
    if (nextButton != null) nextButton.SetActive(false);

    // 2️⃣ Smooth fade to black
    if (fadeScreenInFinal != null)
    {
        fadeScreenInFinal.SetActive(true);

        Animator fadeAnimator = fadeScreenInFinal.GetComponent<Animator>();
        if (fadeAnimator != null) fadeAnimator.enabled = false;

        CanvasGroup cg = fadeScreenInFinal.GetComponent<CanvasGroup>();
        Image fadeImg = fadeScreenInFinal.GetComponent<Image>();
        RawImage fadeRaw = fadeScreenInFinal.GetComponent<RawImage>();

        // Start fully transparent
        if (cg != null) cg.alpha = 0f;
        else if (fadeImg != null) fadeImg.color = new Color(0, 0, 0, 0);
        else if (fadeRaw != null) fadeRaw.color = new Color(0, 0, 0, 0);

        // Fade coroutine
        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (cg != null) cg.alpha = alpha;
            else if (fadeImg != null) fadeImg.color = new Color(0,0,0,alpha);
            else if (fadeRaw != null) fadeRaw.color = new Color(0,0,0,alpha);
            yield return null;
        }

        // Ensure fully black
        if (cg != null) cg.alpha = 1f;
        else if (fadeImg != null) fadeImg.color = new Color(0,0,0,1f);
        else if (fadeRaw != null) fadeRaw.color = new Color(0,0,0,1f);
    }

    yield return new WaitForSeconds(1.5f); // hold fully black

    // 3️⃣ Show final narration (smaller font, slower typewriter)
    if (narrationFinalText != null)
    {
        narrationFinalText.gameObject.SetActive(true);
        narrationFinalText.alignment = TextAlignmentOptions.Center;
        narrationFinalText.fontSize = 65;
        narrationFinalText.text = "";

        string narration =
            "From that moment on, their path continued through a thousand stories, many of which became legends.\n\n" +
            "Some of those legends still live to this day.\n\n";

        yield return StartCoroutine(TypewriterEffect(narrationFinalText, narration, 0.06f));
    }

    yield return new WaitForSeconds(2f); // pause before "The End"

    // 4️⃣ Show "The End"
    if (narrationFinalText != null)
    {
        narrationFinalText.fontSize = 80;
        string theEndText = "The End";
        yield return StartCoroutine(TypewriterEffect(narrationFinalText, theEndText, 0.08f));

        yield return new WaitForSeconds(2.5f); // let it sink in
    }

    // 5️⃣ Show credits
    if (narrationFinalText != null)
    {
        narrationFinalText.fontSize = 80;
        narrationFinalText.alignment = TextAlignmentOptions.Top;

        string creditsText =
            "Credits\n\n" +
            "Chechina Anna Mariia — Producer, Programmer, Writer\n" +
            "Tsalai Katherine — Lead Artist, Level Designer\n" +
            "Marryane Tasya Karina — Level Design, Programmer, Sound Engineer\n\n" +
            "Thank you for playing!";

        narrationFinalText.text = creditsText;

        RectTransform rt = narrationFinalText.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Scroll from below screen to above total text height
            Vector2 startPos = new Vector2(rt.anchoredPosition.x, -Screen.height);
            Vector2 endPos = new Vector2(rt.anchoredPosition.x, rt.sizeDelta.y + Screen.height);
            float scrollTime = 12f;
            float elapsed = 0f;

            while (elapsed < scrollTime)
            {
                elapsed += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / scrollTime);
                yield return null;
            }
        }
    }

    // fadeScreenInFinal stays active until you decide to remove it
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
        if (eventPos == 12)
        {
            StartCoroutine(EventTwelve());
        }
        if (eventPos == 13)
        {
            StartCoroutine(EventThirteen());
        }
        if (eventPos == 14)
        {
            StartCoroutine(EventFourteen());
        }
        if (eventPos == 15)
        {
            StartCoroutine(EventFifthteen());
        }
        if (eventPos == 16)
        {
            StartCoroutine(EventSixteen());
        }
        if (eventPos == 17)
        {
            StartCoroutine(EventSeventeen());
        }
        if (eventPos == 18)
        {
            StartCoroutine(EventEightteen());
        }
        if (eventPos == 19)
        {
            StartCoroutine(EventNineteen());
        }
        if (eventPos == 20)
        {
            StartCoroutine(EventTwenty());
        }

    }

}

