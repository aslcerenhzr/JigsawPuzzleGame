using UnityEngine;
using System;

using Random = UnityEngine.Random;

public class PuzzlePiece : MonoBehaviour, IComparable<PuzzlePiece>
{
    [Header(" Elements ")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private PuzzlePieceGenerator generator;

    [Header(" Movement ")]
    private Vector3 startMovePos;

    [Header(" Validation ")]
    private Vector3 correctPosition;
    public bool IsValid { get; private set; }
    public static Action onValidated;

    [Header(" Neighbors ")]
    private PuzzlePiece[] neighbors;
    public Transform Group { get; private set; }

    public void Configure(float scale, Vector2 tiling, Vector2 offset, Vector3 correctPos)
    {
        transform.localScale = Vector3.one * scale;

        renderer.material.mainTextureScale = tiling;
        renderer.material.mainTextureOffset = offset;

        this.correctPosition = correctPos;
    }

    public void SetTexture(Texture2D texture)
    {
        if (renderer != null && texture != null)
        {
            renderer.material.mainTexture = texture;
        }
    }

    public void ConfigureGenerator(int[] trits, float[] offsets)
    {
        generator.Configure(trits, offsets);
    }
    public void SetNeighbors(params PuzzlePiece[] puzzlePieces)
    {
        neighbors = puzzlePieces;
    }

    public void StartMoving()
    {
        if (Group == null)
            startMovePos = transform.position;
        else
            startMovePos = Group.position;
    }

    public void Move(Vector3 moveDelta)
    {
        Vector3 targetPos = startMovePos + moveDelta;

        if(Group == null)
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 60 * .3f);

        else
            Group.position = Vector3.Lerp(Group.position, targetPos, Time.deltaTime * 60 * .3f);
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
        if (IsCloseToCorrectPos() && HasCloseToIdentityRotation())
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

    private bool HasCloseToIdentityRotation()
    {
        return Vector3.Angle(Vector3.right, transform.right) < 10;
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

            if (Vector3.Angle(transform.right, neighbors[i].transform.right) > 10)
                continue;

            Vector3 correctLocalPos = Quaternion.Euler(0, 0, -90 * i) * Vector3.right * transform.localScale.x;

            correctLocalPos = transform.rotation * correctLocalPos;

            Vector3 correctWorldPos = transform.position + correctLocalPos;
            correctWorldPos.z = neighbors[i].transform.position.z;

            if (Vector3.Distance(correctWorldPos, neighbors[i].transform.position) < GetMinValidDistance())
            {
                SnapNeighbor(neighbors[i], correctWorldPos, i);
            }
        }
    }

    private void SnapNeighbor(PuzzlePiece neighbor,  Vector3 correctWorldPos, int neighborIndex)
    {
        //Both pieces in same group
        if (Group != null && neighbor.Group != null && Group == neighbor.Group)
            return;

        //This piece is not in a group, neighbor is also not in a group
        else if (Group == null && neighbor.Group == null)
        {
            neighbor.transform.position = correctWorldPos;
            GameObject pieceGroup = new GameObject("Piece Group " + Random.Range(100, 200));
            pieceGroup.transform.position = transform.position;
            pieceGroup.transform.SetParent(transform.parent);
            pieceGroup.transform.rotation = transform.rotation;

            transform.SetParent(pieceGroup.transform);
            neighbor.transform.SetParent(pieceGroup.transform);

            transform.localRotation = Quaternion.identity;
            neighbor.transform.localRotation = Quaternion.identity;

            Group = pieceGroup.transform;
            neighbor.Group = pieceGroup.transform;
        }

        //This piece in a group, neighbor not in a group
        else if (Group != null && neighbor.Group == null)
        {
            neighbor.transform.position = correctWorldPos;
            neighbor.transform.rotation = transform.rotation;
            neighbor.transform.SetParent(Group);
            neighbor.Group = Group;
        }

        //This piece not in a group, neighbor in a group
        else if(Group == null && neighbor.Group != null)
        {
            Group = neighbor.Group;
            transform.SetParent(Group);

            Vector3 thisCorrectWorldPos = neighbor.transform.position +
                Quaternion.Euler(0, 0, -90 * (neighborIndex + 2)) * neighbor.transform.right * transform.localScale.x;

            transform.position = thisCorrectWorldPos;
            transform.rotation = neighbor.transform.rotation;
        }

        //Both pieces are in different groups
        else if (Group != null && neighbor.Group != null && Group != neighbor.Group)
        {
            Vector3 thisCorrectWorldPos = neighbor.transform.position +
                Quaternion.Euler(0, 0, -90 * (neighborIndex + 2)) * neighbor.transform.right * transform.localScale.x;

            Vector3 moveVector = thisCorrectWorldPos - transform.position;

            Group.position += moveVector;

            Vector3 rotationCenter = transform.position;
            float angle = Vector3.SignedAngle(transform.right, neighbor.transform.right, Vector3.forward);

            Group.RotateAround(rotationCenter, Vector3.forward, angle);

            while (Group.childCount > 0)
            {
                Transform piece = Group.GetChild(0);
                piece.SetParent(neighbor.Group);
            }

            DestroyImmediate(Group.gameObject);

            foreach(Transform piece in neighbor.Group)
                piece.GetComponent<PuzzlePiece>().Group = neighbor.Group;
        }
    }

    private void Validate(bool isRecursive = true)
    {
        correctPosition.z = 0;
        transform.position = correctPosition;
        transform.rotation = Quaternion.identity;

        IsValid = true;

        onValidated?.Invoke();

        if(!isRecursive)
            return;
        if (Group == null)
            return;
        

        foreach(Transform piece in Group)
        {
            if(piece == transform) continue;

            piece.GetComponent<PuzzlePiece>().Validate(false);
        }

        Debug.Log("correct place " + name);
    }

    public int CompareTo(PuzzlePiece otherPiece)
    {
        return transform.position.z.CompareTo(otherPiece.transform.position.z);
    }

    public PuzzlePieceGenerator GetGenerator() => generator;
    
}
