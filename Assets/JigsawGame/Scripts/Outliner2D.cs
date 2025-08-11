using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic; // <-- List için gerekli

static class Outliner2D
{
    public static List<Vector2> CreateInsideOutline(Vector2[] originalVertices, float thickness)
    {
        List<Vector2> insideOutline = new List<Vector2>();

        for (int i = 0; i < originalVertices.Length; i++)
        {
            int nextIndex = (i + 1) % originalVertices.Length;
            int previousIndex = i - 1;
            if (previousIndex < 0)
                previousIndex = originalVertices.Length - 1;

            Vector2 nextEdge = originalVertices[nextIndex] - originalVertices[i];
            Vector2 previousEdge = originalVertices[previousIndex] - originalVertices[i];

            Vector2 nextEdgeNormal = (Vector2)Vector3.Cross(nextEdge.normalized, Vector3.forward);
            Vector2 previousEdgeNormal = (Vector2)Vector3.Cross(Vector3.forward, previousEdge.normalized);

            Vector2 normalMean = ((nextEdgeNormal + previousEdgeNormal) / 2).normalized;

            insideOutline.Add(originalVertices[i] + normalMean * thickness);
        }

        return insideOutline;
    }
}

