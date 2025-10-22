using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [Header("Backgrounds")]
    public SpriteRenderer baseBackground;    // исходный фон
    public SpriteRenderer overlayBackground; // фон, который проявляется

    [Header("Transition Settings")]
    public ScoreManager scoreManager;
    public int startScore = 500;    // счёт, с которого начинается переход
    public int endScore = 1500;     // счёт, при котором переход завершается

    void Update()
    {
        if (scoreManager == null || overlayBackground == null || baseBackground == null)
            return;

        int score = scoreManager.GetCurrentScore();

        // Вычисляем прогресс перехода от 0 до 1
        float t = Mathf.InverseLerp(startScore, endScore, score);

        // Меняем прозрачность второго фона (чем выше счёт, тем виднее он)
        Color c = overlayBackground.color;
        c.a = t; // 0 = полностью прозрачный, 1 = полностью видимый
        overlayBackground.color = c;
    }
}
