using UnityEngine;

public class AC_GraphDebugVisualizer : MonoBehaviour
{
    [Header("Debug Toggle")]
    public bool showDebug = true;
    public KeyCode toggleKey = KeyCode.G;

    [Header("Graph Nodes")]
    public Transform nodeA;
    public Transform nodeB;
    public Transform nodeC;
    public Transform nodeD;
    public Transform nodeE;
    public Transform nodeF;

    [Header("Node Display")]
    public float nodeRadius = 0.25f;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebug = !showDebug;
            Debug.Log("Graph Debug Mode: " + (showDebug ? "ON" : "OFF"));
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug)
        {
            return;
        }

        if (nodeA == null || nodeB == null || nodeC == null || nodeD == null || nodeE == null || nodeF == null)
        {
            return;
        }

        DrawGraphNodes();
        DrawGraphEdges();
        DrawFinalPath();
    }

    void DrawGraphNodes()
    {
        Gizmos.color = Color.yellow;

        DrawNode(nodeA);
        DrawNode(nodeB);
        DrawNode(nodeC);
        DrawNode(nodeD);
        DrawNode(nodeE);
        DrawNode(nodeF);
    }

    void DrawNode(Transform node)
    {
        Gizmos.DrawSphere(node.position, nodeRadius);
    }

    void DrawGraphEdges()
    {
        Gizmos.color = Color.white;

        DrawEdge(nodeA, nodeB);
        DrawEdge(nodeA, nodeC);
        DrawEdge(nodeB, nodeD);
        DrawEdge(nodeC, nodeE);
        DrawEdge(nodeD, nodeF);
        DrawEdge(nodeE, nodeF);
    }

    void DrawEdge(Transform fromNode, Transform toNode)
    {
        Gizmos.DrawLine(fromNode.position, toNode.position);
    }

    void DrawFinalPath()
    {
        Gizmos.color = Color.green;

        // Final BFS path: A -> B -> D -> F
        DrawEdge(nodeA, nodeB);
        DrawEdge(nodeB, nodeD);
        DrawEdge(nodeD, nodeF);
    }
}