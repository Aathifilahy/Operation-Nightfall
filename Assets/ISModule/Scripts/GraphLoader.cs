using System.Collections.Generic;
using UnityEngine;

// Converts CustomNavMeshGraph into A* nodes
public class GraphLoader : MonoBehaviour
{
    public CustomNavMeshGraph customGraph; // Assign in Inspector
    public List<Node> Nodes = new List<Node>();

    void Awake()
    {
        if (customGraph == null)
        {
            Debug.LogError("CustomNavMeshGraph reference not assigned!");
            return;
        }
        LoadGraph();
    }

    public void LoadGraph()
    {
        Nodes.Clear();

        // Create Node objects
        for (int i = 0; i < customGraph.Nodes.Count; i++)
        {
            var graphNode = customGraph.Nodes[i];
            Node node = new Node(graphNode.Position, !graphNode.Blocked);
            Nodes.Add(node);
        }

        // Assign neighbors
        for (int i = 0; i < customGraph.Nodes.Count; i++)
        {
            Node currentNode = Nodes[i];
            currentNode.Parent = null;

            foreach (int neighborIndex in customGraph.Nodes[i].Neighbors)
            {
                if (neighborIndex >= 0 && neighborIndex < Nodes.Count)
                    currentNode.neighbors.Add(Nodes[neighborIndex]);
            }
        }

        Debug.Log($"Graph loaded: {Nodes.Count} nodes.");
    }

    public Node GetClosestNode(Vector3 position)
    {
        Node closest = null;
        float minDist = Mathf.Infinity;

        foreach (Node node in Nodes)
        {
            float dist = Vector3.Distance(position, node.Position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }
}