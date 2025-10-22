using UnityEngine;

public class CleanUp : MonoBehaviour
{
     public float cleanupDistance = 15f;

    void Update()
    {
        if (Camera.main != null && 
            transform.position.y < Camera.main.transform.position.y - cleanupDistance)
        {
            Destroy(gameObject);
        }
    }
}
