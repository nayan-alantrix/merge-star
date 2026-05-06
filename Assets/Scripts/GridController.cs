using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private CellController cellPrefab;

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

    // --- Called by BlockDragHandler on drag start ---
    public void HighlightValidCells(Block block)
    {
        // We don't know drop position yet, just mark all empty cells as valid
        // Actual multi-cell validation happens on hover
        for (int r = 0; r < colSize; r++)
            for (int c = 0; c < colSize; c++)
                grid[r, c].SetNormal();
    }

    // --- Called by BlockDragHandler on drag end (miss or cancel) ---
    public void ClearAllHighlights()
    {
        for (int r = 0; r < colSize; r++)
            for (int c = 0; c < colSize; c++)
                grid[r, c].SetNormal();
    }

    // --- Called by CellController.OnPointerEnter ---
    public void OnHoverCell(CellController hoverCell, Block block)
    {
        ClearAllHighlights();

        int row = hoverCell.Row;
        int col = hoverCell.Col;

        // --- First pass: check if the entire placement is valid ---
        bool allValid = true;
        for (int i = 0; i < block.TileCount; i++)
        {
            int targetCol = col + i;

            // Out of bounds OR occupied = entire block placement is invalid
            if (targetCol >= colSize || targetCol < 0 || grid[row, targetCol].IsOccupied)
            {
                allValid = false;
                break;
            }
        }

        // --- Second pass: color all target cells the same (all green or all red) ---
        for (int i = 0; i < block.TileCount; i++)
        {
            int targetCol = col + i;

            // Only highlight cells that actually exist in the grid
            if (targetCol < 0 || targetCol >= colSize) continue;

            if (allValid)
                grid[row, targetCol].SetHover();
            else
                grid[row, targetCol].SetInvalid();
        }
    }

    // --- Called by CellController.OnPointerExit ---
    public void OnExitCell(CellController exitCell, Block block)
    {
        ClearAllHighlights();
    }

    // --- Placement ---
    public bool TryPlaceBlock(Block block, CellController dropCell)
    {
        int row = dropCell.Row;
        int col = dropCell.Col;
        List<Tile> tiles = block.Tiles;

        // Validate
        for (int i = 0; i < tiles.Count; i++)
        {
            int targetCol = col + i;
            if (targetCol >= colSize || grid[row, targetCol].IsOccupied)
            {
                ClearAllHighlights();
                return false;
            }
        }

        // Place
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            CellController targetCell = grid[row, col + i];

            tile.transform.SetParent(null, false);
            targetCell.AcceptTile(tile);
        }

        Destroy(block.gameObject);
        ClearAllHighlights();
        BlockSpawnManager.Instance.SpawnBlock();
        return true;
    }

    public CellController GetCell(int row, int col)
    {
        if (row < 0 || row >= colSize || col < 0 || col >= colSize) return null;
        return grid[row, col];
    }
}