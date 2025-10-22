using UnityEngine;

public class FakePlatform : MonoBehaviour
{
     private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null && collision.relativeVelocity.y <= 0f)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;

            Destroy(gameObject);
        }
    }
}
