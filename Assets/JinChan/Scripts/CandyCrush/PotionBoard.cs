using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    public int width = 5;
    public int height = 5;

    public float spacingX;
    public float spacingY;

    public GameObject[] potionPrefabs;
    private Node[,] potionBoard;
    public GameObject potionboardGO;

    public List<GameObject> potionsToDestroy = new();

    [SerializeField] private Potion selectedPotion;
    [SerializeField] private bool isProcessingMove;

    public ArrayLayout arrayLayout;
    public static PotionBoard Instance;

    private int shrinkingPotionsCount = 0; // Track how many potions are shrinking

    // safety limits
    private const int MAX_ATTEMPTS_PER_CELL = 60;

    // timeout (seconds) we will allow waiting for shrinking operations to finish
    private const float SHRINK_WAIT_TIMEOUT = 2.5f;

    private void Awake()
    {
        Instance = this;
    }

void Start()
{
    int attempts = 0;
    int maxAttempts = 10;

    InitializeBoard();

    while (HasAnyMatchesOnBoard() && attempts < maxAttempts)
    {
        Debug.LogWarning($"Initial board had matches, regenerating... Attempt #{attempts + 1}");
        InitializeBoard(); // no need to DestroyPotions() here
        attempts++;
    }

    if (attempts >= maxAttempts)
    {
        Debug.LogError("Could not generate initial board without matches after max attempts.");
    }
}


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Potion potion = hit.collider.GetComponent<Potion>();
                if (potion != null && !isProcessingMove)
                {
                    Debug.Log("Clicked potion: " + potion.name);
                    SelectPotion(potion);
                }
            }
        }
    }

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        // compute spacing center offsets (kept from your original)
        spacingX = (float)(width - 1) / 2;
        spacingY = (float)(height - 1) / 2;

        float spacing = 1.4f;
        float yOffset = -2f;

        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            Debug.LogError("Potion prefabs not assigned! Assign prefabs in Inspector.");
            return;
        }

        // Fill board: place potions one-by-one ensuring no immediate 3-match is created
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int attempts = 0;
                GameObject chosenPrefab = null;

                while (attempts < MAX_ATTEMPTS_PER_CELL)
                {
                    attempts++;
                    int idx = Random.Range(0, potionPrefabs.Length);
                    chosenPrefab = potionPrefabs[idx];

                    if (!PrefabWouldMakeMatchAt(x, y, chosenPrefab))
                    {
                        break; // good choice
                    }
                    chosenPrefab = null; // try again
                }

                if (chosenPrefab == null)
                {
                    // if we exhausted attempts, pick any prefab (safety fallback)
                    chosenPrefab = potionPrefabs[Random.Range(0, potionPrefabs.Length)];
                    Debug.LogWarning($"Max attempts reached for cell ({x},{y}), placing fallback prefab.");
                }

                Vector2 position = new Vector2((x - spacingX) * spacing, (y - spacingY) * spacing + yOffset);
                GameObject potionGO = Instantiate(chosenPrefab, position, Quaternion.identity);
                // parent under board GO if available
                if (potionboardGO != null) potionGO.transform.SetParent(potionboardGO.transform);

                Potion potionComp = potionGO.GetComponent<Potion>();
                if (potionComp != null)
                {
                    potionComp.Init(this); // link Potion to this board
                    potionComp.SetIndices(x, y);
                    potionBoard[x, y] = new Node(true, potionGO);
                    potionsToDestroy.Add(potionGO);
                }
                else
                {
                    Debug.LogError("⚠ Missing Potion script on prefab: " + potionGO.name);
                    potionBoard[x, y] = new Node(false, null);
                }
            }
        }

        Debug.Log("Board initialized — no immediate matches at start.");
    }

