using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

using Dreamteck.Splines;

public class PuzzlePieceGenerator : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Renderer renderer;
    [SerializeField] private SplineComputer knobSpline;
    [SerializeField] private SplineComputer holeSpline;

    List<Vector3> vertices = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 topRight = (Vector3.right + Vector3.up) * 0.5f;
        Vector3 bottomRight = topRight + Vector3.down;
        Vector3 bottomLeft = bottomRight + Vector3.left;
        Vector3 topLeft = bottomLeft + Vector3.up;

        Vector3 topMid = (topRight + topLeft) / 2;
        Vector3 rightMid = (topRight + bottomRight) / 2;
        Vector3 bottomMid = (bottomLeft + bottomRight) / 2;
        Vector3 leftMid = (bottomLeft + topLeft) / 2;

        List<Vector2> v2Vertices = new List<Vector2>();

        v2Vertices.Add(topRight);

        ManageRightEdge(rightMid, v2Vertices);

        v2Vertices.Add(bottomRight);

        ManageBottomEdge(bottomMid, v2Vertices);

        v2Vertices.Add(bottomLeft);

        ManageLeftEdge(leftMid, v2Vertices);

        v2Vertices.Add(topLeft);

        ManageTopEdge(topMid, v2Vertices);

        MeshTriangulator triangulator = new MeshTriangulator(v2Vertices.ToArray());
        int[] triangles = triangulator.Triangulate();

        for(int i = 0; i<v2Vertices.Count; i++)
            vertices.Add(v2Vertices[i]);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        renderer.GetComponent<MeshFilter>().mesh = mesh;

    }

    private void ManageTopEdge(Vector3 topMid, List<Vector2> vertices)
    {
        SampleCollection sampleCollection = new SampleCollection();
        knobSpline.GetSamples(sampleCollection);

        for(int i = 0;i<sampleCollection.length; i++)
        {
            Vector3 sample = sampleCollection.samples[i].position;
            Vector3 knobVertex = sample * .3f + topMid;

            vertices.Add(knobVertex);
        }
    }

    private void ManageRightEdge(Vector3 rightMid, List<Vector2> vertices)
    {
        SampleCollection sampleCollection = new SampleCollection();
        holeSpline.GetSamples(sampleCollection);

        for (int i = 0; i < sampleCollection.length; i++)
        {
            Vector3 sample = sampleCollection.samples[i].position;
            Vector3 rotatedSample = Quaternion.Euler(0,0,-90) * sample;   
            Vector3 knobVertex = rotatedSample * .3f + rightMid;

            vertices.Add(knobVertex);
        }
    }

    private void ManageBottomEdge(Vector3 bottomMid, List<Vector2> vertices)
    {
        SampleCollection sampleCollection = new SampleCollection();
        knobSpline.GetSamples(sampleCollection);

        for (int i = 0; i < sampleCollection.length; i++)
        {
            Vector3 sample = sampleCollection.samples[i].position;
            Vector3 rotatedSample = Quaternion.Euler(0, 0, -180) * sample;
            Vector3 knobVertex = rotatedSample * .3f + bottomMid;

            vertices.Add(knobVertex);
        }
    }

    private void ManageLeftEdge(Vector3 leftMid, List<Vector2> vertices)
    {
        SampleCollection sampleCollection = new SampleCollection();
        knobSpline.GetSamples(sampleCollection);

        for (int i = 0; i < sampleCollection.length; i++)
        {
            Vector3 sample = sampleCollection.samples[i].position;
            Vector3 rotatedSample = Quaternion.Euler(0, 0, -270) * sample;
            Vector3 knobVertex = rotatedSample * .3f + leftMid;

            vertices.Add(knobVertex);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Count; i++) {
            Gizmos.DrawWireSphere(vertices[i], .1f);
        }
    }
}
