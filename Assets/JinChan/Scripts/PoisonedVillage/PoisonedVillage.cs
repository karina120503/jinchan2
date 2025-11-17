using System.Collections;
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

    public float blackHoldTime = 1.5f;
    public float blackFadeDuration = 2.5f;
    public float titleFadeInDuration = 2.5f;
    public float titleDisplayTime = 3f;
    public float titleFadeOutDuration = 2.5f;

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

        if (fadeScreenIn) fadeScreenIn.SetActive(true);
        if (chapterTitle) chapterTitle.SetActive(true);

        textBox.SetActive(false);

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

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, t / fadeInTime);
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        while (img.gameObject.activeSelf)
        {
            float glow = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                                  Mathf.Lerp(0.7f, 0.7f + maxAlphaStrength, glow));
            yield return null;
        }

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

            if (!string.IsNullOrEmpty(triggerPhrase1) && objectToActivate1 != null)
            {
                if (i >= fullText.IndexOf(triggerPhrase1) && !objectToActivate1.activeSelf)
                {
                    StartCoroutine(FadeInRawImage(objectToActivate1, fadeDuration1));
                }
            }

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

    void SetSpeaker(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            charName.GetComponent<TMPro.TMP_Text>().text = name;
            charName.gameObject.SetActive(true);
            splitter.SetActive(true);
        }
        else
        {
            charName.GetComponent<TMPro.TMP_Text>().text = "";
            charName.gameObject.SetActive(false);
            splitter.SetActive(false);
        }
    }

    IEnumerator EventStarter()
    {
        yield return new WaitForSeconds(1f);

        if (fadeScreenIn != null) fadeScreenIn.SetActive(true);
        if (chapterTitle != null) chapterTitle.SetActive(true);

        RawImage fadeImg = fadeScreenIn?.GetComponent<RawImage>();
        CanvasGroup titleGroup = chapterTitle?.GetComponent<CanvasGroup>();
        Image titleImg = chapterTitle?.GetComponent<Image>();

        if (fadeImg != null) fadeImg.color = new Color(0f, 0f, 0f, 1f);
        if (titleGroup != null) titleGroup.alpha = 1f;

        if (titleImg != null)
            StartCoroutine(GlowPulse(titleImg, 2f, 0.5f, 0f));

        yield return new WaitForSeconds(titleDisplayTime);

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

        if (fadeImg != null) fadeImg.color = new Color(0f, 0f, 0f, 0f);
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (fadeScreenIn != null) fadeScreenIn.SetActive(false);
        if (chapterTitle != null) chapterTitle.SetActive(false);

        if (textBox != null) textBox.SetActive(true);

        if (narrationText != null)
        {
            narrationText.gameObject.SetActive(true);
            narrationText.text = "";
            narrationText.maxVisibleCharacters = 0;

            textToSpeak = "Ancient trees arched overhead, their entwined branches guiding the toad as he hurried forward with his precious burden.The girl breathed faintly on his back like a melting shard of ice, slipping away with every heartbeat.At last, the forest opened.";

            // Faster narration
            yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));
        }

        if (nextButton != null)
            nextButton.SetActive(true);

        eventPos = 1;
    }

    // ==========================
    // Events 1-11 (with faster narration)
    // ==========================

    IEnumerator EventOne()
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowNarration("");

        textToSpeak = "The village lay hidden, sunken into itself. The silence that had gathered here over the years was deafening. Half-empty homes. Windows without shutters. Tufts of dry grass pushing through broken tiles. No one had truly lived here for a long time, only lingered. Elderly women turned their heads slowly at his arrival, gray devouring their hair. Gaunt old men straightened by reflex, though their bones could no longer resist any threat.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));

        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);

        overlayFade.FadeIn();
        ShowNarration("");

        textToSpeak = "Children, long abandoned by their parents, fled into the shadows, dropping scraps of cloth and clay, the only “toys” they had. A silent, unchanging heaviness settled over Jin Chan the moment he took a breath of the stale air. He knelt gently and lifted the girl from his back. Her bare feet touched the dry earth. One of the elder women stirred and rushed to them.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f, "A silent", charVillager1, 1f));

        nextButton.SetActive(true);
        eventPos = 3;
    }

    IEnumerator EventThree()
    {
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
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));

        if (slowSteps != null)
            StartCoroutine(FadeOutAudio(slowSteps, 1.5f));

        nextButton.SetActive(true);
        eventPos = 4;
    }

    IEnumerator EventFour()
    {
        nextButton.SetActive(false);
        ShowDialogue("Elder Man", "");

        StartCoroutine(FadeOutRawImage(charVillager2, 1f));
        StartCoroutine(FadeInRawImage(charVillager3, 2f));

        textToSpeak = "\"<i>You brought her back.</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "You brought", charVillager3, 1f));

        nextButton.SetActive(true);
        eventPos = 5;
    }

    IEnumerator EventFive()
    {
        nextButton.SetActive(false);
        ShowNarration("");

        textToSpeak = "Jin Chan turned. An old man stood behind him. Frail, wrapped in a patched robe, sorrow and age carved deep beneath serious eyes.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));

        nextButton.SetActive(true);
        eventPos = 6;
    }

    IEnumerator EventSix()
    {
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
        nextButton.SetActive(false);
        ShowNarration("");

        StartCoroutine(FadeOutRawImage(charVillager3, 1f));

        textToSpeak = "The toad bowed his head with respect, taking in every word as he should.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));

        nextButton.SetActive(true);
        eventPos = 8;
    }

    IEnumerator EventEight()
    {
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
        nextButton.SetActive(false);
        ShowNarration("");

        textToSpeak = "His voice caught in his throat. Loss, sorrow, and grief lingered in the air like smoke. And Jin Chan, perhaps, could clear it. The elder studied him in silence. Then he nodded slowly.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.015f));

        nextButton.SetActive(true);
        eventPos = 10;
    }

    IEnumerator EventTen()
    {
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
        // Event 11 — Narration
        nextButton.SetActive(false);
        textBox.SetActive(true);

        // Fade out character + overlay visually
        StartCoroutine(FadeOutRawImage(charVillager3, 1f));
        overlayFade.FadeOut();

        ShowNarration("");
        textToSpeak = "With that blessing, Jin Chan began to prepare the ritual of cleansing.";

        // Start typewriter but do NOT start fade yet
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

        // Show the 'Next' button now
        nextButton.SetActive(true);

        // Mark eventPos to trigger fade-on-click
        eventPos = 12;
    }

    IEnumerator FadeAndLoadNextScene()
    {
        // Disable the button so player can't click again
        nextButton.SetActive(false);

        // Fade out BGM
        if (bgmSource != null && bgmSource.isPlaying)
            StartCoroutine(FadeOutAudio(bgmSource, 2f));

        // Fade to black
        yield return StartCoroutine(FadeToBlack(2f));

        // Load the next scene
        SceneManager.LoadScene("RhytmGame");
    }

    public void NextButton()
    {
        if (eventPos == 1) StartCoroutine(EventOne());
        else if (eventPos == 2) StartCoroutine(EventTwo());
        else if (eventPos == 3) StartCoroutine(EventThree());
        else if (eventPos == 4) StartCoroutine(EventFour());
        else if (eventPos == 5) StartCoroutine(EventFive());
        else if (eventPos == 6) StartCoroutine(EventSix());
        else if (eventPos == 7) StartCoroutine(EventSeven());
        else if (eventPos == 8) StartCoroutine(EventEight());
        else if (eventPos == 9) StartCoroutine(EventNine());
        else if (eventPos == 10) StartCoroutine(EventTen());
        else if (eventPos == 11) StartCoroutine(EventEleven());
        else if (eventPos == 12)
        {
            // Player click triggers fade + scene load
            StartCoroutine(FadeAndLoadNextScene());
        }
    }
}