private bool PrefabWouldMakeMatchAt(int x, int y, GameObject prefab)
{
    Potion prefabPotion = prefab.GetComponent<Potion>();
    if (prefabPotion == null)
        return true; // safety: if prefab missing script, avoid using it

    PotionType type = prefabPotion.potionType;

    // 🔹 Horizontal check (left and right)
    // left 2
    if (x >= 2)
    {
        Potion p1 = potionBoard[x - 1, y]?.potion?.GetComponent<Potion>();
        Potion p2 = potionBoard[x - 2, y]?.potion?.GetComponent<Potion>();
        if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
            return true;
    }

    // right 2
    if (x <= width - 3)
    {
        Potion p1 = potionBoard[x + 1, y]?.potion?.GetComponent<Potion>();
        Potion p2 = potionBoard[x + 2, y]?.potion?.GetComponent<Potion>();
        if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
            return true;
    }

    // 🔹 Vertical check (up and down)
    // down 2
    if (y >= 2)
    {
        Potion p1 = potionBoard[x, y - 1]?.potion?.GetComponent<Potion>();
        Potion p2 = potionBoard[x, y - 2]?.potion?.GetComponent<Potion>();
        if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
            return true;
    }

    // up 2
    if (y <= height - 3)
    {
        Potion p1 = potionBoard[x, y + 1]?.potion?.GetComponent<Potion>();
        Potion p2 = potionBoard[x, y + 2]?.potion?.GetComponent<Potion>();
        if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
            return true;
    }

    return false; // ✅ safe to place
}

    private void DestroyPotions()
    {
        foreach (GameObject potion in potionsToDestroy)
        {
            if (potion != null) Destroy(potion);
        }
        potionsToDestroy.Clear();
    }
    
bool HasAnyMatchesOnBoard()
{
    // Loop through the board
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable || potionBoard[x, y].potion == null)
                continue;

            Potion current = potionBoard[x, y].potion.GetComponent<Potion>();
            if (current == null) continue;

            PotionType type = current.potionType;

            // --- Check horizontal run ---
            int runLength = 1;
            // look right
            int checkX = x + 1;
            while (checkX < width && potionBoard[checkX, y]?.potion != null &&
                   potionBoard[checkX, y].potion.GetComponent<Potion>()?.potionType == type)
            {
                runLength++;
                checkX++;
            }
            // look left
            checkX = x - 1;
            while (checkX >= 0 && potionBoard[checkX, y]?.potion != null &&
                   potionBoard[checkX, y].potion.GetComponent<Potion>()?.potionType == type)
            {
                runLength++;
                checkX--;
            }
            if (runLength >= 3)
                return true;

            // --- Check vertical run ---
            runLength = 1;
            // look up
            int checkY = y + 1;
            while (checkY < height && potionBoard[x, checkY]?.potion != null &&
                   potionBoard[x, checkY].potion.GetComponent<Potion>()?.potionType == type)
            {
                runLength++;
                checkY++;
            }
            // look down
            checkY = y - 1;
            while (checkY >= 0 && potionBoard[x, checkY]?.potion != null &&
                   potionBoard[x, checkY].potion.GetComponent<Potion>()?.potionType == type)
            {
                runLength++;
                checkY--;
            }
            if (runLength >= 3)
                return true;
        }
    }

    return false; // no matches found
}

    public bool CheckBoard()
    {
        bool hasMatched = false;

        // Reset all potions' isMatched flags first
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion pot = potionBoard[x, y].potion.GetComponent<Potion>();
                    if (pot != null) pot.isMatched = false;
                }
            }
        }

        // Now check for matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion == null) continue;

                    MatchResult matchedPotions = IsConnected(potion);

                    if (matchedPotions != null && matchedPotions.connectedPotions.Count >= 3)
                    {
                        MatchResult superMatchedPotions = SuperMatch(matchedPotions);
                        if (superMatchedPotions != null)
                        {
                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                            {
                                Debug.Log($"Marking potion as matched at ({pot.xIndex},{pot.yIndex}) of type {pot.potionType}");
                                pot.isMatched = true; // ✅ Only these get marked!
                            }
                        hasMatched = true;
                        }
                    }

                }
            }
        }
        return hasMatched;
    }

private void ClearMatchedPotions()
{
    Debug.Log("ClearMatchedPotions() called");
    shrinkingPotionsCount = 0;

    // 🔹 Step 1: Collect only matched potions into a list
    List<(Potion potion, int x, int y)> matchedPotions = new List<(Potion, int, int)>();

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
            {
                Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                if (potion != null && potion.isMatched)
                {
                    matchedPotions.Add((potion, x, y));
                }
            }
        }
    }

    // 🔹 Step 2: Shrink only the matched ones
    foreach (var match in matchedPotions)
    {
        shrinkingPotionsCount++;
        StartCoroutine(ShrinkAndDestroyPotionSafe(match.potion, match.x, match.y));
    }

    Debug.Log($"Started clearing {shrinkingPotionsCount} matched potions.");

    // 🔹 Step 3: refill only once (not per potion!)
    if (shrinkingPotionsCount > 0)
        StartCoroutine(WaitAndRefill());
}

