using UnityEngine;

public class BreakOnUsePlatform : MonoBehaviour
{
     public float jumpForce = 10f;

    private bool used = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!used && collision.collider.GetComponent<Player>() != null && collision.relativeVelocity.y <= 0f)
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;
            }
            used = true; 

            Destroy(gameObject, 0.05f);
        }
    }
}
