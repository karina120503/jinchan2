using UnityEngine;

public class ButtonController : MonoBehaviour
{
   // public KeyCode keyToPress; 
    public int lane; // номер лэйна (0-3)
    private SpriteRenderer sr;
    public Sprite defaultSprite;
    public Sprite pressedSprite;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = defaultSprite;
    }

    public void SetPressedVisual()
    {
        sr.sprite = pressedSprite;
        CancelInvoke(nameof(ResetSprite));
        Invoke(nameof(ResetSprite), 0.1f);
    }

    private void ResetSprite()
    {
        sr.sprite = defaultSprite;
    }

    void Update()
    {
    if (Input.GetMouseButtonDown(0)) 
    {
        Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            SetPressedVisual();

            NoteObject note = FindClosestNote();
            note?.Pressed();
        }
    }
    }

    private NoteObject FindClosestNote()
    {
        NoteObject[] notes = FindObjectsOfType<NoteObject>();
        NoteObject closest = null;
        float minDistance = float.MaxValue;

        foreach (NoteObject n in notes)
        {
            if (n == null) continue;
            if (n.noteData.lane != lane) continue;

            float distance = Mathf.Abs(n.transform.position.y - n.targetY);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = n;
            }
        }

        return closest;
    }
}
