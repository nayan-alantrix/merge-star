using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private Tile tilePrefab;

    private List<Tile> tiles = new();

    public int TileCount => tiles.Count;
    public List<Tile> Tiles => tiles;

    public void Initialize(List<TileData> tileDataList)
    {
        foreach (var data in tileDataList)
        {
            Tile t = Instantiate(tilePrefab, layoutGroup.transform);
            t.SetData(data);
            tiles.Add(t);
        }
    }
}