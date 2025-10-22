using System.Collections;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;
    public int xIndex;
    public int yIndex;
    public bool isMatched;
    public bool isMoving;

    private PotionBoard board;
    private SpriteRenderer spriteRenderer;

    private Color originalColor;
    private Color matchedColor = Color.red; // color to flash when matched

    private Vector2 dragStartPos;
    private bool isDragging = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Init(PotionBoard _board)
    {
        board = _board;
    }

    public void SetIndices(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;
        Vector2 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector2.Lerp(startPosition, _targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }

    public void MarkAsMatched()
    {
        isMatched = true;
        StartCoroutine(MatchFlash());
    }

    private IEnumerator MatchFlash()
    {
        float flashDuration = 0.5f;
        float flashTime = 0f;

        while (flashTime < flashDuration)
        {
            spriteRenderer.color = Color.Lerp(originalColor, matchedColor, Mathf.PingPong(flashTime * 5f, 1));
            flashTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    private void OnMouseDown()
    {
        if (board != null && !isMoving)
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            board.SelectPotion(this);
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isMoving) return;

        Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = currentPos - dragStartPos;

        if (dragVector.magnitude > 0.5f) // threshold to trigger swap
        {
            Vector2Int direction;

            if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
                direction = dragVector.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                direction = dragVector.y > 0 ? Vector2Int.up : Vector2Int.down;

            Potion targetPotion = board.GetPotionAt(xIndex + direction.x, yIndex + direction.y);
            if (targetPotion != null)
            {
                board.SelectPotion(targetPotion); // second selection triggers swap
                isDragging = false;
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}

public enum PotionType
{
    Diamond,
    Money,
    Potion,
    Pouch,
    Scrolls,
}
