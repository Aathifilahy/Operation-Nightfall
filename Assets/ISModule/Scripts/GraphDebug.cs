using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways] // So you can see it in Edit Mode
public class GraphDebug : MonoBehaviour
{
    public GraphLoader loader; // Drag your GraphLoader here
    public float nodeSphereSize = 0.2f; // Size of the spheres to visualize nodes
    public Color nodeColor = Color.yellow;
    public Color neighborLineColor = Color.red;

    private void OnDrawGizmos()
    {
        if (loader != null && loader.Nodes != null)
        {
            foreach (var node in loader.Nodes)
            {
                if (node == null)
                    continue;

                // Draw the node as a sphere
                Gizmos.color = nodeColor;
                Gizmos.DrawSphere(node.Position, nodeSphereSize);

                // Draw lines to neighbors
                if (node.neighbors != null)
                {
                    foreach (var neighbor in node.neighbors)
                    {
                        if (neighbor == null)
                            continue;

                        Gizmos.color = neighborLineColor;
                        Gizmos.DrawLine(node.Position, neighbor.Position);
                    }
                }
            }
        }
    }
}