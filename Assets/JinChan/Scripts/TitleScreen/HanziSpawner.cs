using UnityEngine;

public class HanziSpawner : MonoBehaviour
{
    public GameObject[] hanziPrefabs;
    public float spawnInterval = 0.3f;       // faster spawn
    public float fallSpeed = 100f;
    public RectTransform canvasRect;
    public RectTransform hanziParent;
    public int maxOnScreen = 60;

    private float timer;

    void Start()
    {
        if (canvasRect == null)
        {
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null) canvasRect = c.GetComponent<RectTransform>();
        }

        if (hanziParent == null && canvasRect != null)
        {
            Transform t = canvasRect.Find("HanziBackground");
            hanziParent = t != null ? t.GetComponent<RectTransform>() : canvasRect;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            // spawn 1–3 Hanzi per batch with random starting Y
            int batchCount = Random.Range(1, 4);
            for (int i = 0; i < batchCount; i++)
            {
                float startYOffset = Random.Range(0f, canvasRect.rect.height / 2f);
                SpawnHanzi(startYOffset);
            }
            timer = 0f;
        }
    }

    void SpawnHanzi(float yOffset)
    {
        if (hanziParent == null || hanziPrefabs.Length == 0) return;
        if (hanziParent.childCount >= maxOnScreen) return;

        GameObject prefab = hanziPrefabs[Random.Range(0, hanziPrefabs.Length)];
        GameObject hanzi = Instantiate(prefab);
        RectTransform rt = hanzi.GetComponent<RectTransform>();
        rt.SetParent(hanziParent, false);

        // Random horizontal and vertical position
        float halfWidth = canvasRect.rect.width / 2f;
        float randomX = Random.Range(-halfWidth * 0.9f, halfWidth * 0.9f);
        float spawnY = canvasRect.rect.height / 2f + yOffset;
        rt.anchoredPosition = new Vector2(randomX, spawnY);

        // Random size: mostly medium, some small or bigger
        float rand = Random.value;
        float t;
        if (rand < 0.2f) t = 0.2f;       // small
        else if (rand < 0.8f) t = 0.5f;  // medium
        else t = 0.9f;                    // slightly bigger
        float size = Mathf.Lerp(120f, 200f, t);
        rt.sizeDelta = new Vector2(size, size);

        // Add/configure HanziFall
        HanziFall hf = hanzi.GetComponent<HanziFall>();
        if (hf == null) hf = hanzi.AddComponent<HanziFall>();
        hf.fallSpeed = fallSpeed;
    }
}

   


