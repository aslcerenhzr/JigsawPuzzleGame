using UnityEngine;
using System.Collections.Generic;

public class PuzzlePieceOutline : MonoBehaviour
{
    [SerializeField] private Renderer outlineRenderer;
    [SerializeField] private float thickness;

    public void Generate(List<Vector2> vertices)
    {
        List<Vector2> inlineVertices = Outliner2D.CreateInsideOutline(vertices.ToArray(), thickness / 2);
        List<Vector2> outlineVertices = Outliner2D.CreateInsideOutline(vertices.ToArray(), -thickness / 2);

        int maxIndex = inlineVertices.Count;

        List<Vector3> finalVertices = new List<Vector3>();

        for (int i = 0; i < inlineVertices.Count; i++)
        {
            finalVertices.Add(inlineVertices[i]);
        }

        for (int i = 0; i < outlineVertices.Count; i++)
        {
            finalVertices.Add(outlineVertices[i]);
        }

        List<int> triangles = new List<int>();

        for (int i = 0; i < maxIndex - 1; i++) 
        {
            triangles.Add(i+0);
            triangles.Add(i + maxIndex +1);
            triangles.Add(i + 1);

            triangles.Add(i + 0);
            triangles.Add(i + maxIndex);
            triangles.Add(i + 1 + maxIndex);
        }

        int j = maxIndex - 1;

        triangles.Add(finalVertices.Count-1);
        triangles.Add(j +1);
        triangles.Add(0);

        triangles.Add(finalVertices.Count - 1);
        triangles.Add(0);
        triangles.Add(j);

        Mesh mesh = new Mesh();
        mesh.vertices = finalVertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();

        outlineRenderer.GetComponent<MeshFilter>().mesh = mesh;
    }
}