private IEnumerator WaitAndRefill()
{
    // wait a bit longer than shrink duration to ensure all destroyed
    yield return new WaitForSeconds(0.25f);

    RefillBoard();
    Debug.Log("Board has been refilled!");
}


private IEnumerator ShrinkAndDestroyPotionSafe(Potion potion, int x, int y)
{
    if (potion == null) yield break;

    float shrinkDuration = 0.2f;
    Vector3 originalScale = potion.transform.localScale;
    float elapsed = 0f;

    // Smoothly shrink
    while (elapsed < shrinkDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / shrinkDuration;
        potion.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
        yield return null;
    }

    potion.transform.localScale = Vector3.zero;

    // Destroy safely
    if (potion != null)
    {
        Destroy(potion.gameObject);
        Debug.Log($"Destroyed potion at ({x},{y})");
    }

    // Mark slot as empty
    if (potionBoard[x, y] != null)
    {
        potionBoard[x, y].potion = null;
        potionBoard[x, y].isUsable = false;
    }

    // Decrement shrinking count
    shrinkingPotionsCount = Mathf.Max(0, shrinkingPotionsCount - 1);
    Debug.Log($"Shrink finished for ({x},{y}). Remaining shrinking: {shrinkingPotionsCount}");
}


    // original shrink coroutine (kept mostly as-is). It decrements count in finally to be robust.
    private IEnumerator ShrinkAndDestroyPotion(Potion potion, int x, int y)
    {
        float duration = 0.3f;
        Vector3 startScale = potion.transform.localScale;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (potion != null && potion.transform != null)
                potion.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        if (potion != null && potion.transform != null)
            potion.transform.localScale = endScale;

        // destroy and clear safely
        if (potion != null)
        {
            Destroy(potion.gameObject);
        }

        if (potionBoard[x, y] != null)
        {
            potionBoard[x, y].potion = null;
            potionBoard[x, y].isUsable = true;
        }

        // decrement (this is the primary decrement)
        shrinkingPotionsCount = Mathf.Max(0, shrinkingPotionsCount - 1);
        Debug.Log($"Destroyed potion at ({x},{y}). Remaining shrinking: {shrinkingPotionsCount}");
    }

