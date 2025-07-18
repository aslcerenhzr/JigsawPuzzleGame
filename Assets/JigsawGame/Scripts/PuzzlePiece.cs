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
    public bool IsValid { get; private set; }

    public void Configure(float scale, Vector2 tiling, Vector2 offset, Vector3 correctPos)
    {
        transform.localScale = Vector3.one * scale;

        renderer.material.mainTextureScale = tiling;
        renderer.material.mainTextureOffset = offset;

        this.correctPosition = correctPos;
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
        CheckForValidation();
    }

    private void CheckForValidation()
    {
        if (IsCloseToCorrectPos())
            Validate();
    }

    private bool IsCloseToCorrectPos()
    {
        return Vector3.Distance((Vector2)transform.position, (Vector2)correctPosition) < GetMinValidDistance();
    }

    private float GetMinValidDistance()
    {
        return MathF.Max(.05f, transform.localScale.x / 5);
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
