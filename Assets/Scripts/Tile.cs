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

    public void PlayPopEffect(System.Action onComplete = null)
    {
        // Scale up then destroy
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;

        LeanTween.sequence()
            .append(LeanTween.scale(gameObject, Vector3.one * 1.3f, 0.1f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.scale(gameObject, Vector3.zero, 0.12f).setEase(LeanTweenType.easeInQuad))
            .append(() => onComplete?.Invoke());
    }

    public void PlaySpawnEffect()
    {
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.zero;

        LeanTween.scale(gameObject, Vector3.one, 0.2f)
            .setEase(LeanTweenType.easeOutBack);
    }

    public void PlayMergeReceiveEffect()
    {
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;

        LeanTween.sequence()
            .append(LeanTween.scale(gameObject, Vector3.one * 1.4f, 0.12f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeInQuad));
    }
}