using UnityEngine;
using System;
using UnityEngine.UI;

public class PhotoManager : MonoBehaviour
{
    [Header("Manual Crop Settings")]
    public Rect manualCropRect = new Rect(0.1f, 0.1f, 0.8f, 0.8f); // Relative to image size
    
    [Header(" Events ")]
    public static Action<Texture2D> onPhotoTaken;

    [Header(" Elements ")]
    [SerializeField] private PhotoCropUI photoCropUI;
    [SerializeField] private Button takePictureButton;


    public void TakePictureButtonCallback()
    {
        NativeCamera.TakePicture((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeCamera.LoadImageAtPath(path, 1024, false);

                if (texture == null)
                    return;

                // UI: cropping aşamasında container aktif olsun, buton inaktif olsun
                if (photoCropUI != null)
                    photoCropUI.gameObject.SetActive(true);
                if (takePictureButton != null)
                    takePictureButton.interactable = false;

                // Send original texture for manual cropping
                photoCropUI.OnPhotoReadyForCrop(texture);
            }
        });
    }

    // Manual crop method - called from UI
    public void ApplyManualCrop(Texture2D originalTexture, Rect cropRect)
    {
        Texture2D croppedTexture = CropTexture(originalTexture, cropRect);
        photoCropUI.OnPhotoTaken(croppedTexture);
        onPhotoTaken?.Invoke(croppedTexture);

        // Cropping bitti: butonu tekrar aktif et
        if (takePictureButton != null)
            takePictureButton.interactable = true;
    }

    // Apply manual crop with relative coordinates
    public void ApplyManualCropRelative(Texture2D originalTexture, Rect relativeCropRect)
    {
        Rect absoluteCropRect = new Rect(
            relativeCropRect.x * originalTexture.width,
            relativeCropRect.y * originalTexture.height,
            relativeCropRect.width * originalTexture.width,
            relativeCropRect.height * originalTexture.height
        );
        
        ApplyManualCrop(originalTexture, absoluteCropRect);
    }

    private Texture2D CropTexture(Texture2D texture, Rect cropRect)
    {
        int x = Mathf.RoundToInt(cropRect.x);
        int y = Mathf.RoundToInt(cropRect.y);
        int width = Mathf.RoundToInt(cropRect.width);
        int height = Mathf.RoundToInt(cropRect.height);

        // Clamp values to texture bounds
        x = Mathf.Clamp(x, 0, texture.width - 1);
        y = Mathf.Clamp(y, 0, texture.height - 1);
        width = Mathf.Clamp(width, 1, texture.width - x);
        height = Mathf.Clamp(height, 1, texture.height - y);

        Color[] pixels = texture.GetPixels(x, y, width, height);

        Texture2D croppedTexture = new Texture2D(width, height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        return croppedTexture;
    }
}
