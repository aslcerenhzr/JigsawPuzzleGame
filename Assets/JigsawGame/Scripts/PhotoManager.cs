using UnityEngine;
using System;

public class PhotoManager : MonoBehaviour
{
    [Header(" Action ")]
    public static Action<Texture2D> onPhotoTaken;


    public void TakePictureButtonCallback()
    {
        NativeCamera.TakePicture((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeCamera.LoadImageAtPath(path, 1024, false);

                if (texture == null)
                    return;

                texture = SquareTexture(texture);
                onPhotoTaken?.Invoke(texture);
            }

        });
    }


    private Texture2D SquareTexture(Texture2D texture)
    {
        if(texture.width==texture.height)
            return texture;
        
        int minSize = texture.width > texture.height ? texture.height: texture.width;

        Color[] pixels = texture.GetPixels(0,0,minSize,minSize);

        Texture2D croppedTexture = new Texture2D(minSize, minSize);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        return croppedTexture;
    }
}
