using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshGraphBuilder : MonoBehaviour
{
    [Header("Debug")]
    public bool logGraphOnStart = true;   // Show graph info in the console

    // The built graph – other scripts will access this
    public Graph graph { get; private set; }

    // Singleton pattern so everyone can find it easily
    public static NavMeshGraphBuilder Instance { get; private set; }

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        BuildGraphFromNavMesh();
    }

    void BuildGraphFromNavMesh()
    {
        // 1. Get the NavMesh triangulation (all triangles of the baked NavMesh)
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        // If there’s no NavMesh baked, abort
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogError("No NavMesh data found! Bake the NavMesh first.");
            return;
        }

        // 2. Create a new graph
        graph = new Graph();

        // 3. For each triangle, compute its centroid and create a node
        int triangleCount = triangulation.indices.Length / 3;
        int[] nodeIds = new int[triangleCount]; // store node ID per triangle

        for (int i = 0; i < triangleCount; i++)
        {
            // Indices of the three vertices of this triangle
            int i1 = triangulation.indices[i * 3];
            int i2 = triangulation.indices[i * 3 + 1];
            int i3 = triangulation.indices[i * 3 + 2];

            // Compute the centre (centroid) of the triangle
            Vector3 centroid = (triangulation.vertices[i1] +
                                triangulation.vertices[i2] +
                                triangulation.vertices[i3]) / 3f;

            int id = graph.AddNode(centroid);
            nodeIds[i] = id;
        }

        // 4. Build edges: two triangles are neighbours if they share two vertices (an edge)
        // Use a dictionary to find which triangles share which edges
        Dictionary<EdgeKey, int> edgeToTriangle = new Dictionary<EdgeKey, int>();

        for (int i = 0; i < triangleCount; i++)
        {
            int i1 = triangulation.indices[i * 3];
            int i2 = triangulation.indices[i * 3 + 1];
            int i3 = triangulation.indices[i * 3 + 2];

            // Process the three edges of this triangle
            TryAddEdge(i, i1, i2, edgeToTriangle);
            TryAddEdge(i, i2, i3, edgeToTriangle);
            TryAddEdge(i, i3, i1, edgeToTriangle);
        }

        // 5. Done! Log if requested
        if (logGraphOnStart)
            Debug.Log($"Graph built: {graph.nodes.Count} nodes, adjacency ready.");
    }

    // Helper method that checks if an edge has been seen before;
    // if yes, connect the two triangles that share it.
    void TryAddEdge(int triangleIndex, int v1, int v2,
                    Dictionary<EdgeKey, int> edgeToTriangle)
    {
        EdgeKey key = new EdgeKey(v1, v2);
        if (edgeToTriangle.ContainsKey(key))
        {
            int otherTriangle = edgeToTriangle[key];
            // Connect this triangle to the previous triangle that shared this edge
            int nodeA = triangleIndex < otherTriangle ? triangleIndex : otherTriangle;
            int nodeB = triangleIndex < otherTriangle ? otherTriangle : triangleIndex;
            graph.AddEdge(nodeA, nodeB);
        }
        else
        {
            edgeToTriangle[key] = triangleIndex;
        }
    }

    // A simple struct to represent an undirected edge (order doesn't matter)
    private struct EdgeKey
    {
        public int a, b;
        public EdgeKey(int v1, int v2)
        {
            // Store the smaller index first so (0,1) and (1,0) are equal
            a = Mathf.Min(v1, v2);
            b = Mathf.Max(v1, v2);
        }
    }
}