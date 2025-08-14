using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PhotoCropUI photoCropUI;
    [SerializeField] private GameObject cropPanel;
    [SerializeField] private Button takePictureButton;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject gameUiRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (photoCropUI != null && photoCropUI.cancelButton != null)
            photoCropUI.cancelButton.onClick.AddListener(HandleCropCanceled);
    }

    public void BeginCrop(Texture2D texture)
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);
        if (gameUiRoot != null)
            gameUiRoot.SetActive(false);
        if (cropPanel != null)
            cropPanel.SetActive(true);
        if (photoCropUI != null)
        {
            photoCropUI.gameObject.SetActive(true);
            photoCropUI.OnPhotoReadyForCrop(texture);
        }
        if (takePictureButton != null)
            takePictureButton.interactable = false;
    }

    public void FinishCrop(Texture2D croppedTexture)
    {
        if (photoCropUI != null)
            photoCropUI.OnPhotoTaken(croppedTexture);

        if (cropPanel != null)
            cropPanel.SetActive(false);
        if (photoCropUI != null)
            photoCropUI.gameObject.SetActive(false);
        if (takePictureButton != null)
            takePictureButton.interactable = true;
        if (menuRoot != null)
            menuRoot.SetActive(false);
        if (gameUiRoot != null)
            gameUiRoot.SetActive(true);

        // Launch puzzle with the newly cropped texture
        StartPuzzleFromTexture(croppedTexture);
    }

    private void HandleCropCanceled()
    {
        if (takePictureButton != null)
            takePictureButton.interactable = true;
        if (menuRoot != null)
            menuRoot.SetActive(true);
        if (gameUiRoot != null)
            gameUiRoot.SetActive(false);
    }

    public void OnGameBackButtonClicked()
    {
        var generator = FindObjectOfType<PuzzleGenerator>();
        if (generator != null)
            generator.ClearPuzzle();

        if (cropPanel != null)
            cropPanel.SetActive(false);
        if (photoCropUI != null)
            photoCropUI.gameObject.SetActive(false);
        if (gameUiRoot != null)
            gameUiRoot.SetActive(false);
        if (menuRoot != null)
            menuRoot.SetActive(true);
        if (takePictureButton != null)
            takePictureButton.interactable = true;
    }

    public void StartPuzzleFromTexture(Texture2D texture)
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);
        if (gameUiRoot != null)
            gameUiRoot.SetActive(true);

        var generator = FindObjectOfType<PuzzleGenerator>();
        if (generator != null && texture != null)
        {
            generator.SetTextureAndGenerate(texture);
        }
    }

    public bool IsMenuOpen
    {
        get { return menuRoot != null && menuRoot.activeInHierarchy; }
    }
}
