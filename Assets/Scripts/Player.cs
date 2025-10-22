using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float smoothing = 5f;

    private float targetMovement = 0f;
    private float currentMovement = 0f;
    private Rigidbody2D rb;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        targetMovement = 0f;

        // === Управление мышью (в редакторе) ===
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x < Screen.width / 2)
                targetMovement = -movementSpeed;
            else
                targetMovement = movementSpeed;
        }

        // === Управление на телефоне ===
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                if (touchPos.x < Screen.width / 2)
                    targetMovement = -movementSpeed;
                else
                    targetMovement = movementSpeed;
            }
        }

        // === Плавная инерция ===
        currentMovement = Mathf.Lerp(currentMovement, targetMovement, Time.deltaTime * smoothing);
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = currentMovement;
        rb.linearVelocity = velocity;

        // === Ограничение по краям камеры ===
        Vector3 pos = transform.position;
        Vector3 leftEdge = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 rightEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, mainCamera.nearClipPlane));

        // оставим небольшой запас, чтобы жаба не касалась края
        float halfWidth = 0.5f; // подогнать под размер спрайта жабы
        pos.x = Mathf.Clamp(pos.x, leftEdge.x + halfWidth, rightEdge.x - halfWidth);

        transform.position = pos;
    }
}
