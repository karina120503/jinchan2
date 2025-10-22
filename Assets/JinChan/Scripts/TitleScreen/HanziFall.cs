using UnityEngine;

public class HanziFall : MonoBehaviour
{
    public float fallSpeed = 100f;     // base speed
    public float fadeDistance = 100f;  // fade zone near bottom

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private float bottomY;
    private float topY;

    // Horizontal sway/drift
    private float horizontalDrift;
    private float driftSpeed;
    private float driftPhase;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        RectTransform canvas = rect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        bottomY = -canvas.rect.height / 2f;
        topY = canvas.rect.height / 2f;

        // Random horizontal sway
        horizontalDrift = Random.Range(20f, 50f);
        driftSpeed = Random.Range(0.5f, 1.5f);
        driftPhase = Random.Range(0f, 2 * Mathf.PI);

        // Slightly randomize fall speed
        fallSpeed *= Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        // Vertical fall
        rect.anchoredPosition -= new Vector2(0, fallSpeed * Time.deltaTime);

        // Horizontal sway
        rect.anchoredPosition += new Vector2(
            Mathf.Sin(Time.time * driftSpeed + driftPhase) * horizontalDrift * Time.deltaTime,
            0
        );

        // Fade near bottom
        if (rect.anchoredPosition.y < bottomY + fadeDistance)
        {
            float t = Mathf.InverseLerp(bottomY, bottomY + fadeDistance, rect.anchoredPosition.y);
            canvasGroup.alpha = t;
            rect.localScale = Vector3.one * t; // optional: shrink near bottom
        }

        // Recycle to top for continuous rain
        if (rect.anchoredPosition.y < bottomY - 50f)
        {
            float newX = Random.Range(-rect.GetComponentInParent<RectTransform>().rect.width / 2f,
                                       rect.GetComponentInParent<RectTransform>().rect.width / 2f);
            rect.anchoredPosition = new Vector2(newX, topY + Random.Range(50f, 150f));

            // reset fade/scale
            canvasGroup.alpha = 1f;
            rect.localScale = Vector3.one;

            // refresh drift for randomness
            horizontalDrift = Random.Range(20f, 50f);
            driftSpeed = Random.Range(0.5f, 1.5f);
            driftPhase = Random.Range(0f, 2 * Mathf.PI);
        }
    }
}
