using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    [SerializeField] private Material puzzlePieceMaterial;

    private void PhotoTakenCallback(Texture2D texture)
    {
        puzzlePieceMaterial.mainTexture = texture;
        puzzleGenerator.GeneratePuzzle();
    }

    private void Awake()
    {
        PhotoManager.onPhotoTaken += PhotoTakenCallback;
        PuzzlePiece.onValidated += CheckIfPuzzleComplete;

    }

    private void OnDestroy()
    {
        PhotoManager.onPhotoTaken -= PhotoTakenCallback;
        PuzzlePiece.onValidated -= CheckIfPuzzleComplete;
    }
   

    private void CheckIfPuzzleComplete()
    {
        PuzzlePiece[] pieces = puzzleGenerator.GetPuzzlePieces();

        for (int i = 0; i < pieces.Length; i++)
            if(!pieces[i].IsValid) return;

        PuzzleCompletedCallback();
    }

    private void PuzzleCompletedCallback()
    {
        Debug.Log("PuzzleComplete");
    }
}
