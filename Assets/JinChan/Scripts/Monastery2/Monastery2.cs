using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monastery2 : MonoBehaviour
{
    public GameObject fadeScreenIn;
    public GameObject textBox;
    public GameObject charJinChan;
    public GameObject charMonk;
    public OverlayFade9 overlayFade;
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
        mainTextObject.SetActive(true);
        textToSpeak = "The scent of ink still hung in the air. Broad brushstrokes shimmered faintly, not yet dried upon the paper. Jin Chan gently set the brush on its stand, beside the unrolled scroll. It felt as though everything inside had been poured into it — every thought, every memory, every fragment of guilt. Within him, as outside him — nothing remained. Toad flinched slightly at the foreign voice that broke the quiet space.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(tmpText, textToSpeak, 0.03f));  // 0.03 delay can be adjusted
        nextButton.SetActive(true);
        eventPos = 1;

    }


IEnumerator EventOne()
{
    // Dialogue: Jin Chan
    nextButton.SetActive(false);
    ShowDialogue("Monk", "");
    StartCoroutine(FadeInRawImage(charMonk, 1f));
    overlayFade.FadeIn();
    textToSpeak = "\"<i>There is one last step to take...</i>\"";
    // (If you want to trigger on a phrase, change "The toad" to a phrase that actually exists in this line.)
    yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
    nextButton.SetActive(true);
    eventPos = 2;
}

        IEnumerator EventTwo()
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowNarration("");
        textToSpeak = "A wide sleeve moved gently through the air as the monk gestured upward, to where the cliffs vanished into clouds.";
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
        ShowDialogue("Monk", "");

        textToSpeak = "\"<i>“His dwelling lies where no stairs lead. But you know the way. Liu Hai awaits you.”</i>\"";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(dialogueText, textToSpeak, 0.03f));
        nextButton.SetActive(true);
        eventPos = 4; 
    }

        IEnumerator EventFour()
    {
        // event 4
        nextButton.SetActive(false);
        textBox.SetActive(true);
        ShowNarration("");
        StartCoroutine(FadeOutRawImage(charMonk, 4f));
        overlayFade.FadeOut();
        textToSpeak = "Toad nodded. Thanking the monks, he stepped outside the hall, webbed paws treading the damp ground as the fog around him grew thicker and thicker. A white veil obscured his gaze like another membrane. And soon he realized: he was no longer walking on his own.";
        TMP_Text tmpText = textBox.GetComponent<TMP_Text>();
        yield return StartCoroutine(TypewriterEffect(narrationText, textToSpeak, 0.03f)); 
        nextButton.SetActive(true);
        SceneManager.LoadScene("HeavenlyCourt");
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

    }

}   

    
