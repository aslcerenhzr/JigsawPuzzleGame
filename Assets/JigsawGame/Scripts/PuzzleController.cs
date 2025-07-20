using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;

public class PuzzleController : MonoBehaviour
{
    [Header(" Elements ")]
    private PuzzleGenerator puzzleGenerator;
    [Header(" Settings ")]
    private float detectionRadius;
    [Header(" Piece Movement ")]
    private Vector3 startPos;
    private PuzzlePiece currentPiece;
    [Header(" Rotation ")]
    [SerializeField] private float rotationSpeed;
    private Quaternion pieceStartRotation;

    public void Configure(PuzzleGenerator puzzleGenerator, float gridScale)
    {
        this.puzzleGenerator = puzzleGenerator;
        detectionRadius = gridScale / 2 * 1.5f;
    }

    public bool SingleTouchBeganCallback(Vector3 worldPosition)
    {
        PuzzlePiece[] puzzlePieces = puzzleGenerator.GetPuzzlePieces();
        currentPiece = GetTopClosestPiece(puzzlePieces, worldPosition);

        Debug.Log(worldPosition);

        if (currentPiece == null)
            return false;

        ManagePiecesOrder(puzzlePieces);

        startPos = worldPosition;
        currentPiece.StartMoving();

        return true;
    }

    private void ManagePiecesOrder(PuzzlePiece[] puzzlePieces)
    {
        float highestZ = puzzlePieces.Length * Constants.pieceZOffset;
        float currentPieceZ = currentPiece.transform.position.z;

        Vector3 currentPieceTargetPos = currentPiece.transform.position;
        currentPieceTargetPos.z = -highestZ;
        currentPiece.transform.position = currentPieceTargetPos;

        for (int i = 0; i < puzzlePieces.Length; i++)
        {
            if(puzzlePieces[i] == currentPiece) 
                continue;

            if(puzzlePieces[i].transform.position.z < currentPieceZ)
            {
                puzzlePieces[i].transform.position += Vector3.forward * Constants.pieceZOffset;
            }
        }

        if (currentPiece.Group == null)
            return;
        foreach (Transform piece in currentPiece.Group)
        {
            Vector3 newPos = piece.position;
            newPos.z = -highestZ;
            piece.position = newPos;

        }

    }

    public void SingleTouchDrag(Vector3 worldPosition)
    {
        Vector3 moveDelta = worldPosition - startPos;

        if(currentPiece != null)
            currentPiece.Move(moveDelta);
    }

    private PuzzlePiece GetTopClosestPiece(PuzzlePiece[] puzzlePieces, Vector3 worldPos)
    {
        List<PuzzlePiece> potentialPieces = new List<PuzzlePiece>();

        for (int i = 0;i < puzzlePieces.Length;i++)
        {
            if (puzzlePieces[i].IsValid)
                continue;

            float distance = Vector3.Distance((Vector2)puzzlePieces[i].transform.position, worldPos);

            if(distance > detectionRadius)
                continue;

            potentialPieces.Add(puzzlePieces[i]);
        }

        if (potentialPieces.Count <= 0)
            return null;

        potentialPieces.Sort();

        return potentialPieces[0];
    }

    public void SingleTouchEnded()
    {
        if (currentPiece == null)
            return;
        currentPiece.StopMoving();
        currentPiece = null;
    }

    public void StartRotatingPiece()
    {
        if (currentPiece == null)
            return;

        if (currentPiece.Group == null)
            pieceStartRotation = currentPiece.transform.rotation;
        else
            pieceStartRotation = currentPiece.Group.rotation;

    }

    public void RotatePiece(float xDelta)
    {
        if(currentPiece == null)
            return;
        float targetAdditionalZAngle = xDelta * rotationSpeed;
        Quaternion targetRotation = pieceStartRotation * Quaternion.Euler(0, 0, targetAdditionalZAngle);

        if(currentPiece.Group == null)
            currentPiece.transform.rotation = targetRotation;
        else
            currentPiece.Group.rotation = targetRotation;
    }


}

