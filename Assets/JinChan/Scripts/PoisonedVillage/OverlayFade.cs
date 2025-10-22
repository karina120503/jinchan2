using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OverlayFade : MonoBehaviour
{
    public Image overlayImage;  
    public float fadeDuration = 2f;  
    public float targetAlpha = 2f; // how dark you want the overlay

    public void FadeIn()
    {
        StartCoroutine(FadeOverlay(overlayImage.color.a, targetAlpha));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOverlay(overlayImage.color.a, 0f));
    }

    private IEnumerator FadeOverlay(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = overlayImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            overlayImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        overlayImage.color = color;
    }
}
