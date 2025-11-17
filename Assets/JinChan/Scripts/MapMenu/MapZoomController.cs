using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MapZoomController : MonoBehaviour
{
    [Header("References")]
    public RectTransform mapRect;      // assign MapContainer RectTransform
    public Image fadeImage;            // optional full-screen black image
    public GameObject backButton;      // assign BackButton UI here
    public GameObject startButton;     // assign StartButton UI here

    // new: parent that contains all chapter name UI images
    public GameObject chapterNames;    // assign the parent GameObject that holds all name images

    [Header("Timing & feel")]
    public float zoomScale = 1.8f;
    public float zoomDuration = 0.8f;
    public float smoothTime = 0.18f;
    public float fadeDuration = 0.35f;
    public bool testMode = true;       // set false when ready to load scenes

    // internals
    private Vector2 originalAnchored;
    private float originalScale;
    private Vector3 scaleVelocity = Vector3.zero;
    private Vector3 posVelocity = Vector3.zero;

    // pending scene to load only after Start pressed
    private string pendingSceneName = "";

    void Awake()
    {
        if (mapRect == null) Debug.LogWarning("MapRect not assigned on MapZoomController.");
        if (mapRect != null)
        {
            originalAnchored = mapRect.anchoredPosition;
            originalScale = mapRect.localScale.x;
        }

        // Ensure buttons hidden at start
        if (backButton != null) backButton.SetActive(false);
        if (startButton != null) startButton.SetActive(false);

        // Ensure chapter names are visible at start (if assigned)
        if (chapterNames != null) chapterNames.SetActive(true);
    }

    // Public API: call when chapter clicked. NOTE: this no longer auto-loads the scene.
    public void ZoomAndLoad(Transform targetTransform, string sceneName)
    {
        if (mapRect == null) return;
        StopAllCoroutines();
        pendingSceneName = sceneName ?? "";
        // Hide buttons while zooming
        if (backButton != null) backButton.SetActive(false);
        if (startButton != null) startButton.SetActive(false);

        // Hide chapter names while zooming in
        if (chapterNames != null) chapterNames.SetActive(false);

        StartCoroutine(SmoothZoomToPoint(targetTransform.position, zoomScale));
    }

    // Called by Back button (assign in inspector): zoom out and hide UI
    public void ZoomOut()
    {
        if (backButton != null) backButton.SetActive(false);
        if (startButton != null) startButton.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(SmoothZoomToState(originalAnchored, originalScale));
        pendingSceneName = ""; // cancel pending scene
    }

    // Called by Start button (assign in inspector)
    public void OnStartButtonPressed()
    {
        if (string.IsNullOrEmpty(pendingSceneName))
        {
            Debug.LogWarning("No pending scene to load.");
            return;
        }

        // optional fade, then load
        StartCoroutine(FadeAndLoad(pendingSceneName));
    }

// Fade coroutine + actual load (respects testMode)
private IEnumerator FadeAndLoad(string sceneName)
{
    if (fadeImage != null)
    {
        fadeImage.raycastTarget = true;
        Color c = fadeImage.color;
        float elapsed = 0f;

        // fade to black
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    // ⏳ NEW: short pause to let music fade finish before loading next scene
    yield return new WaitForSeconds(1f);  // tweak between 0.5f–1.5f depending on your music fade duration

    if (!testMode)
    {
        SceneManager.LoadScene(sceneName);
    }
    else
    {
        Debug.Log("[TestMode] Would load scene: " + sceneName);
    }
}


    // Smoothly zoom to a world point and then show UI (Start/Back) when done
    private IEnumerator SmoothZoomToPoint(Vector3 worldTargetPos, float targetScale)
    {
        // Convert world -> local
        Vector3 localTarget = mapRect.InverseTransformPoint(worldTargetPos);
        Vector3 targetScaleV = Vector3.one * targetScale;
        Vector2 targetAnchored = -new Vector2(localTarget.x, localTarget.y) * targetScale;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            // Smooth scale
            mapRect.localScale = Vector3.SmoothDamp(mapRect.localScale, targetScaleV, ref scaleVelocity, smoothTime);

            // Smooth anchored position
            Vector3 currentAnch = (Vector3)mapRect.anchoredPosition;
            Vector3 newAnch = Vector3.SmoothDamp(currentAnch, (Vector3)targetAnchored, ref posVelocity, smoothTime);
            mapRect.anchoredPosition = newAnch;

            yield return null;
        }

        // finalize
        mapRect.localScale = Vector3.one * targetScale;
        mapRect.anchoredPosition = targetAnchored;

        // Zoom finished — show BackButton & StartButton depending on pendingSceneName
        if (backButton != null) backButton.SetActive(true);
        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            if (startButton != null) startButton.SetActive(true);
        }
        // note: chapter names remain hidden while zoomed in
    }

    // Smoothly zoom back to given anchored/scale
    private IEnumerator SmoothZoomToState(Vector2 anchoredTarget, float scaleTarget)
    {
        Vector3 targetScaleV = Vector3.one * scaleTarget;
        Vector3 targetAnch = anchoredTarget;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            mapRect.localScale = Vector3.SmoothDamp(mapRect.localScale, targetScaleV, ref scaleVelocity, smoothTime);

            Vector3 currentAnch = (Vector3)mapRect.anchoredPosition;
            Vector3 newAnch = Vector3.SmoothDamp(currentAnch, (Vector3)targetAnch, ref posVelocity, smoothTime);
            mapRect.anchoredPosition = newAnch;

            yield return null;
        }

        // finalize
        mapRect.localScale = Vector3.one * scaleTarget;
        mapRect.anchoredPosition = anchoredTarget;

        // show chapter names again after fully zoomed out
        if (chapterNames != null) chapterNames.SetActive(true);
    }

    // Optional helper to reset immediately
    public void ResetMapImmediate()
    {
        if (mapRect == null) return;
        mapRect.localScale = Vector3.one * originalScale;
        mapRect.anchoredPosition = originalAnchored;
        scaleVelocity = Vector3.zero;
        posVelocity = Vector3.zero;
        pendingSceneName = "";
        if (backButton != null) backButton.SetActive(false);
        if (startButton != null) startButton.SetActive(false);
        if (chapterNames != null) chapterNames.SetActive(true);
    }
}

