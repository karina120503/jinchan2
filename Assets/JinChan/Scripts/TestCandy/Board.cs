using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    private bool _isBusy;

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    private readonly List<Tile> _selection = new List<Tile>();
    private const float TweenDuration = 0.25f;

    private void Awake() => Instance = this;

    private void Start()
    {
        Tiles = new Tile[rows.Max(r => r.tiles.Length), rows.Length];

        // Fill the board randomly
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];
                tile.x = x;
                tile.y = y;
                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                Tiles[x, y] = tile;
            }
        }

        // Ensure no matches exist at the start
        while (HasAnyMatches() || !HasAnyMoves())
        {
            ShuffleBoard();
        }
    }

    #region Board Checks

    private bool HasAnyMatches()
    {
        // Horizontal
        for (int y = 0; y < Height; y++)
        {
            int runLength = 1;
            for (int x = 1; x < Width; x++)
            {
                if (Tiles[x, y].Item == Tiles[x - 1, y].Item)
                    runLength++;
                else
                    runLength = 1;

                if (runLength >= 3)
                    return true;
            }
        }

        // Vertical
        for (int x = 0; x < Width; x++)
        {
            int runLength = 1;
            for (int y = 1; y < Height; y++)
            {
                if (Tiles[x, y].Item == Tiles[x, y - 1].Item)
                    runLength++;
                else
                    runLength = 1;

                if (runLength >= 3)
                    return true;
            }
        }

        return false;
    }

    private bool HasAnyMoves()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                if (tile == null) continue;

                if (x < Width - 1 && CanFormMatch(tile, Tiles[x + 1, y]))
                    return true;
                if (y < Height - 1 && CanFormMatch(tile, Tiles[x, y + 1]))
                    return true;
            }
        }
        return false;
    }

    private bool CanFormMatch(Tile a, Tile b)
    {
        var temp = a.Item;
        a.Item = b.Item;
        b.Item = temp;

        bool canMatch = a.GetConnectedTiles().Count >= 3 || b.GetConnectedTiles().Count >= 3;

        temp = a.Item;
        a.Item = b.Item;
        b.Item = temp;

        return canMatch;
    }

    #endregion

    #region Selection & Swap

    public async void Select(Tile tile)
    {
        if (_isBusy) return;
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsOver) return;

        if (!_selection.Contains(tile))
        {
            if (_selection.Count == 1)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1 && _selection[0] != tile)
                    _selection.Add(tile);
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if (_selection.Count < 2) return;

        var a = _selection[0];
        var b = _selection[1];
        _selection.Clear();

        _isBusy = true;

        try
        {
            await Swap(a, b);

            if (SwapCreatesMatch(a, b))
            {
                ChallengeManager.Instance?.RegisterMove();
                await PopAsync();
            }
            else
            {
                await Swap(a, b); // Swap back if no match
            }
        }
        finally
        {
            _isBusy = false;
            EndTurnCheck();
        }
    }

    private bool SwapCreatesMatch(Tile a, Tile b)
    {
        return a.GetConnectedTiles().Count >= 3 || b.GetConnectedTiles().Count >= 3;
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Start = icon1.transform.position;
        var icon2Start = icon2.transform.position;

        var sequence = DOTween.Sequence();
        sequence.Join(icon1.transform.DOMove(icon2Start, TweenDuration))
                .Join(icon2.transform.DOMove(icon1Start, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        // Snap
        icon1.transform.position = icon1Start;
        icon2.transform.position = icon2Start;

        // Swap items logically
        var tempItem = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tempItem;
    }

    #endregion

    #region Pop & Scoring

    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x, y].GetConnectedTiles().Count >= 3)
                    return true;

        return false;
    }

private async Task PopAsync()
{
    bool popped;

    do
    {
        popped = false;
        HashSet<Tile> tilesToPop = new HashSet<Tile>();

        // 🔹 Horizontal matches
        for (int y = 0; y < Height; y++)
        {
            int runLength = 1;
            for (int x = 1; x < Width; x++)
            {
                if (Tiles[x, y].Item == Tiles[x - 1, y].Item)
                    runLength++;
                else
                    runLength = 1;

                if (runLength >= 3)
                    for (int i = 0; i < runLength; i++)
                        tilesToPop.Add(Tiles[x - i, y]);
            }
        }

        // 🔹 Vertical matches
        for (int x = 0; x < Width; x++)
        {
            int runLength = 1;
            for (int y = 1; y < Height; y++)
            {
                if (Tiles[x, y].Item == Tiles[x, y - 1].Item)
                    runLength++;
                else
                    runLength = 1;

                if (runLength >= 3)
                    for (int i = 0; i < runLength; i++)
                        tilesToPop.Add(Tiles[x, y - i]);
            }
        }

        if (tilesToPop.Count == 0) break;
        popped = true;

        // 🔹 Shrink animation
        var deflate = DOTween.Sequence();
        foreach (var tile in tilesToPop)
            deflate.Join(tile.icon.transform.DOScale(Vector3.zero, TweenDuration));

        audioSource.PlayOneShot(collectSound);
        ScoreCounter.Instance.Score += tilesToPop.Sum(t => t.Item.value);
        await deflate.Play().AsyncWaitForCompletion();

        // 🔹 Refill new icons
        var inflate = DOTween.Sequence();
        foreach (var tile in tilesToPop)
        {
            tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
            inflate.Join(tile.icon.transform.DOScale(Vector3.one, TweenDuration));
        }
        await inflate.Play().AsyncWaitForCompletion();

    } while (popped);

    // ✅ After all matches are done and refilled, check for possible moves
    if (!HasAnyMoves())
    {
        Debug.Log("No moves left after refill — reshuffling!");
        await ReshuffleBoardSmoothly();
    }
}

private async Task ReshuffleBoardSmoothly()
{
    _isBusy = true;
    Debug.Log("No moves left — reshuffling smoothly!");

    await Task.Delay(500); // short pause before reshuffle

    // simple visual bounce for player feedback
    var shrink = DOTween.Sequence();
    foreach (var tile in Tiles)
        shrink.Join(tile.icon.transform.DOScale(0.9f, 0.2f));
    await shrink.Play().AsyncWaitForCompletion();

    ShuffleBoard();

    var expand = DOTween.Sequence();
    foreach (var tile in Tiles)
        expand.Join(tile.icon.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    await expand.Play().AsyncWaitForCompletion();

    _isBusy = false;
}


    #endregion

    #region End Turn / Shuffle

    private void EndTurnCheck()
    {
        if (!HasAnyMoves())
        {
            Debug.Log("No moves left — reshuffling!");
            ShuffleBoard();
        }
    }

    private void ShuffleBoard()
    {
        List<Item> items = new List<Item>();

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                items.Add(Tiles[x, y].Item);

        // Fisher–Yates shuffle
        for (int i = 0; i < items.Count; i++)
        {
            int rand = Random.Range(i, items.Count);
            (items[i], items[rand]) = (items[rand], items[i]);
        }

        int index = 0;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Tiles[x, y].Item = items[index++];

        if (!HasAnyMoves())
            ShuffleBoard();
    }

    #endregion
}

