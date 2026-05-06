using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;

    public TileData Data { get; private set; }
    public TileType TileType => Data.tileType;

    public void SetData(TileData data)
    {
        Data = data;
        image.sprite = data.sprite;
    }

    public void Upgrade(TileData newData)
    {
        SetData(newData);

        // Optional: punch scale animation
        StopAllCoroutines();
        StartCoroutine(PunchScale());
    }

    private System.Collections.IEnumerator PunchScale()
    {
        float t = 0f;
        Vector3 originalScale = Vector3.one;

        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t / 0.15f * Mathf.PI) * 0.3f;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}