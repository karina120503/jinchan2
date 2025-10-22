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
        // event 0
        yield return new WaitForSeconds(2);
        fadeScreenIn.SetActive(false);
        yield return new WaitForSeconds(2);
        // this is where our text function will go in future tutorial
        lightRain.Play();
        mainTextObject.SetActive(true);
        textToSpeak = "The ritual was complete. Jin Chan opened his eyes. A single drop of rain fell on the newly reborn soil. Then another. One by one, they began to strike the rooftops and old pagodas, drumming like quiet applause. The earth drank them greedily. He felt the water roll down his thick skin, soaking through the folds of his robe. Then a bamboo umbrella opened above him. A man stood beside him. His face looked younger now.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 1;

    }
    
        IEnumerator EventOne()
    {
        // event 13
        nextButton.SetActive(false);
        textBox.SetActive(true);
        overlayFade.FadeIn();
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>The rain will wash away the last of the dark energy. After the rain, flowers will bloom.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "The rain", charJinChan, 1f));
        nextButton.SetActive(true);
        eventPos = 2;
    }

        IEnumerator EventTwo()
    {
        // event 2
        nextButton.SetActive(false);
        textBox.SetActive(true);
        overlayFade.FadeOut();
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        ShowNarration("");
        textToSpeak = "The villagers, who had watched the ritual in silence, now looked up. Something in their faces had changed. Color had returned to their cheeks. Eyes once hollow with despair now glistened. Shoulders once curled by humility now straightened.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 3;
    }

        IEnumerator EventThree()
    {
        // event 3
        nextButton.SetActive(false);
        textBox.SetActive(true);
        overlayFade.FadeIn();
        StartCoroutine(FadeInRawImage(charElder, 1f));
        ShowDialogue("Elder Man", "");
        textToSpeak = "\"<i>I will wait for them.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f, "I will", charElder, 1f));
        nextButton.SetActive(true);
        eventPos = 4;
    }

        IEnumerator EventFour()
    {
        // event 4
        nextButton.SetActive(false);
        textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charElder, 1f));
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowNarration("");
        textToSpeak = "A toad felt a gentle tug on his heart. A small dream. He wished Liu Hai could see them too.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 5;
    }

        IEnumerator EventFive()
    {
        // event 17
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>Have you heard of Liu Hai?</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 6;
    }

        IEnumerator EventSix()
    {
        // event 18
        nextButton.SetActive(false);
        textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        StartCoroutine(FadeInRawImage(charElder, 1f));
        ShowNarration("");
        textToSpeak = "The man looked off into the distance for a moment.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 7;
    }

        IEnumerator EventSeven()
    {
        // event 19
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowDialogue("Elder Man", "");
        textToSpeak = "\"<i>Liu Hai came here many years ago. Back when this village was still breathing. He studied in the mountains, there is a monastery.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 8;
    }

    IEnumerator EventEight()
    {
        // event 8
        nextButton.SetActive(false);
        textBox.SetActive(true);
        StartCoroutine(FadeOutRawImage(charElder, 3f));
        ShowNarration("");
        textToSpeak = "Then that is where Jin Chan would go.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 9;
    }

        IEnumerator EventNine()
    {
        // event 21
        nextButton.SetActive(false);
        textBox.SetActive(true);
        StartCoroutine(FadeInRawImage(charJinChan, 1f));
        ShowDialogue("Jin Chan", "");
        textToSpeak = "\"<i>Thank you.</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 10;
    }

        IEnumerator EventTen()
    {
        // event 22
        nextButton.SetActive(false);
        textBox.SetActive(true);
        overlayFade.FadeOut();
        StartCoroutine(FadeOutRawImage(charJinChan, 1f));
        ShowNarration("");
        textToSpeak = "The elder said nothing more. He simply handed the bamboo umbrella to Jin Chan. The rain was already beginning to fade. It was time to move on.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        SceneManager.LoadScene("CalligraphyMonastery");
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

    }



}
