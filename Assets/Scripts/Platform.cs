using UnityEngine;

public class Platform : MonoBehaviour
{
   public float jumpForce = 10f;

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.relativeVelocity.y <= 0f)
		{
			Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				Vector2 velocity = rb.linearVelocity;
				velocity.y = jumpForce;
				rb.linearVelocity = velocity;
			}
		}
	}
}
