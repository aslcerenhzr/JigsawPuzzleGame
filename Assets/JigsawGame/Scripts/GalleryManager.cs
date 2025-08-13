using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GridLayoutGroup photoGrid;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        ConfigureGridLayout();
    }

    private void ConfigureGridLayout()
    {
        float screenWidth = photoGrid.GetComponent<RectTransform>().rect.width;
        float cellSize = screenWidth / 2.5f;
        float spaceLeft = screenWidth - (cellSize * 2f);

        float padding = spaceLeft / 3f;
        
        photoGrid.padding.left = (int)padding;
        photoGrid.padding.right = (int)padding;

        photoGrid.cellSize = Vector2.one * cellSize;
        photoGrid.spacing = Vector2.one * padding;
    }

}
