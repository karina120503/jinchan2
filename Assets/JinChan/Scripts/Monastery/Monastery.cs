using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monastery : MonoBehaviour
{
    [Header("UI References")]
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
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject mainTextObject;
    [SerializeField] private GameObject nextButton;

    [Header("Typewriter Settings")]
    public float narrationSpeed = 0.01f; // faster running text
    public float dialogueSpeed = 0.03f;

    private string textToSpeak;
    private int textLength;
    private int eventPos = 0;

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

    // ---------- UI Methods ----------
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

    // ---------- Fade Methods ----------
    IEnumerator FadeInRawImage(GameObject obj, float duration)
    {
        RawImage img = obj.GetComponent<RawImage>();
        if (img == null) yield break;

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

        // Fade-in glow
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, t / fadeInTime);
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Pulse
        while (img.gameObject.activeSelf)
        {
            float glow = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, glow));
            yield return null;
        }

        img.color = originalColor;
    }

    // ---------- Typewriter ----------
    IEnumerator TypewriterEffect(TMP_Text textComponent, string fullText, float delay,
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
                    StartCoroutine(FadeInRawImage(objectToActivate1, fadeDuration1));
            }

            if (!string.IsNullOrEmpty(triggerPhrase2) && objectToActivate2 != null)
            {
                int idx2 = fullText.IndexOf(triggerPhrase2);
                if (idx2 >= 0 && i >= idx2 && !objectToActivate2.activeSelf)
                    StartCoroutine(FadeInRawImage(objectToActivate2, fadeDuration2));
            }

            yield return new WaitForSeconds(delay);
        }

        textLength = fullText.Length;
    }

    // ---------- Events ----------
    IEnumerator EventStarter()
    {
        yield return new WaitForSeconds(1f);

        // Chapter title + fade
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

                yield return new WaitForSeconds(2f);

                float fadeDuration = 3f;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(elapsed / fadeDuration));
                    fadeImg.color = new Color(0f, 0f, 0f, t);
                    titleGroup.alpha = t;
                    yield return null;
                }

                fadeScreenIn.SetActive(false);
                chapterTitle.SetActive(false);
            }
        }

        yield return new WaitForSeconds(0.2f);
        if (mainTextObject != null) mainTextObject.SetActive(true);

        textToSpeak = "The bamboo forest was thinning, parting in front of the monster, first with a beaten path, then with a paved stone, showing that the path had an endpoint, and he was getting closer to it. The air was getting cooler and the rain had long since dried up, instead enveloping the toad's skin in a translucent veil of mist in which the shape of the monastery dissolved.";

        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        if (nextButton != null) nextButton.SetActive(true);
        eventPos = 1;
    }

    IEnumerator EventOne()
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);

        textToSpeak = "White walls with black tiles appeared before him, opening into a gateway that revealed darkened halls on thin pillars, long gallery bridges, and roofs like unfolded scrolls. The three, dressed in modest monk's robes, were already waiting for him inside. No one was surprised at his arrival. Toad halted, bowing his head respectfully.";

        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        nextButton.SetActive(false);

        ShowDialogue("Jin Chan", "");
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        overlayFade.FadeIn();

        textToSpeak = "\"<i>I came here to find Liu Hai.</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed, "I came", charJinChan, 1f));

        nextButton.SetActive(true);
        eventPos = 3;
    }

    IEnumerator EventThree()
    {
        nextButton.SetActive(false);
        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charJinChan, 3f));

        textToSpeak = "One of them standing in the middle spoke to him in response. There was no fear or prejudice in the calm voice.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        nextButton.SetActive(true);
        eventPos = 4;
    }

    IEnumerator EventFour()
    {
        nextButton.SetActive(false);
        ShowDialogue("Monk", "");
        StartCoroutine(FadeInRawImage(charMonk, 1f));

        textToSpeak = "\"<i>Why are you looking for him, Jin Chan?.</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed, "Why are", charMonk, 1f));

        nextButton.SetActive(true);
        eventPos = 5;
    }

    IEnumerator EventFive()
    {
        nextButton.SetActive(false);
        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charMonk, 1f));

        textToSpeak = "The toad closed his eyes. Liu Hai had told him to find him when he could forgive himself. But is the little he has managed to do along the way enough to earn forgiveness? What is this split-second mercy compared to years of rampage and destruction? The monk seemed to hear even what he didn’t say.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        nextButton.SetActive(true);
        eventPos = 6;
    }

    IEnumerator EventSix()
    {
        nextButton.SetActive(false);
        ShowDialogue("Monk", "");
        StartCoroutine(FadeInRawImage(charMonk, 1f));

        textToSpeak = "\"<i>We judge others by their deeds, but only ourselves by our thoughts. Judge yourself by what is in your heart. Show that your heart is pure now, and you will be able to accept forgiveness.</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, dialogueSpeed));

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
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, narrationSpeed));

        // Player now clicks Next to continue
        nextButton.SetActive(true);
        eventPos = 8; // Update eventPos so NextButton knows to trigger fade & load
    }

    // ---------- New Fade-Out & Scene Loader for the final event ----------
    IEnumerator FadeAndLoadMain(float duration = 2.5f)
    {
        if (fadeScreenIn == null) yield break;

        fadeScreenIn.SetActive(true);
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        if (fadeImg == null) yield break;

        Color c = fadeImg.color;
        c.a = 0f; // start transparent
        fadeImg.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            fadeImg.color = c;
            yield return null;
        }

        // Ensure fully black
        fadeImg.color = new Color(0f, 0f, 0f, 1f);

        // Load next scene
        SceneManager.LoadScene("Main");
    }

    // ---------- Updated NextButton handler ----------
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
            case 8: StartCoroutine(FadeAndLoadMain()); break; // final click triggers fade
        }
    }

}