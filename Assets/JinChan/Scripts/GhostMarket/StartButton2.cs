using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton2 : MonoBehaviour
{
    // The name of the scene you want to load
    public string sceneToLoad = "TestCandy";

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
