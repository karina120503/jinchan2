using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;          // скорость движения
    public float distance = 2f;   // амплитуда (насколько далеко платформа уходит влево/вправо)
    public float jumpForce = 10f;    

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Сдвигаем платформу
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Если ушли дальше амплитуды – меняем направление
        if (Mathf.Abs(transform.position.x - startPos.x) >= distance)
        {
            direction *= -1;
        }
    }

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
