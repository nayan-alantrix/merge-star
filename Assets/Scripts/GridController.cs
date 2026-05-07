using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private CellController cellPrefab;
    [SerializeField] private TileDataSO tileDataSO;
[SerializeField] private Tile tilePrefab; // assign your Tile prefab in Inspector
    private int colSize = 5;
    private CellController[,] grid;

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

    public bool TryPlaceBlock(Block block, CellController dropCell)
{
    int row = dropCell.Row;
    int col = dropCell.Col;

    if (!CanPlaceBlock(block, row, col))
    {
        ClearAllHighlights();
        return false;
    }

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
        placedCells.Add(targetCell);
    }

    Destroy(block.gameObject);
    ClearAllHighlights();

    foreach (var cell in placedCells)
        CheckAndMerge(cell);

    BlockSpawnManager.Instance.SpawnBlock();
    return true;
}
    // -------------------------------------------------------
    // Placement Validation
    // -------------------------------------------------------

    private bool CanPlaceBlock(Block block, int row, int col)
    {
        for (int i = 0; i < block.TileCount; i++)
        {
            int targetRow = block.IsVertical ? row + i : row;
            int targetCol = block.IsVertical ? col : col + i;

            if (targetRow >= colSize || targetCol >= colSize)
                return false;

            if (grid[targetRow, targetCol].IsOccupied)
                return false;
        }

        return true;
    }

    // -------------------------------------------------------
    // Merge Logic
    // -------------------------------------------------------

    private static readonly (int dr, int dc)[] Neighbours =
    {
        ( 0,  1),
        ( 0, -1),
        ( 1,  0),
        (-1,  0),
    };

    private void CheckAndMerge(CellController originCell)
{
    if (!originCell.IsOccupied) return;

    TileType targetType = originCell.occupyingTile.TileType;

    // Flood fill
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

    if (group.Count < 3) return;

    // --- Boom Block: clear 3x3 around every cell in group ---
    if (targetType == TileType.Boom_Block)
    {
        // First pop all bomb tiles in the group
        foreach (CellController cell in group)
        {
            Destroy(cell.occupyingTile.gameObject);
            cell.ClearTile();
        }

        // Then clear 3x3 around the last placed bomb (originCell)
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                CellController c = GetCell(originCell.Row + dr, originCell.Col + dc);
                if (c != null && c.IsOccupied)
                {
                    Destroy(c.occupyingTile.gameObject);
                    c.ClearTile();
                }
            }
        }

        return;
    }

    // --- Normal merge: pop group, spawn upgraded tile at origin ---
    if (!tileDataSO.TryGetNextTier(targetType, out TileData nextData))
    {
        // Max tier — just pop
        foreach (CellController cell in group)
        {
            Destroy(cell.occupyingTile.gameObject);
            cell.ClearTile();
        }
        return;
    }

    foreach (CellController cell in group)
    {
        Destroy(cell.occupyingTile.gameObject);
        cell.ClearTile();
    }

    // Spawn upgraded tile at the origin (last placed) cell
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

    // Chain reaction
    CheckAndMerge(originCell);
}

private CellController GetCenterCell(List<CellController> group)
{
    // Average row/col of the group, pick the cell closest to that average
    float avgRow = 0, avgCol = 0;

    foreach (var cell in group)
    {
        avgRow += cell.Row;
        avgCol += cell.Col;
    }

    avgRow /= group.Count;
    avgCol /= group.Count;

    CellController closest = group[0];
    float bestDist = float.MaxValue;

    foreach (var cell in group)
    {
        float dist = Mathf.Abs(cell.Row - avgRow) + Mathf.Abs(cell.Col - avgCol);
        if (dist < bestDist)
        {
            bestDist = dist;
            closest  = cell;
        }
    }

    return closest;
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

    public CellController GetCell(int row, int col)
    {
        if (row < 0 || row >= colSize || col < 0 || col >= colSize) return null;

        return grid[row, col];
    }
}