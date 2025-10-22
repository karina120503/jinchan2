using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostMarket2 : MonoBehaviour
{
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charVillageGirl;
    public OverlayFade7 overlayFade;
    public TMP_Text narrationText;
    public TMP_Text dialogueText;
    public TMP_Text charName;
    public GameObject splitter;
    public AudioSource softRustle;
    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlayImage;
    [SerializeField] AudioSource whisperStirs;
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

    // Ensure fade is full black at start
    if (fadeScreenIn)
    {
        fadeScreenIn.SetActive(true);
        RawImage fadeImg = fadeScreenIn.GetComponent<RawImage>();
        fadeImg.color = new Color(0f,0f,0f,1f);
    }

    // Show textbox immediately (optional)
    textBox.SetActive(true);

    StartCoroutine(SceneStartSequence());
}

IEnumerator SceneStartSequence()
{
    // Fade in from black (2 sec)
    yield return StartCoroutine(FadeFromBlack(2f));

    // Start first event immediately after fade
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
    // No FadeFromBlack here — only at start
    // Show the text box — no need to disable fadeScreenIn
    mainTextObject.SetActive(true);

    // Narration text
    textToSpeak = "Jin Chan gently placed the last stall back in place. The planks creaked beneath his webbed feet, then glowed faintly with soft light — the market, for the first time in years, seemed to breathe again. But just as he felt the stirrings of relief, he sensed it. A small hand tugged at the hem of his robe.";
    TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
    yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));

    nextButton.SetActive(true);
    eventPos = 1;
}


    IEnumerator EventOne()
    {
        // Narration
        nextButton.SetActive(false);

        ShowNarration("");
        StartCoroutine(FadeInRawImage(charVillageGirl, 3f));
        overlayFade.FadeIn();
        if (softRustle != null)
    {
  softRustle.loop = false;  // ensures it won’t repeat
softRustle.Play();

    }
        textToSpeak = "He turned his head. A tiny figure stood before him. A little girl — alive, but nearly translucent from weakness. Her lips were cracked with thirst, her cheeks hollow with hunger. Had she been here all along, among the lost spirits? Jin Chan bent toward her with concern, just as she parted her lips, letting out the weak hoarse voice.";
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 2;
    }

    IEnumerator EventTwo()
    {
        nextButton.SetActive(false);
        ShowDialogue("Girl", "");

        textToSpeak = "\"<i>Lost. Home… can’t walk. Village… past forest. I'm so scared…</i>\"";
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));

        nextButton.SetActive(true);
        eventPos = 3;
    }

IEnumerator EventThree()
{
    // Event 3
    nextButton.SetActive(false);
    textBox.SetActive(true);

    // Fade out character + overlay
    StartCoroutine(FadeOutRawImage(charVillageGirl, 2f));
    overlayFade.FadeOut();

    // Narration
    ShowNarration("");
    textToSpeak = "Then she fell silent. She would have cried if she had any strength left to cry. Without hesitation, Jin Chan lifted the girl onto his back. She clung to him weakly, as he leapt into the forest, toward the woods her thin hand pointed at.";

    // Start typewriter + fade music slightly before narration ends
    Coroutine typewriter = StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));

    // Delay a bit, then start fading music while narration is still happening
    yield return new WaitForSeconds(textToSpeak.Length * 0.03f - 3f); // start fading ~3s before text ends
    StartCoroutine(FadeOutMusic(3f)); // slow, cinematic fade

    // Wait until typewriter finishes completely
    yield return typewriter;

    // Short dramatic pause
    yield return new WaitForSeconds(0.3f);

    // Fade to black (fixed version)
    yield return StartCoroutine(FadeToBlack(2f));

    // Load next scene
    SceneManager.LoadScene("PoisonedVillage");
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
    }


}
