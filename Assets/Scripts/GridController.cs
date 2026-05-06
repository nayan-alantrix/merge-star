using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private CellController cellPrefab;
    [SerializeField] private TileDataSO tileDataSO;

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

            CellController c =
                Instantiate(cellPrefab, gridLayoutGroup.transform);

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

        List<CellController> placedCells = new();

        for (int i = 0; i < block.TileCount; i++)
        {
            Tile tile = block.Tiles[i];

            CellController targetCell = grid[row, col + i];

            tile.transform.SetParent(null, false);

            targetCell.AcceptTile(tile);

            placedCells.Add(targetCell);
        }

        Destroy(block.gameObject);

        ClearAllHighlights();

        foreach (var cell in placedCells)
        {
            CheckAndMerge(cell);
        }

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
            int targetCol = col + i;

            if (targetCol >= colSize)
                return false;

            if (grid[row, targetCol].IsOccupied)
                return false;
        }

        return true;
    }

    // -------------------------------------------------------
    // Merge Logic
    // -------------------------------------------------------

    private static readonly (int dr, int dc)[] Neighbours =
    {
        (0,1),
        (0,-1),
        (1,0),
        (-1,0)
    };

    private void CheckAndMerge(CellController originCell)
{
    if (tileDataSO == null)
    {
        Debug.LogError("TileDataSO is missing in GridController!");
        return;
    }

    if (originCell == null)
    {
        Debug.LogError("Origin Cell is NULL");
        return;
    }

    if (!originCell.IsOccupied)
    {
        Debug.Log("Cell not occupied");
        return;
    }

    Tile originTile = originCell.occupyingTile;

    if (originTile == null)
    {
        Debug.LogError("Origin Tile is NULL");
        return;
    }

    if (originTile.Data == null)
    {
        Debug.LogError("Origin Tile Data is NULL");
        return;
    }

    TileType currentType = originTile.TileType;

    // Boom block doesn't merge
    if (currentType == TileType.Boom_Block)
        return;

    // Try getting next tier
    if (!tileDataSO.TryGetNextTier(currentType, out TileData nextData))
    {
        Debug.Log("No next tier exists");
        return;
    }

    if (nextData == null)
    {
        Debug.LogError("NextData is NULL");
        return;
    }

    foreach (var (dr, dc) in Neighbours)
    {
        int nr = originCell.Row + dr;
        int nc = originCell.Col + dc;

        CellController neighbourCell = GetCell(nr, nc);

        if (neighbourCell == null)
            continue;

        if (!neighbourCell.IsOccupied)
            continue;

        Tile neighbourTile = neighbourCell.occupyingTile;

        if (neighbourTile == null)
            continue;

        if (neighbourTile.TileType != currentType)
            continue;

        // MERGE

        originTile.Upgrade(nextData);

        neighbourCell.ClearTile();

        Destroy(neighbourTile.gameObject);

        CheckAndMerge(originCell);

        return;
    }
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
        {
            for (int c = 0; c < colSize; c++)
            {
                grid[r, c].SetNormal();
            }
        }
    }

    public void OnHoverCell(CellController hoverCell, Block block)
    {
        ClearAllHighlights();

        int row = hoverCell.Row;
        int col = hoverCell.Col;

        bool allValid = CanPlaceBlock(block, row, col);

        for (int i = 0; i < block.TileCount; i++)
        {
            int targetCol = col + i;

            if (targetCol < 0 || targetCol >= colSize)
                continue;

            if (allValid)
                grid[row, targetCol].SetHover();
            else
                grid[row, targetCol].SetInvalid();
        }
    }

    public void OnExitCell(CellController exitCell, Block block)
    {
        ClearAllHighlights();
    }

    public CellController GetCell(int row, int col)
    {
        if (row < 0 || row >= colSize ||
            col < 0 || col >= colSize)
            return null;

        return grid[row, col];
    }
}