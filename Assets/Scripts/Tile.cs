using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;

    public TileData Data { get; private set; }

    public void SetData(TileData data)
    {
        Data = data;
        image.sprite = data.sprite;
    }
}