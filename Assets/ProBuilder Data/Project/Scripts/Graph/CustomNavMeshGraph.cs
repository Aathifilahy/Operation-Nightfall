using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom navigation graph for Zone A.
/// Compatible with AStarPathfinder and GraphLoader.
/// </summary>
public class CustomNavMeshGraph : MonoBehaviour
{
    [Header("Graph Nodes")]
    public List<NavMeshNode> Nodes = new List<NavMeshNode>();

    /// <summary>
    /// Finds the closest graph node to a world position.
    /// </summary>
    public NavMeshNode GetClosestNode(Vector3 worldPosition)
    {
        if (Nodes == null || Nodes.Count == 0)
        {
            Debug.LogWarning("CustomNavMeshGraph: No nodes available.");
            return null;
        }

        NavMeshNode closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (NavMeshNode node in Nodes)
        {
            if (node == null)
                continue;

            if (node.Blocked)
                continue;

            float distance = Vector3.Distance(worldPosition, node.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    /// <summary>
    /// Automatically assigns node indexes.
    /// </summary>
    public void RefreshNodeIndexes()
    {
        if (Nodes == null)
            return;

        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i] != null)
                Nodes[i].index = i;
        }
    }

    private void OnValidate()
    {
        RefreshNodeIndexes();
    }

    /// <summary>
    /// Draw nodes and edges in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Nodes == null)
            return;

        RefreshNodeIndexes();

        foreach (NavMeshNode node in Nodes)
        {
            if (node == null)
                continue;

            Gizmos.color = node.Blocked ? Color.red : Color.yellow;
            Gizmos.DrawSphere(node.position, 0.2f);

            if (node.neighbors == null)
                continue;

            Gizmos.color = Color.cyan;

            foreach (int neighborIndex in node.neighbors)
            {
                if (neighborIndex < 0 || neighborIndex >= Nodes.Count)
                    continue;

                if (Nodes[neighborIndex] == null)
                    continue;

                Gizmos.DrawLine(node.position, Nodes[neighborIndex].position);
            }
        }
    }

    /// <summary>
    /// Node class used by AStarPathfinder and GraphLoader.
    /// Includes both lowercase and uppercase names for compatibility.
    /// </summary>
    [System.Serializable]
    public class NavMeshNode
    {
        [Header("Node Data")]
        public int index;
        public Vector3 position;
        public bool Blocked;

        [Header("Connected Node Indexes")]
        public List<int> neighbors = new List<int>();

        // Compatibility with old GraphLoader.cs
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public List<int> Neighbors
        {
            get { return neighbors; }
            set { neighbors = value; }
        }
    }
}