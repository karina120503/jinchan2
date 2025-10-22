using System.Collections;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monastery : MonoBehaviour
{
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charMonk;
    public OverlayFade2 overlayFade;
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
        fadeScreenIn.SetActive(true);
        chapterTitle.SetActive(true);

        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        CanvasGroup titleGroup = chapterTitle.GetComponent<CanvasGroup>();
        Image titleImg = chapterTitle.GetComponent<Image>();

        if (fadeImg != null && titleGroup != null)
        {
            // Initial state: black fully opaque, title fully visible
            fadeImg.color = new Color(0f, 0f, 0f, 1f);
            titleGroup.alpha = 1f;

            // Start glow immediately
            if (titleImg != null)
                StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

            // Hold time before fade
            float holdTime = 2.5f;   // duration fully visible
            float fadeDuration = 3f; // fade out duration
            float overlap = 0.5f;    // start fade slightly before hold ends
            yield return new WaitForSeconds(holdTime - overlap);

            // Fade both black screen and title together using SmoothStep easing
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
    }

    // 1️⃣ Short pause before narration
    yield return new WaitForSeconds(0.2f);

    // 2️⃣ Show main text object
    if (mainTextObject != null)
        mainTextObject.SetActive(true);

    // 3️⃣ Assign full narration text
    textToSpeak = "The bamboo forest was thinning, parting in front of the monster, first with a beaten path, then with a paved stone, showing that the path had an endpoint, and he was getting closer to it. The air was getting cooler and the rain had long since dried up, instead enveloping the toad's skin in a translucent veil of mist in which the shape of the monastery dissolved.";

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

        textToSpeak = "White walls with black tiles appeared before him, opening into a gateway that revealed darkened halls on thin pillars, long gallery bridges, and roofs like unfolded scrolls. The three, dressed in modest monk's robes, were already waiting for him inside. No one was surprised at his arrival. Toad halted, bowing his head respectfully.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 2;
    }

IEnumerator EventTwo()
{
    // Dialogue: Elder Man
    nextButton.SetActive(false);

    // switch UI to dialogue mode (name+splitter shown, narration hidden)
    ShowDialogue("Jin Chan", "");
    StartCoroutine(FadeInRawImage(charJinChan, 1f));

    // type the line to the dialogue text
    string prev = dialogueText.text;
    var prevSize = dialogueText.fontSize;
    dialogueText.fontSize = 45;
    overlayFade.FadeIn();    
    textToSpeak = "\"<i>I came here to find Liu Hai.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "I came", charJinChan, 1f));
    nextButton.SetActive(true);
    eventPos = 3;
}
    IEnumerator EventThree()
    {
        // event 3
        nextButton.SetActive(false);
        textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charJinChan, 3f));
        ShowNarration("");      
        textToSpeak = "One of them standing in the middle spoke to him in response. There was no fear or prejudice in the calm voice.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        // No phrase triggers needed, just start typing immediately
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 4; // <-- make sure this isn't set to 3 again or it will loop
    }

IEnumerator EventFour()
{
    nextButton.SetActive(false);
    ShowDialogue("Monk", "");
    StartCoroutine(FadeInRawImage(charMonk, 1f));

    string prev = dialogueText.text;
    var prevSize = dialogueText.fontSize;
    dialogueText.fontSize = 45;

    textToSpeak = "\"<i>Why are you looking for him, Jin Chan?.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "Why are", charMonk, 1f));

    nextButton.SetActive(true);
    eventPos = 5;
}

IEnumerator EventFive()
{
    nextButton.SetActive(false);
    ShowNarration("");
    StartCoroutine(FadeOutRawImage(charMonk, 1f));
    textToSpeak = "The toad closed his eyes. Liu Hai had told him to find him when he could forgive himself. But is the little he has managed to do along the way enough to earn forgiveness? What is this split-second mercy compared to years of rampage and destruction? The monk seemed to hear even what he didn’t say.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 6;
}

IEnumerator EventSix()
{
    // Dialogue: Elder Man
    nextButton.SetActive(false);
    ShowDialogue("Monk", "");
    StartCoroutine(FadeInRawImage(charMonk, 1f));
    textToSpeak = "\"<i>We judge others by their deeds, but only ourselves by our thoughts. Judge yourself by what is in your heart. Show that your heart is pure now, and you will be able to accept forgiveness.</i>\"";
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
    yield return new WaitForSeconds(2f);
    nextButton.SetActive(true);
    eventPos = 7;
}

IEnumerator EventSeven()
{
    nextButton.SetActive(false);
    ShowNarration("");
    StartCoroutine(FadeOutRawImage(charMonk, 3f));
    textToSpeak = "The monks retreated, opening the way to the calligraphy hall, where another trial awaited him. The walls, the scrolls, the silence. On the table was a brush. And beneath the brush, a thought.";
    yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
    nextButton.SetActive(true);
    SceneManager.LoadScene("Main");
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