private void RefillBoard()
{
    Debug.Log("RefillBoard() called");

    float spacing = 1.4f;
    float yOffset = -2f;

    List<Vector2Int> emptyCells = new List<Vector2Int>();

    // Step 1: Collapse existing potions down
    for (int x = 0; x < width; x++)
    {
        int writeY = 0; // next available slot from bottom

        for (int y = 0; y < height; y++)
        {
            Node node = potionBoard[x, y];
            if (node != null && node.potion != null)
            {
                if (y != writeY)
                {
                    // Move potion down to writeY
                    Potion pComp = node.potion.GetComponent<Potion>();
                    Vector2 targetPos = new Vector2((x - spacingX) * spacing, (writeY - spacingY) * spacing + yOffset);
                    pComp.MoveToTarget(targetPos);
                    pComp.SetIndices(x, writeY);

                    potionBoard[x, writeY].potion = node.potion;
                    node.potion = null;
                }
                writeY++;
            }
        }

        // Step 2: Mark remaining cells as empty
        for (int y = writeY; y < height; y++)
        {
            potionBoard[x, y].potion = null;
            potionBoard[x, y].isUsable = false;
            emptyCells.Add(new Vector2Int(x, y));
        }
    }

    // Step 3: Spawn new potions at empty cells only
    foreach (var pos in emptyCells)
    {
        int x = pos.x;
        int y = pos.y;

        GameObject chosenPrefab = null;
        int attempts = 0;

        while (attempts < MAX_ATTEMPTS_PER_CELL)
        {
            attempts++;
            GameObject candidate = potionPrefabs[Random.Range(0, potionPrefabs.Length)];

            if (!PrefabWouldMakeMatchAtRefill(x, y, candidate))
            {
                chosenPrefab = candidate;
                break;
            }
        }

        if (chosenPrefab == null)
            chosenPrefab = potionPrefabs[Random.Range(0, potionPrefabs.Length)];

        Vector2 spawnPos = new Vector2((x - spacingX) * spacing, (y - spacingY) * spacing + yOffset + 2f);
        GameObject newPotion = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
        if (potionboardGO != null) newPotion.transform.SetParent(potionboardGO.transform);

        Potion potionComp = newPotion.GetComponent<Potion>();
        potionComp.Init(this);
        potionComp.isMatched = false;
        potionComp.SetIndices(x, y);
        potionComp.MoveToTarget(new Vector2((x - spacingX) * spacing, (y - spacingY) * spacing));
        newPotion.transform.localScale = Vector3.one;

        potionBoard[x, y].potion = newPotion;
        potionBoard[x, y].isUsable = true;

        Debug.Log($"Spawned new potion at ({x},{y})");
    }

    Debug.Log($"RefillBoard() finished. Spawned {emptyCells.Count} new potions.");
}


    // This check is used when refilling to avoid creating immediate matches (looks at neighbors already in board)
    private bool PrefabWouldMakeMatchAtRefill(int x, int y, GameObject prefab)
    {
        Potion prefabPotion = prefab.GetComponent<Potion>();
        if (prefabPotion == null) return true;
        PotionType type = prefabPotion.potionType;

        // check left neighbors (x-1, x-2)
        if (x >= 2)
        {
            if (potionBoard[x - 1, y] != null && potionBoard[x - 2, y] != null &&
                potionBoard[x - 1, y].potion != null && potionBoard[x - 2, y].potion != null)
            {
                Potion p1 = potionBoard[x - 1, y].potion.GetComponent<Potion>();
                Potion p2 = potionBoard[x - 2, y].potion.GetComponent<Potion>();
                if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
                    return true;
            }
        }

        // check right-left combos (x-1 and x+1)
        if (x >= 1 && x + 1 < width)
        {
            if (potionBoard[x - 1, y] != null && potionBoard[x - 1, y].potion != null && potionBoard[x + 1, y] != null && potionBoard[x + 1, y].potion != null)
            {
                Potion p1 = potionBoard[x - 1, y].potion.GetComponent<Potion>();
                Potion p2 = potionBoard[x + 1, y].potion.GetComponent<Potion>();
                if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
                    return true;
            }
        }

        // check down neighbors (y-1, y-2)
        if (y >= 2)
        {
            if (potionBoard[x, y - 1] != null && potionBoard[x, y - 2] != null &&
                potionBoard[x, y - 1].potion != null && potionBoard[x, y - 2].potion != null)
            {
                Potion p1 = potionBoard[x, y - 1].potion.GetComponent<Potion>();
                Potion p2 = potionBoard[x, y - 2].potion.GetComponent<Potion>();
                if (p1 != null && p2 != null && p1.potionType == type && p2.potionType == type)
                    return true;
            }
        }

        return false;
    }

private MatchResult SuperMatch(MatchResult _matchedResults)
{
    if (_matchedResults == null) return null;

    if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
    {
        foreach (Potion pot in _matchedResults.connectedPotions)
        {
            List<Potion> extraConnectedPotions = new();
            CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
            CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

            if (extraConnectedPotions.Count >= 2)
            {
                // ✅ new list so we don’t merge multiple results incorrectly
                List<Potion> finalList = new List<Potion>(_matchedResults.connectedPotions);
                finalList.AddRange(extraConnectedPotions);

                return new MatchResult { connectedPotions = finalList, direction = MatchDirection.Super };
            }
        }
        return _matchedResults;
    }
    else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
    {
        foreach (Potion pot in _matchedResults.connectedPotions)
        {
            List<Potion> extraConnectedPotions = new();
            CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
            CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

            if (extraConnectedPotions.Count >= 2)
            {
                List<Potion> finalList = new List<Potion>(_matchedResults.connectedPotions);
                finalList.AddRange(extraConnectedPotions);

                return new MatchResult { connectedPotions = finalList, direction = MatchDirection.Super };
            }
        }
        return _matchedResults;
    }
    return null;
}

