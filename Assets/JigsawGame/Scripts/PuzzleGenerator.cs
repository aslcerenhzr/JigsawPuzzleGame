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

                Quaternion pieceRot = Quaternion.Euler(0, 0, Random.Range(0, 360));

                PuzzlePiece puzzlePieceInstance = Instantiate(puzzlePiecePrefab, randomPos, pieceRot, transform);

                puzzlePieces.Add(puzzlePieceInstance);

                Vector2 tiling = new Vector2(1f / gridSize, 1f / gridSize);
                Vector2 offset = new Vector2((float)i /gridSize, (float)j /gridSize);

                puzzlePieceInstance.Configure(gridScale, tiling, offset, correctPos);
            }
        }

        ConfigurePieces();
    }

    private void ConfigurePieces()
    {
        for(int i = 0;i < puzzlePieces.Count;i++) 
            ConfigurePiece(puzzlePieces[i], i);
    }

    private void ConfigurePiece(PuzzlePiece piece, int index)
    {
        Vector2Int gridPos = IndexToGridPos(index);

        int x = gridPos.x;
        int y = gridPos.y;

        int[] trits = GetTrits(x, y);
        float[] offsets = GetOffsets(x,y);

        PuzzlePiece rightPiece = IsValidGridPos(x+1, y) ? transform.GetChild(GridIndexFromPos(x+1, y)).GetComponent<PuzzlePiece>() : null;
        PuzzlePiece leftPiece = IsValidGridPos(x - 1, y) ? transform.GetChild(GridIndexFromPos(x - 1, y)).GetComponent<PuzzlePiece>() : null;
        PuzzlePiece bottomPiece = IsValidGridPos(x, y-1) ? transform.GetChild(GridIndexFromPos(x, y-1)).GetComponent<PuzzlePiece>() : null;
        PuzzlePiece topPiece = IsValidGridPos(x, y + 1) ? transform.GetChild(GridIndexFromPos(x, y + 1)).GetComponent<PuzzlePiece>() : null;

        piece.ConfigureGenerator(trits, offsets);
        piece.SetNeighbors(rightPiece, leftPiece, bottomPiece, topPiece);
    }

    private int[] GetTrits(int x, int y)
    {
        int right, bottom, left , top;
        right = bottom = left = top = 0;

        if (x == 0 && y == 0)
            return new int[] { GetRandomEdge(), 0, 0, GetRandomEdge() };
        if(IsValidGridPos(x, y-1))
        {
            PuzzlePieceGenerator bottomNeighbor = transform.GetChild(GridIndexFromPos(x, y-1)).GetComponent<PuzzlePiece>().GetGenerator();
            int[] bottomNeighborTrits = bottomNeighbor.GetTrits();

            int bottomNeighborTopTrit = bottomNeighborTrits[3];

            bottom = bottomNeighborTopTrit == 1 ? 2 : 1;
        }

        if (IsValidGridPos(x-1,y))
        {
            PuzzlePieceGenerator leftNeighbor = transform.GetChild(GridIndexFromPos(x-1, y)).GetComponent<PuzzlePiece>().GetGenerator();
            int[] leftNeighborTrits = leftNeighbor.GetTrits();

            int leftNeighborRightTrit = leftNeighborTrits[0];

            left = leftNeighborRightTrit == 1 ? 2 : 1;
        }

        if(IsValidGridPos(x,y+1))
            top = GetRandomEdge();
        if(IsValidGridPos(x+1,y))
            right = GetRandomEdge();

        return new int[] { right, bottom, left, top };
    }

    private float[] GetOffsets(int x, int y)
    {
        float right, bottom, left, top;
        right = bottom = left = top = 0;

        if (x == 0 && y == 0)
            return new float[] { Constants.GetRandomOffset(), 0, 0, Constants.GetRandomOffset() };
        
        if (IsValidGridPos(x, y - 1))
        {
            PuzzlePieceGenerator bottomNeighbor = transform.GetChild(GridIndexFromPos(x, y - 1)).GetComponent<PuzzlePiece>().GetGenerator();
            float[] bottomNeighborOffsets = bottomNeighbor.GetOffsets();

            float bottomNeighborTopOffset = bottomNeighborOffsets[3];

            bottom = 1-bottomNeighborTopOffset;
        }

        if (IsValidGridPos(x - 1, y))
        {
            PuzzlePieceGenerator leftNeighbor = transform.GetChild(GridIndexFromPos(x - 1, y)).GetComponent<PuzzlePiece>().GetGenerator();
            float[] leftNeighborOffsets = leftNeighbor.GetOffsets();

            float leftNeighborRightOffset = leftNeighborOffsets[0];

            left = leftNeighborRightOffset;
        }

        if (IsValidGridPos(x, y + 1))
            top = Constants.GetRandomOffset();
        if (IsValidGridPos(x + 1, y))
            right = Constants.GetRandomOffset();

        return new float[] { right, bottom, left, top };
    }

    private bool IsValidGridPos(int x, int y) => x >= 0 && y >= 0 && x < gridSize && y < gridSize;
    private int GridIndexFromPos(int i, int j) => j + gridSize * i;
    private int GetRandomEdge() => Random.Range(1, 3);

    private Vector2Int IndexToGridPos(int index)
    {
        int x = index / gridSize;
        int y = (int)(((float)index) % gridSize);
        return new Vector2Int(x, y);
    }

    public PuzzlePiece[] GetPuzzlePieces()
    {
        return puzzlePieces.ToArray();
    }
}
