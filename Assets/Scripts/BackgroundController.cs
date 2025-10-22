using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public SpriteRenderer darkBG;
    public SpriteRenderer lightBG;

    // На каком accuracy фон должен стать полностью светлым
    public float fullBrightAccuracy = 0.9f;

    void Update()
    {
        float accuracy = GameManager.instance.GetAccuracy();
        float t = Mathf.Clamp01(accuracy / fullBrightAccuracy);

        Color lightColor = lightBG.color;
        lightColor.a = t;
        lightBG.color = lightColor;
    }
}
