using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private CellController cellPrefab;
    [SerializeField] private TileDataSO tileDataSO;
    [SerializeField] private Tile tilePrefab;

    private int colSize = 5;
    private CellController[,] grid;

    private Queue<CellController> mergeQueue = new();
    private bool isMerging = false;

    private static readonly (int dr, int dc)[] Neighbours =
    {
        ( 0,  1),
        ( 0, -1),
        ( 1,  0),
        (-1,  0),
    };

    // -------------------------------------------------------
    // Init
    // -------------------------------------------------------

    void Awake()
    {
        Instance = this;

        colSize = gridLayoutGroup.constraintCount;
        int totalCells = colSize * colSize;
        grid = new CellController[colSize, colSize];

        for (int i = 0; i < totalCells; i++)
        {
            int row = i / colSize;
            int col = i % colSize;

            CellController c = Instantiate(cellPrefab, gridLayoutGroup.transform);
            c.SetPosition(row, col);
            c.gameObject.name = $"cell_{row}_{col}";
            grid[row, col] = c;
        }
    }

    // -------------------------------------------------------
    // Placement
    // -------------------------------------------------------

    public bool TryPlaceBlock(Block block, CellController dropCell)
    {
        int row = dropCell.Row;
        int col = dropCell.Col;

        if (!CanPlaceBlock(block, row, col))
        {
            ClearAllHighlights();
            return false;
        }

        // Reset chain multiplier for this new placement
        ScoreManager.Instance.ResetChain();

        List<Tile> orderedTiles = block.GetOrderedTiles();
        List<CellController> placedCells = new();

        for (int i = 0; i < block.TileCount; i++)
        {
            int targetRow = block.IsVertical ? row + i : row;
            int targetCol = block.IsVertical ? col : col + i;

            Tile tile = orderedTiles[i];
            CellController targetCell = grid[targetRow, targetCol];

            tile.transform.SetParent(null, false);
            targetCell.AcceptTile(tile);
            tile.PlaySpawnEffect();

            placedCells.Add(targetCell);
        }

        Destroy(block.gameObject);
        ClearAllHighlights();

        foreach (var cell in placedCells)
            mergeQueue.Enqueue(cell);

        if (!isMerging)
            StartCoroutine(ProcessMergeQueue());

        BlockSpawnManager.Instance.OnBlockPlaced(block);
        return true;
    }

    private bool CanPlaceBlock(Block block, int row, int col)
    {
        for (int i = 0; i < block.TileCount; i++)
        {
            int targetRow = block.IsVertical ? row + i : row;
            int targetCol = block.IsVertical ? col : col + i;

            if (targetRow >= colSize || targetCol >= colSize) return false;
            if (targetRow < 0 || targetCol < 0) return false;
            if (grid[targetRow, targetCol].IsOccupied) return false;
        }

        return true;
    }

    // -------------------------------------------------------
    // Merge Queue
    // -------------------------------------------------------

    private IEnumerator ProcessMergeQueue()
    {
        isMerging = true;

        while (mergeQueue.Count > 0)
        {
            CellController cell = mergeQueue.Dequeue();

            if (!cell.IsOccupied) continue;

            yield return StartCoroutine(CheckAndMergeCoroutine(cell));

            yield return new WaitForSeconds(0.15f);
        }

        isMerging = false;
    }

    // -------------------------------------------------------
    // Merge Logic
    // -------------------------------------------------------

    private IEnumerator CheckAndMergeCoroutine(CellController originCell)
    {
        if (!originCell.IsOccupied) yield break;

        TileType targetType = originCell.occupyingTile.TileType;

        // Flood fill — find all connected same-type tiles
        List<CellController> group = new();
        Queue<CellController> queue = new();
        HashSet<CellController> visited = new();

        queue.Enqueue(originCell);
        visited.Add(originCell);

        while (queue.Count > 0)
        {
            CellController current = queue.Dequeue();
            group.Add(current);

            foreach (var (dr, dc) in Neighbours)
            {
                CellController neighbour = GetCell(current.Row + dr, current.Col + dc);

                if (neighbour == null) continue;
                if (visited.Contains(neighbour)) continue;
                if (!neighbour.IsOccupied) continue;
                if (neighbour.occupyingTile.TileType != targetType) continue;

                visited.Add(neighbour);
                queue.Enqueue(neighbour);
            }
        }

        if (group.Count < 3) yield break;

        // --- Bomb merge ---
        if (targetType == TileType.Boom_Block)
        {
            yield return StartCoroutine(PopGroup(group));

            // 3x3 blast around last placed bomb (originCell)
            List<CellController> blastCells = new();

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    CellController c = GetCell(originCell.Row + dr, originCell.Col + dc);
                    if (c != null && c.IsOccupied)
                        blastCells.Add(c);
                }
            }

            yield return StartCoroutine(PopGroup(blastCells));

            ScoreManager.Instance.AddBombScore(group.Count + blastCells.Count);

            yield break;
        }

        // --- Normal merge ---

        // Max tier — just pop with no upgrade
        if (!tileDataSO.TryGetNextTier(targetType, out TileData nextData))
        {
            yield return StartCoroutine(PopGroup(group));
            ScoreManager.Instance.AddMergeScore(group.Count, targetType);
            yield break;
        }

        // Pop all tiles in group simultaneously
        yield return StartCoroutine(PopGroup(group));

        // Add score for this merge
        ScoreManager.Instance.AddMergeScore(group.Count, targetType);

        // Spawn upgraded tile at origin cell
        Tile upgradedTile = Instantiate(tilePrefab, originCell.transform);
        upgradedTile.SetData(nextData);

        RectTransform tileRT = upgradedTile.GetComponent<RectTransform>();
        RectTransform cellRT = originCell.GetComponent<RectTransform>();

        tileRT.anchorMin        = new Vector2(0.5f, 0.5f);
        tileRT.anchorMax        = new Vector2(0.5f, 0.5f);
        tileRT.pivot            = new Vector2(0.5f, 0.5f);
        tileRT.anchoredPosition = Vector2.zero;
        tileRT.sizeDelta        = cellRT.sizeDelta;

        originCell.SetOccupyingTile(upgradedTile);

        // Merge receive animation
        upgradedTile.PlayMergeReceiveEffect();

        // Wait for spawn animation to finish
        yield return new WaitForSeconds(0.25f);

        // Queue chain reaction with delay handled by ProcessMergeQueue
        mergeQueue.Enqueue(originCell);
    }

    // Plays pop animation on all tiles simultaneously, waits until all done
    private IEnumerator PopGroup(List<CellController> group)
    {
        if (group.Count == 0) yield break;

        int pending = group.Count;
        bool allDone = false;

        foreach (CellController cell in group)
        {
            if (!cell.IsOccupied)
            {
                pending--;
                if (pending == 0) allDone = true;
                continue;
            }

            Tile tile = cell.occupyingTile;
            cell.ClearTile();

            tile.PlayPopEffect(() =>
            {
                Destroy(tile.gameObject);
                pending--;
                if (pending == 0) allDone = true;
            });
        }

        yield return new WaitUntil(() => allDone);
    }

    // -------------------------------------------------------
    // Highlight Helpers
    // -------------------------------------------------------

    public void HighlightValidCells(Block block)
    {
        ClearAllHighlights();
    }

    public void ClearAllHighlights()
    {
        for (int r = 0; r < colSize; r++)
            for (int c = 0; c < colSize; c++)
                grid[r, c].SetNormal();
    }

    public void OnHoverCell(CellController hoverCell, Block block)
    {
        ClearAllHighlights();

        int row = hoverCell.Row;
        int col = hoverCell.Col;

        bool allValid = CanPlaceBlock(block, row, col);

        for (int i = 0; i < block.TileCount; i++)
        {
            int targetRow = block.IsVertical ? row + i : row;
            int targetCol = block.IsVertical ? col : col + i;

            if (targetRow < 0 || targetRow >= colSize) continue;
            if (targetCol < 0 || targetCol >= colSize) continue;

            if (allValid)
                grid[targetRow, targetCol].SetHover();
            else
                grid[targetRow, targetCol].SetInvalid();
        }
    }

    public void OnExitCell(CellController exitCell, Block block)
    {
        ClearAllHighlights();
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    public CellController GetCell(int row, int col)
    {
        if (row < 0 || row >= colSize || col < 0 || col >= colSize) return null;
        return grid[row, col];
    }

    //game over'

    // Call this after every placement
    public bool HasAnyValidPlacement(Block block)
    {
        for (int r = 0; r < colSize; r++)
        {
            for (int c = 0; c < colSize; c++)
            {
                if (CanPlaceBlock(block, r, c))
                    return true;
            }
        }
        return false;
    }
}