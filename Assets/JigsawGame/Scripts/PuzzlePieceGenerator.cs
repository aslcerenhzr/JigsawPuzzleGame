using Dreamteck.Splines;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

using NaughtyAttributes;

public class PuzzlePieceGenerator : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private SplineComputer knobSpline;
    [SerializeField] private SplineComputer holeSpline;



    [Header(" Settings ")]
    [OnValueChanged("Generate")] [SerializeField] [UnityEngine.Range(0, 2)] private int rightTrit;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0, 2)] private int bottomTrit;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0, 2)] private int leftTrit;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0, 2)] private int topTrit;

    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0f, 1f)] private float rightOffset;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0f, 1f)] private float bottomOffset;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0f, 1f)] private float leftOffset;
    [OnValueChanged("Generate")][SerializeField][UnityEngine.Range(0f, 1f)] private float topOffset;

    List<Vector3> vertices = new List<Vector3>();

    private void Generate()
    {
        int[] trits = new int[] { rightTrit, bottomTrit, leftTrit, topTrit };
        float[] offsets = new float[] { rightOffset, bottomOffset, leftOffset, topOffset };
        
        Configure(trits, offsets);
    }

    public void Configure(int[] trits, float[] offsets)
    {
        vertices.Clear();

        rightTrit = trits[0];
        bottomTrit = trits[1];
        leftTrit = trits[2];
        topTrit = trits[3];

        rightOffset = offsets[0];
        bottomOffset = offsets[1];
        leftOffset = offsets[2];
        topOffset = offsets[3];

        Vector3 topRight = (Vector3.right + Vector3.up) * 0.5f;
        Vector3 bottomRight = topRight + Vector3.down;
        Vector3 bottomLeft = bottomRight + Vector3.left;
        Vector3 topLeft = bottomLeft + Vector3.up;
        
        /*Vector3 rightMid = (topRight + bottomRight) / 2;
        Vector3 bottomMid = (bottomLeft + bottomRight) / 2;
        Vector3 leftMid = (bottomLeft + topLeft) / 2;
        Vector3 topMid = (topRight + topLeft) / 2; */

        Vector3 rightMid = topRight + rightOffset * (bottomRight-topRight);
        Vector3 bottomMid = bottomLeft + bottomOffset * (bottomRight-bottomLeft);
        Vector3 leftMid = topLeft + leftOffset * (bottomLeft-topLeft);
        Vector3 topMid = topRight + topOffset * (topLeft-topRight);

        List<Vector2> v2Vertices = new List<Vector2>();

        v2Vertices.Add(topRight);

        ManageEdge(rightMid, v2Vertices, -90, rightTrit);

        v2Vertices.Add(bottomRight);

        ManageEdge(bottomMid, v2Vertices, -180, bottomTrit);

        v2Vertices.Add(bottomLeft);

        ManageEdge(leftMid, v2Vertices, -270, leftTrit);

        v2Vertices.Add(topLeft);

        ManageEdge(topMid, v2Vertices, 0, topTrit);

        List<Vector2> uvs = new List<Vector2>();
        for (int i =0;i<v2Vertices.Count;i++)
        {
            uvs.Add(v2Vertices[i] + Vector2.one / 2);
        }


        MeshTriangulator triangulator = new MeshTriangulator(v2Vertices.ToArray());
        int[] triangles = triangulator.Triangulate();

        for(int i = 0; i<v2Vertices.Count; i++)
            vertices.Add(v2Vertices[i]);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();

        renderer.GetComponent<MeshFilter>().mesh = mesh;

    }

    private void ManageEdge(Vector3 midPoint,  List<Vector2> vertices, float angle, int trit)
    {
        Vector3[] edgeVertices = GetEdgeVertices(trit);

        if(edgeVertices==null)
            return;

        for (int i = 0;i < edgeVertices.Length;i++)
        {
            Vector3 edgeVertex = edgeVertices[i];
            Vector3 rotatedSample = Quaternion.Euler(0, 0, angle) * edgeVertex;
            Vector3 knobVertex = rotatedSample * Constants.assetScale + midPoint;

            vertices.Add(knobVertex);
        }
    }

    private Vector3[] GetEdgeVertices(int trit)
    {
        SampleCollection sampleCollection = new SampleCollection();
        List<Vector3> vertices = new List<Vector3>();

        switch (trit)
        {
            case 0:
                return null;

            //Knob
            case 1:
                knobSpline.GetSamples(sampleCollection);
                break;

            //Hole
            case 2:
                holeSpline.GetSamples(sampleCollection);
                break;

        }

        for (int i = 0; i < sampleCollection.length; i++)
            vertices.Add(sampleCollection.samples[i].position);
        return vertices.ToArray();
    }   
    
    public int[] GetTrits() => new int[] {rightTrit,bottomTrit, leftTrit, topTrit};
    public float[] GetOffsets() => new float[] { rightOffset, bottomOffset, leftOffset, topOffset };
}
