using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Transform originalParent;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
        canvas        = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent   = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform, true);
        canvasGroup.alpha          = 0.85f;
        canvasGroup.blocksRaycasts = false;

        // Show valid drop zones
        GridController.Instance.HighlightValidCells(GetComponent<Block>());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;

        // If block wasn't consumed by a successful drop, return it
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalPosition;
        }

        // Always clear highlights when drag ends
        GridController.Instance.ClearAllHighlights();
    }
}