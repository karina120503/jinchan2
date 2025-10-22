using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    // The name of the scene you want to load
    public string sceneToLoad = "GhostMarket";

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
