using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalPlatform : MonoBehaviour
{
    private bool triggered = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!triggered && collision.gameObject.CompareTag("Player"))
        {
            triggered = true;
            Time.timeScale = 0f;
            Debug.Log("🏁 Победа! Переход к следующей сцене!");
            SceneManager.LoadScene("Monastery2");
        }
    }


}
