using UnityEngine;

public class ChapterButton : MonoBehaviour
{
    public MapZoomController mapZoomController;  // Drag your MapManager here
    public Transform targetPoint;                // Drag the Well or Forest transform here
    public string sceneName;                     // Leave empty if you don't want Start button

    public void OnChapterClicked()
    {
        if (mapZoomController != null)
        {
            mapZoomController.ZoomAndLoad(targetPoint, sceneName);
        }
        else
        {
            Debug.LogWarning("MapZoomController not assigned on " + gameObject.name);
        }
    }
}