MatchResult IsConnected(Potion potion)
{
    List<Potion> connectedPotions = new();
    connectedPotions.Add(potion);

    // Horizontal check
    CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
    CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);

    if (connectedPotions.Count >= 3)
    {
        return new MatchResult
        {
            connectedPotions = new List<Potion>(connectedPotions),
            direction = connectedPotions.Count == 3 ? MatchDirection.Horizontal : MatchDirection.LongHorizontal
        };
    }

    // Vertical check
    connectedPotions.Clear();
    connectedPotions.Add(potion);
    CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
    CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

    if (connectedPotions.Count >= 3)
    {
        return new MatchResult
        {
            connectedPotions = new List<Potion>(connectedPotions),
            direction = connectedPotions.Count == 3 ? MatchDirection.Vertical : MatchDirection.LongVertical
        };
    }

    // ❌ No match
    return null;
}


    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                if (neighbourPotion != null && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);
                    x += direction.x;
                    y += direction.y;
                }
                else break;
            }
            else break;
        }
    }

    #region Swapping Potions
    public void SelectPotion(Potion potion)
    {
        if (selectedPotion == null)
        {
            selectedPotion = potion;
            Debug.Log("Selected: " + potion.name);
        }
        else
        {
            Debug.Log("Trying to swap with: " + potion.name);
            SwapPotion(selectedPotion, potion);
            selectedPotion = null;
        }
    }

    public Potion GetPotionAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                return potionBoard[x, y].potion.GetComponent<Potion>();
        }
        return null;
    }

    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        if (!IsAdjacent(_currentPotion, _targetPotion))
        {
            Debug.Log("Not adjacent — no swap");
            return;
        }

        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;
        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        Vector3 currentWorldPos = _currentPotion.transform.position;
        Vector3 targetWorldPos = _targetPotion.transform.position;

        int currentX = _currentPotion.xIndex;
        int currentY = _currentPotion.yIndex;
        int targetX = _targetPotion.xIndex;
        int targetY = _targetPotion.yIndex;

        potionBoard[currentX, currentY].potion = _targetPotion.gameObject;
        potionBoard[targetX, targetY].potion = _currentPotion.gameObject;

        _currentPotion.xIndex = targetX;
        _currentPotion.yIndex = targetY;
        _targetPotion.xIndex = currentX;
        _targetPotion.yIndex = currentY;

        _currentPotion.MoveToTarget(targetWorldPos);
        _targetPotion.MoveToTarget(currentWorldPos);

        Debug.Log($"Swapped {_currentPotion.name} with {_targetPotion.name}");
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
{
    yield return new WaitUntil(() => !_currentPotion.isMoving && !_targetPotion.isMoving);

    bool hasMatch = CheckBoard();

    if (!hasMatch)
    {
        Debug.Log("No match — swapping back");
        DoSwap(_currentPotion, _targetPotion);
        yield return new WaitUntil(() => !_currentPotion.isMoving && !_targetPotion.isMoving);
    }
    else
    {
        Debug.Log("Match found!");

        int loopCounter = 1;
        int maxSafety = 10;

        do
        {
            Debug.Log($"Refilling... Loop #{loopCounter} at time {Time.time:F2}s");

            Debug.Log("Before RefillBoard call");  // <-- ADD THIS LINE

            ClearMatchedPotions();

            yield return StartCoroutine(WaitForShrinkingOrTimeout(SHRINK_WAIT_TIMEOUT));

            RefillBoard();

            Debug.Log("After RefillBoard call");  // <-- ADD THIS LINE

            yield return new WaitForSeconds(1f);
            loopCounter++;
        }
        while (CheckBoard() && loopCounter <= maxSafety);

        if (loopCounter > maxSafety)
        {
            Debug.LogWarning("Stopped refill loop after max safety iterations.");
        }
    }

    isProcessingMove = false;
}


    private IEnumerator WaitForShrinkingOrTimeout(float timeout)
    {
        float start = Time.time;
        while (shrinkingPotionsCount > 0)
        {
            if (Time.time - start > timeout)
            {
                Debug.LogWarning($"WaitForShrinkingOrTimeout timed out after {timeout}s (remaining shrinking: {shrinkingPotionsCount}). Continuing anyway.");
                yield break;
            }
            yield return null;
        }
        yield break;
    }

    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }
    #endregion
}

// small helper classes
public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}

public class Node
{
    public bool isUsable;
    public GameObject potion;

    public Node(bool usable, GameObject potionGO)
    {
        isUsable = usable;
        potion = potionGO;
    }
}
