using UnityEngine;
using UnityEngine.UI;

public class PhotoCropUI : MonoBehaviour
{
    public PhotoManager photoManager;
    public GameObject cropPanel;
    public RawImage photoPreview;
    public RectTransform cropRect;       // Opsiyonel: görsel overlay; artık hareket etmiyor
    public Button confirmButton;
    public Button cancelButton;

    [Header("Container")]
    public RectTransform photoContainer; // RawImage'ın ebeveyni; görünür alan

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 1f;   // container dolu kalmalı, min 1
    [SerializeField] private float maxZoom = 4f;   // üst sınır
    [SerializeField] private float mouseWheelZoomSpeed = 0.15f;

    private Texture2D currentTexture;
    private AspectRatioFitter aspectFitter;

    // Pan state (panning the photo)
    private bool isDragging;
    private Vector2 dragStartLocalInContainer;
    private Vector2 imageStartAnchored;

    // Pinch state (zoom)
    private bool isPinching;
    private float pinchInitialDist;
    private float pinchInitialZoom;
    private Vector2 pinchCenterLocal;

    private float currentZoom = 1f;
    private Camera uiCamera;

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null ? canvas.worldCamera : null; // ScreenSpaceOverlay ise null olmalı
    }

    void OnEnable()
    {
        confirmButton.onClick.AddListener(ConfirmCrop);
        cancelButton.onClick.AddListener(CancelCrop);
    }

    void OnDisable()
    {
        confirmButton.onClick.RemoveListener(ConfirmCrop);
        cancelButton.onClick.RemoveListener(CancelCrop);
        isDragging = false;
        isPinching = false;
    }

    public void OnPhotoReadyForCrop(Texture2D tex)
    {
        Debug.Log("OnPhotoReadyForCrop");
        currentTexture = tex;
        photoPreview.texture = tex;

        if (photoContainer == null && photoPreview != null)
            photoContainer = photoPreview.transform.parent as RectTransform;

        // AspectRatioFitter ayarı (cover)
        if (aspectFitter == null && photoPreview != null)
        {
            aspectFitter = photoPreview.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = photoPreview.gameObject.AddComponent<AspectRatioFitter>();
            }
        }
        if (aspectFitter != null)
        {
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            aspectFitter.aspectRatio = (float)tex.width / tex.height;
        }

        // Başlangıç pozisyon ve zoom
        currentZoom = 1f;
        if (photoPreview != null)
        {
            photoPreview.rectTransform.anchoredPosition = Vector2.zero;
            photoPreview.rectTransform.localScale = Vector3.one * currentZoom;
        }

        cropPanel.SetActive(true);
    }

    public void OnPhotoTaken(Texture2D cropped)
    {
        cropPanel.SetActive(false);
        gameObject.SetActive(false);
        currentTexture = null;
        isDragging = false;
        isPinching = false;
    }

    // Public pan API for InputManager
    public void BeginCropDrag(Vector2 screenPosition)
    {
        if (photoPreview == null || photoContainer == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(photoContainer, screenPosition, uiCamera, out dragStartLocalInContainer);
        imageStartAnchored = photoPreview.rectTransform.anchoredPosition;
        isDragging = true;
    }

    public void DragCrop(Vector2 screenPosition)
    {
        if (!isDragging || photoPreview == null || photoContainer == null) return;
        Vector2 currentLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(photoContainer, screenPosition, uiCamera, out currentLocal);
        Vector2 delta = currentLocal - dragStartLocalInContainer;
        Vector2 proposed = imageStartAnchored + delta;
        photoPreview.rectTransform.anchoredPosition = ClampImageAnchored(proposed);
    }

    public void EndCropDrag()
    {
        isDragging = false;
    }

    // Public pinch API for InputManager
    public void BeginCropPinch(Vector2 screenPos0, Vector2 screenPos1)
    {
        if (photoContainer == null) return;
        pinchInitialDist = (screenPos1 - screenPos0).magnitude;
        pinchInitialZoom = currentZoom;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(photoContainer, 0.5f * (screenPos0 + screenPos1), uiCamera, out pinchCenterLocal);
        isPinching = true;
    }

    public void UpdateCropPinch(Vector2 screenPos0, Vector2 screenPos1)
    {
        if (!isPinching || photoContainer == null || currentTexture == null) return;
        float currentDist = (screenPos1 - screenPos0).magnitude;
        if (pinchInitialDist <= 0.0001f) return;
        float scaleFactor = currentDist / pinchInitialDist;
        float targetZoom = Mathf.Clamp(pinchInitialZoom * scaleFactor, Mathf.Max(1f, minZoom), Mathf.Max(minZoom, maxZoom));
        SetZoomKeepingPoint(targetZoom, pinchCenterLocal);
    }

    public void EndCropPinch()
    {
        isPinching = false;
    }

    // Mouse wheel zoom
    public void ApplyMouseWheelZoom(float scrollDelta, Vector2 screenPosition)
    {
        if (Mathf.Abs(scrollDelta) < 0.0001f || photoContainer == null || currentTexture == null) return;
        float targetZoom = Mathf.Clamp(currentZoom * (1f + scrollDelta * mouseWheelZoomSpeed), Mathf.Max(1f, minZoom), Mathf.Max(minZoom, maxZoom));
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(photoContainer, screenPosition, uiCamera, out localPoint);
        SetZoomKeepingPoint(targetZoom, localPoint);
    }

    private void SetZoomKeepingPoint(float targetZoom, Vector2 containerLocalPoint)
    {
        // Container boyutları
        Rect containerRect = photoContainer.rect;
        float containerW = containerRect.width;
        float containerH = containerRect.height;

        float texW = currentTexture.width;
        float texH = currentTexture.height;

        // Cover base scale (zoom=1)
        float baseScale = Mathf.Max(containerW / texW, containerH / texH);

        // Şu anki ve hedef görüntülenen boyutlar
        float displayedW_old = texW * baseScale * currentZoom;
        float displayedH_old = texH * baseScale * currentZoom;
        float displayedW_new = texW * baseScale * targetZoom;
        float displayedH_new = texH * baseScale * targetZoom;

        // Container normalized point (0..1)
        float x_n = (containerLocalPoint.x - containerRect.xMin) / containerW;
        float y_n = (containerLocalPoint.y - containerRect.yMin) / containerH;

        // Mevcut ofset ve görünür kesit
        Vector2 imgOffset = photoPreview.rectTransform.anchoredPosition; // center offset
        float visibleU_old = containerW / displayedW_old;
        float visibleV_old = containerH / displayedH_old;
        float uMin_old = 0.5f - 0.5f * visibleU_old - (imgOffset.x / displayedW_old);
        float vMin_old = 0.5f - 0.5f * visibleV_old - (imgOffset.y / displayedH_old);

        // O noktadaki UV
        float u_at_point = uMin_old + x_n * visibleU_old;
        float v_at_point = vMin_old + y_n * visibleV_old;

        // Yeni görünür kesit
        float visibleU_new = containerW / displayedW_new;
        float visibleV_new = containerH / displayedH_new;

        // Aynı UV, aynı ekranda kalsın: uMin_new = u_at_point - x_n * visibleU_new
        float uMin_new = u_at_point - x_n * visibleU_new;
        float vMin_new = v_at_point - y_n * visibleV_new;

        // Yeni ofset
        float newOffsetX = (0.5f - 0.5f * visibleU_new - uMin_new) * displayedW_new;
        float newOffsetY = (0.5f - 0.5f * visibleV_new - vMin_new) * displayedH_new;
        Vector2 proposedAnchored = new Vector2(newOffsetX, newOffsetY);

        // Uygula (clamp ile)
        currentZoom = targetZoom;
        photoPreview.rectTransform.localScale = Vector3.one * currentZoom;
        photoPreview.rectTransform.anchoredPosition = ClampImageAnchored(proposedAnchored);
    }

    private Vector2 ClampImageAnchored(Vector2 anchored)
    {
        if (photoContainer == null || photoPreview == null || currentTexture == null)
            return anchored;

        Rect containerRect = photoContainer.rect;
        float containerW = containerRect.width;
        float containerH = containerRect.height;

        float texW = currentTexture.width;
        float texH = currentTexture.height;

        // Cover base scale and current zoom applied
        float baseScale = Mathf.Max(containerW / texW, containerH / texH);
        float displayedW = texW * baseScale * currentZoom;
        float displayedH = texH * baseScale * currentZoom;

        float maxOffsetX = Mathf.Max(0f, (displayedW - containerW) * 0.5f);
        float maxOffsetY = Mathf.Max(0f, (displayedH - containerH) * 0.5f);

        float clampedX = Mathf.Clamp(anchored.x, -maxOffsetX, maxOffsetX);
        float clampedY = Mathf.Clamp(anchored.y, -maxOffsetY, maxOffsetY);
        return new Vector2(clampedX, clampedY);
    }

    void ConfirmCrop()
    {
        if (currentTexture == null || photoContainer == null || photoPreview == null) return;

        Rect containerRect = photoContainer.rect;
        float containerW = containerRect.width;
        float containerH = containerRect.height;

        float texW = currentTexture.width;
        float texH = currentTexture.height;

        float baseScale = Mathf.Max(containerW / texW, containerH / texH);
        float displayedW = texW * baseScale * currentZoom;
        float displayedH = texH * baseScale * currentZoom;

        // Görünen kısmın texture UV kesiti (panning + zoom)
        float visibleUFrac = containerW / displayedW;
        float visibleVFrac = containerH / displayedH;

        Vector2 imgOffset = photoPreview.rectTransform.anchoredPosition;
        float u0 = 0.5f - 0.5f * visibleUFrac - (imgOffset.x / displayedW);
        float v0 = 0.5f - 0.5f * visibleVFrac - (imgOffset.y / displayedH);
        float u1 = u0 + visibleUFrac;
        float v1 = v0 + visibleVFrac;

        u0 = Mathf.Clamp01(u0); v0 = Mathf.Clamp01(v0);
        u1 = Mathf.Clamp01(u1); v1 = Mathf.Clamp01(v1);

        int px = Mathf.RoundToInt(u0 * texW);
        int py = Mathf.RoundToInt(v0 * texH);
        int pwidth = Mathf.RoundToInt((u1 - u0) * texW);
        int pheight = Mathf.RoundToInt((v1 - v0) * texH);

        if (pwidth < 1) pwidth = 1;
        if (pheight < 1) pheight = 1;
        px = Mathf.Clamp(px, 0, currentTexture.width - pwidth);
        py = Mathf.Clamp(py, 0, currentTexture.height - pheight);

        Rect pixelRect = new Rect(px, py, pwidth, pheight);
        photoManager.ApplyManualCrop(currentTexture, pixelRect);
    }

    void CancelCrop()
    {
        cropPanel.SetActive(false);
        gameObject.SetActive(false);
        currentTexture = null;
        isDragging = false;
        isPinching = false;
    }
}
