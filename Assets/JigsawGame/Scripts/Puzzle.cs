using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private PuzzleGenerator puzzleGenerator;

    private void Awake()
    {
        PuzzlePiece.onValidated += CheckIfPuzzleComplete;

    }

    private void OnDestroy()
    {
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
