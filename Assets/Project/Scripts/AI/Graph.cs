using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    // All nodes, stored in a list so we can look them up by index
    public List<Node> nodes = new List<Node>();

    // Adjacency list: node ID → list of neighbouring node IDs
    private Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();

    // Add a node and return its ID
    public int AddNode(Vector3 position)
    {
        int id = nodes.Count;
        nodes.Add(new Node(id, position));
        adjacency[id] = new List<int>();
        return id;
    }

    // Add a two‑way connection between two nodes
    public void AddEdge(int nodeA, int nodeB)
    {
        if (!adjacency[nodeA].Contains(nodeB))
            adjacency[nodeA].Add(nodeB);
        if (!adjacency[nodeB].Contains(nodeA))
            adjacency[nodeB].Add(nodeA);
    }

    // Get the neighbours of a given node (used heavily by A* etc.)
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        if (adjacency.ContainsKey(node.id))
        {
            foreach (int neighbourId in adjacency[node.id])
                neighbours.Add(nodes[neighbourId]);
        }
        return neighbours;
    }
}