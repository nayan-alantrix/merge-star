using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Block : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private Tile tilePrefab;

    private List<Tile> tiles = new();

    public int TileCount => tiles.Count;
    public List<Tile> Tiles => tiles;
    public bool IsVertical => rotation == 1 || rotation == 3;

    // 0 = horizontal AB, 1 = vertical BA (top=B bottom=A)
    // 2 = horizontal BA, 3 = vertical AB (top=A bottom=B)
    private int rotation = 0;

    public void Initialize(List<TileData> tileDataList)
    {
        foreach (var data in tileDataList)
        {
            Tile t = Instantiate(tilePrefab, horizontalLayoutGroup.transform);
            t.SetData(data);
            tiles.Add(t);
        }

        verticalLayoutGroup.gameObject.SetActive(false);
        horizontalLayoutGroup.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        if (tiles.Count < 2) return;

        rotation = (rotation + 1) % 4;
        ApplyRotation();

        StopAllCoroutines();
        StartCoroutine(PunchScale());
    }

    private void ApplyRotation()
    {
        switch (rotation)
        {
            case 0: // Horizontal: A B
                SetLayout(horizontal: true);
                SetTileOrder(0, 1);
                break;

            case 1: // Vertical: B (top) A (bottom)
                SetLayout(horizontal: false);
                SetTileOrder(1, 0);
                break;

            case 2: // Horizontal: B A
                SetLayout(horizontal: true);
                SetTileOrder(1, 0);
                break;

            case 3: // Vertical: A (top) B (bottom)
                SetLayout(horizontal: false);
                SetTileOrder(0, 1);
                break;
        }
    }

    private void SetLayout(bool horizontal)
    {
        horizontalLayoutGroup.gameObject.SetActive(horizontal);
        verticalLayoutGroup.gameObject.SetActive(!horizontal);

        Transform activeParent = horizontal
            ? horizontalLayoutGroup.transform
            : verticalLayoutGroup.transform;

        foreach (Tile t in tiles)
            t.transform.SetParent(activeParent, false);
    }

    // Pass tile indices in the order they should appear top-to-bottom / left-to-right
    private void SetTileOrder(int first, int second)
    {
        tiles[first].transform.SetSiblingIndex(0);
        tiles[second].transform.SetSiblingIndex(1);
    }

    private System.Collections.IEnumerator PunchScale()
    {
        float duration = 0.12f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.2f;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    // Returns tiles in their current visual order (top-to-bottom or left-to-right)
    public List<Tile> GetOrderedTiles()
    {
        Transform parent = IsVertical
            ? verticalLayoutGroup.transform
            : horizontalLayoutGroup.transform;

        List<Tile> ordered = new();
        for (int i = 0; i < parent.childCount; i++)
            ordered.Add(parent.GetChild(i).GetComponent<Tile>());

        return ordered;
    }
}