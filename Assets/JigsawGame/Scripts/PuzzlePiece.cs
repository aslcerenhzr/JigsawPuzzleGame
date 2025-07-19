using UnityEngine;
using System;


public class PuzzlePiece : MonoBehaviour, IComparable<PuzzlePiece>
{
    [Header(" Elements ")]
    [SerializeField] private Renderer renderer;

    [Header(" Movement ")]
    private Vector3 startMovePos;

    [Header(" Validation ")]
    private Vector3 correctPosition;

    [Header(" Neighbors ")]
    private PuzzlePiece[] neighbors;


    public bool IsValid { get; private set; }

    public void Configure(float scale, Vector2 tiling, Vector2 offset, Vector3 correctPos)
    {
        transform.localScale = Vector3.one * scale;

        renderer.material.mainTextureScale = tiling;
        renderer.material.mainTextureOffset = offset;

        this.correctPosition = correctPos;
    }

    public void SetNeighbors(params PuzzlePiece[] puzzlePieces)
    {
        neighbors = puzzlePieces;
    }

    public void StartMoving()
    {
        startMovePos = transform.position;
    }

    public void Move(Vector3 moveDelta)
    {
        Vector3 targetPos = startMovePos + moveDelta;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 60 * .3f);
    }

    public void StopMoving()
    {
        bool isValid = CheckForValidation();

        if (isValid) 
            return;
        
        CheckForNeighbors();
    }

    private bool CheckForValidation()
    {
        if (IsCloseToCorrectPos())
        {
            Validate();
            return true;
        }
        return false;
            
    }

    private bool IsCloseToCorrectPos()
    {
        return Vector3.Distance((Vector2)transform.position, (Vector2)correctPosition) < GetMinValidDistance();
    }

    private float GetMinValidDistance()
    {
        return MathF.Max(.05f, transform.localScale.x / 5);
    }

    private void CheckForNeighbors()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] == null)
                continue;
            if (neighbors[i].IsValid)
                continue;

            Vector3 correctLocalPos = Quaternion.Euler(0, 0, -90 * i) * Vector3.right * transform.localScale.x;

            Vector3 correctWorldPos = transform.position + correctLocalPos;
            correctWorldPos.z = neighbors[i].transform.position.z;

            if (Vector3.Distance(correctWorldPos, neighbors[i].transform.position) < GetMinValidDistance())
            {
                //5 durum kodlancak
            }
        }
    }

    private void Validate()
    {
        correctPosition.z = 0;
        transform.position = correctPosition;

        IsValid = true;

        Debug.Log("correct place " + name);
    }


    public int CompareTo(PuzzlePiece otherPiece)
    {
        return transform.position.z.CompareTo(otherPiece.transform.position.z);
    }
}
