using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GridLayoutGroup photoGrid;

	[Header("Item Prefab (optional)")]
	[SerializeField] private GameObject photoItemPrefab; // Should contain a RawImage to show the texture

	[Header("Default Photos (optional)")]
	[SerializeField] private List<Texture2D> defaultPhotos = new List<Texture2D>();

	IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        ConfigureGridLayout();
		PopulateInitialPhotos();
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

	void OnEnable()
	{
		PhotoManager.onPhotoTaken += AddPhotoToGrid;
	}

	void OnDisable()
	{
		PhotoManager.onPhotoTaken -= AddPhotoToGrid;
	}

	private void PopulateInitialPhotos()
	{
		if (defaultPhotos == null || defaultPhotos.Count == 0) return;
		for (int i = 0; i < defaultPhotos.Count; i++)
		{
			if (defaultPhotos[i] == null) continue;
			AddPhotoToGrid(defaultPhotos[i]);
		}
	}

	public void AddPhotoToGrid(Texture2D texture)
	{
		if (texture == null || photoGrid == null) return;

		GameObject item;
		item = Instantiate(photoItemPrefab, photoGrid.transform);

		// Try RawImage first
		var rawImage = item.GetComponentInChildren<RawImage>(true);
		if (rawImage != null)
		{
			rawImage.texture = texture;
			AttachClick(item, texture);
		}
		else
		{
			AttachClick(item, texture);
		}
	}

	private void AttachClick(GameObject item, Texture2D texture)
	{
		var button = item.GetComponent<Button>();
		if (button == null) button = item.AddComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() =>
		{
			UIManager.Instance?.StartPuzzleFromTexture(texture);
		});
	}


}
