using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public float targetY = -2f;
    public float hitWindow = 0.5f;

    public bool canBePressed = false;
    private bool wasHit = false;

    public SongManager.NoteData noteData; 

    public void Init(SongManager.NoteData data, float speed)
    {
        noteData = data;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * speed;
    }

    void Update()
    {
        if (transform.position.y < -6f && !wasHit)
        {
            Missed();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Activator"))
    {
        canBePressed = true;
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Activator"))
    {
        canBePressed = false;
    }
}

    public void Pressed()
    {
        if (canBePressed && !wasHit)
        {
            wasHit = true;

            float distance = Mathf.Abs(transform.position.y - targetY);

            if (distance < 0.05f)
            {
            GameManager.instance.PerfectHit();
            } else if (distance < 0.15f)
            {
            GameManager.instance.GoodHit();
            } else
            {
            GameManager.instance.NormalHit();
            }

            Destroy(gameObject);
        }
    }

    private void Missed()
    {
        wasHit = true;
        GameManager.instance.NoteMissed();
        Destroy(gameObject);
    }
}
