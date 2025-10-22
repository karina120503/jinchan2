using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BackToMapWithFade : MonoBehaviour
{
    public Image fadeImage;            // full-screen black image
    public float fadeDuration = 0.35f; // match your MapZoomController

    public void OnBackButtonPressed()
    {
        StartCoroutine(FadeAndLoad("MapMenu")); // replace with your map scene name
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
            Color c = fadeImage.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}

