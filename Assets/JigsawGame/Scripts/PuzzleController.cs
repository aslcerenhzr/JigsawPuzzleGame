using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PuzzleController : MonoBehaviour
{
    [Header(" Elements ")]
    private PuzzleGenerator puzzleGenerator;
    [Header(" Settings ")]
    private float detectionRadius;
    [Header(" Piece Movement ")]
    private Vector3 startPos;
    private PuzzlePiece currentPiece;


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


    private PuzzlePiece GetClosestPiece(PuzzlePiece[] puzzlePieces, Vector3 worldPosition)
    {
        float minDistance = 50000;
        int closestIndex = -1;

        for (int i = 0; i < puzzlePieces.Length; i++)
        {
            float distance = Vector3.Distance((Vector2)puzzlePieces[i].transform.position, worldPosition);

            if (distance > detectionRadius)
                continue;

            if(distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        if(closestIndex < 0)
            return null;

        return puzzlePieces[closestIndex];
    }
}

