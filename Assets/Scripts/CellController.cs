using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellController : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    public bool IsOccupied => occupyingTile != null;

    private Tile occupyingTile;
    private Image cellImage;

    // Colors
    private static readonly Color normalColor    = new Color(0.8f, 0.8f, 0.8f, 1f);
    private static readonly Color validColor     = new Color(0.4f, 0.9f, 0.4f, 0.5f); // soft green
    private static readonly Color hoverColor     = new Color(0.2f, 0.85f, 0.2f, 0.8f); // bright green
    private static readonly Color invalidColor   = new Color(0.9f, 0.3f, 0.3f, 0.5f); // red — occupied

    void Awake()
    {
        cellImage = GetComponent<Image>();
    }

    public void SetPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public void AcceptTile(Tile tile)
    {
        occupyingTile = tile;
        tile.transform.SetParent(transform, false);

        RectTransform tileRT = tile.GetComponent<RectTransform>();
        RectTransform cellRT = GetComponent<RectTransform>();

        tileRT.anchorMin = new Vector2(0.5f, 0.5f);
        tileRT.anchorMax = new Vector2(0.5f, 0.5f);
        tileRT.pivot     = new Vector2(0.5f, 0.5f);
        tileRT.anchoredPosition = Vector2.zero;
        tileRT.sizeDelta = cellRT.sizeDelta;

        SetNormal();
    }

    public void ClearTile()
    {
        occupyingTile = null;
    }

    // --- Highlight API (called by GridController) ---

    public void SetNormal()  => cellImage.color = normalColor;
    public void SetValid()   => cellImage.color = validColor;
    public void SetInvalid() => cellImage.color = invalidColor;
    public void SetHover()   => cellImage.color = hoverColor;

    // --- Pointer events (hover while dragging) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only react if a block is being dragged
        if (eventData.pointerDrag == null) return;
        Block block = eventData.pointerDrag.GetComponent<Block>();
        if (block == null) return;

        GridController.Instance.OnHoverCell(this, block);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        Block block = eventData.pointerDrag.GetComponent<Block>();
        if (block == null) return;

        GridController.Instance.OnExitCell(this, block);
    }

    // --- IDropHandler ---

    public void OnDrop(PointerEventData eventData)
    {
        Block block = eventData.pointerDrag?.GetComponent<Block>();
        if (block == null) return;

        GridController.Instance.TryPlaceBlock(block, this);
    }
}