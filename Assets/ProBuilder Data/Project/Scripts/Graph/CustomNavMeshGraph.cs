using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Custom Graph Formulation for Zone A (Student 1)
/// Extracts Unity's baked NavMesh data into a mathematical graph of nodes (triangle centroids) and edges (adjacency).
/// Works with whichever NavMesh is currently loaded (does not require Zone B to be present).
/// </summary>
public class CustomNavMeshGraph : MonoBehaviour
{
    [System.Serializable]
    public class NavMeshNode
    {
        public int index;
        public Vector3 position;
        public List<int> neighbors;

        public NavMeshNode(int idx, Vector3 pos)
        {
            index = idx;
            position = pos;
            neighbors = new List<int>();
        }
    }

    [Header("Graph Data (Read‑Only)")]
    [SerializeField] private List<NavMeshNode> nodes = new List<NavMeshNode>();
    public List<NavMeshNode> Nodes => nodes;

    private bool graphBuilt = false;
    private int retryCount = 0;
    private const int MAX_RETRIES = 5;
    private const float RETRY_DELAY = 1f;

    void Start()
    {
        Debug.Log("CustomNavMeshGraph: Starting graph construction for current NavMesh.");
        StartCoroutine(BuildGraphWithRetry());
    }

    IEnumerator BuildGraphWithRetry()
    {
        while (retryCount < MAX_RETRIES)
        {
            if (TryBuildGraph())
                yield break;

            retryCount++;
            Debug.Log($"CustomNavMeshGraph: Retry {retryCount}/{MAX_RETRIES} in {RETRY_DELAY}s");
            yield return new WaitForSeconds(RETRY_DELAY);
        }
        Debug.LogError("CustomNavMeshGraph: Failed to build graph after multiple retries. Ensure NavMesh is baked and walkable areas exist.");
    }

    private bool TryBuildGraph()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Vector3[] vertices = triangulation.vertices;
        int[] indices = triangulation.indices;

        if (vertices == null || vertices.Length == 0)
        {
            Debug.LogWarning("CustomNavMeshGraph: No vertices found. NavMesh may not be baked yet.");
            return false;
        }

        if (indices == null || indices.Length < 3)
        {
            Debug.LogWarning("CustomNavMeshGraph: Invalid index data.");
            return false;
        }

        int triangleCount = indices.Length / 3;
        if (triangleCount == 0)
        {
            Debug.LogWarning("CustomNavMeshGraph: No triangles (walkable surfaces).");
            return false;
        }

        Debug.Log($"CustomNavMeshGraph: Found {triangleCount} triangles. Building nodes...");

        // Build nodes
        nodes.Clear();
        for (int i = 0; i < triangleCount; i++)
        {
            int v1 = indices[i * 3];
            int v2 = indices[i * 3 + 1];
            int v3 = indices[i * 3 + 2];
            Vector3 centroid = (vertices[v1] + vertices[v2] + vertices[v3]) / 3f;
            nodes.Add(new NavMeshNode(i, centroid));
        }

        // Build adjacency (shared edges)
        for (int i = 0; i < triangleCount; i++)
        {
            int i1 = indices[i * 3];
            int i2 = indices[i * 3 + 1];
            int i3 = indices[i * 3 + 2];

            for (int j = i + 1; j < triangleCount; j++)
            {
                int j1 = indices[j * 3];
                int j2 = indices[j * 3 + 1];
                int j3 = indices[j * 3 + 2];

                int shared = 0;
                if (i1 == j1 || i1 == j2 || i1 == j3) shared++;
                if (i2 == j1 || i2 == j2 || i2 == j3) shared++;
                if (i3 == j1 || i3 == j2 || i3 == j3) shared++;

                if (shared >= 2)
                {
                    nodes[i].neighbors.Add(j);
                    nodes[j].neighbors.Add(i);
                }
            }
        }

        // Report statistics
        int totalNeighbors = 0;
        int isolated = 0;
        foreach (var n in nodes)
        {
            totalNeighbors += n.neighbors.Count;
            if (n.neighbors.Count == 0) isolated++;
        }
        float avg = nodes.Count > 0 ? (float)totalNeighbors / nodes.Count : 0f;
        Debug.Log($"CustomNavMeshGraph: Zone A graph built. Nodes: {nodes.Count}, Avg neighbors: {avg:F2}, Isolated: {isolated}");

        graphBuilt = true;
        return true;
    }

    /// <summary>
    /// Returns the closest node to a world position (linear search).
    /// </summary>
    public NavMeshNode GetClosestNode(Vector3 worldPos)
    {
        if (!graphBuilt || nodes.Count == 0) return null;

        NavMeshNode closest = null;
        float closestDistSqr = float.MaxValue;
        foreach (var node in nodes)
        {
            float distSqr = (node.position - worldPos).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = node;
            }
        }
        return closest;
    }

    /// <summary>
    /// Editor‑only Gizmos to visualise nodes (cyan spheres) and edges (yellow lines) while playing.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !graphBuilt || nodes.Count == 0) return;

        Gizmos.color = Color.cyan;
        foreach (var node in nodes)
            Gizmos.DrawSphere(node.position, 0.2f);

        Gizmos.color = Color.yellow;
        foreach (var node in nodes)
        {
            Vector3 from = node.position;
            foreach (int nb in node.neighbors)
            {
                Vector3 to = nodes[nb].position;
                Gizmos.DrawLine(from, to);
            }
        }
    }
}