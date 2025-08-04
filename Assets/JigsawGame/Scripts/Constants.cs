using UnityEngine;

public static class Constants
{
    [Header(" Settings ")]
    public const float pieceZOffset = 0.001f;
    public const float puzzleWorldSize = 4f;

    [Header(" Offsets ")]
    public const float assetScale = 0.25f;
    public const float minAssetOffset = 0.4f;
    public const float maxAssetOffset = 0.6f;

    public static float GetRandomOffset() => Random.Range(minAssetOffset, maxAssetOffset);
}
