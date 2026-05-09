using System.Collections.Generic;
using UnityEngine;

public class DynamicBlocker : MonoBehaviour
{
    public GraphLoader graphLoader; // Assign GraphLoader from scene
    public float effectRadius = 2f; // Radius around this object to block nodes

    private List<Node> affectedNodes = new List<Node>();

    void OnTriggerEnter(Collider other)
    {
        // Find nodes within radius and mark as unwalkable
        affectedNodes.Clear();
        foreach (Node node in graphLoader.Nodes)
        {
            if (Vector3.Distance(transform.position, node.Position) <= effectRadius)
            {
                node.Walkable = false;
                affectedNodes.Add(node);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Restore walkable nodes
        foreach (Node node in affectedNodes)
        {
            node.Walkable = true;
        }
        affectedNodes.Clear();
    }
}