using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private PuzzleController controller;
    [SerializeField] private PuzzlePiece puzzlePiecePrefab;

    [Header(" Settings ")]
    [SerializeField] private int gridSize;
    private float gridScale;
    private List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        gridScale = Constants.puzzleWorldSize / gridSize;

        controller.Configure(this, gridScale);

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        puzzlePieces.Clear();

        Vector3 startPos = Vector2.left * (gridSize * gridScale) / 2 + Vector2.down * (gridSize * gridScale) / 2;
        
        startPos.x += gridScale / 2;
        startPos.y += gridScale / 2;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector3 correctPos = startPos + new Vector3(i,j) * gridScale;
                correctPos.z -= Constants.pieceZOffset * GridIndexFromPos(i, j);

                Vector3 randomPos = Random.onUnitSphere * 2;
                randomPos.z = correctPos.z;

                PuzzlePiece puzzlePieceInstance = Instantiate(puzzlePiecePrefab, randomPos, Quaternion.identity, transform);

                puzzlePieces.Add(puzzlePieceInstance);

                Vector2 tiling = new Vector2(1f / gridSize, 1f / gridSize);
                Vector2 offset = new Vector2((float)i /gridSize, (float)j /gridSize);

                puzzlePieceInstance.Configure(gridScale, tiling, offset, correctPos);
            }
        }
    }

    private int GridIndexFromPos(int i, int j) => j + gridSize * i;

    public PuzzlePiece[] GetPuzzlePieces()
    {
        return puzzlePieces.ToArray();
    }
}
