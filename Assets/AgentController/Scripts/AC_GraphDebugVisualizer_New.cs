using UnityEngine;

/// <summary>
/// Debug visualizer for graph nodes in Zone A.
/// Press G to toggle visualization.
/// </summary>
public class AC_GraphDebugVisualizer_New : MonoBehaviour
{
    [Header("Debug Toggle")]
    public bool ShowDebug = true;           
    public KeyCode ToggleKey = KeyCode.G;   

    [Header("Graph Nodes")]
    public Transform Node_Spawn;
    public Transform Node_Key;
    public Transform Node_Door;
    public Transform Node_UpperRoom;
    public Transform Node_BelowRoom;
    public Transform Node_LeftRoom;
    public Transform Node_RightRoom;
    public Transform Node_UpperLeftRoom;
    public Transform Node_UpperRightRoom;
    public Transform Node_BelowLeftRoom;
    public Transform Node_BelowRightRoom;
    public Transform Node_CentralRoom;

    void Update()
    {
        // Toggle debug visualization
        if (Input.GetKeyDown(ToggleKey))
            ShowDebug = !ShowDebug;
    }

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        Gizmos.color = Color.yellow;

        // Connect edges between nodes (adjust as needed)
        DrawEdge(Node_Spawn, Node_CentralRoom);
        DrawEdge(Node_CentralRoom, Node_UpperRoom);
        DrawEdge(Node_CentralRoom, Node_BelowRoom);
        DrawEdge(Node_CentralRoom, Node_LeftRoom);
        DrawEdge(Node_CentralRoom, Node_RightRoom);

        DrawEdge(Node_UpperRoom, Node_UpperLeftRoom);
        DrawEdge(Node_UpperRoom, Node_UpperRightRoom);

        DrawEdge(Node_BelowRoom, Node_BelowLeftRoom);
        DrawEdge(Node_BelowRoom, Node_BelowRightRoom);

        DrawEdge(Node_Spawn, Node_Key);
        DrawEdge(Node_CentralRoom, Node_Door);
    }

    /// <summary>
    /// Draw a line between two nodes if both are assigned
    /// </summary>
    void DrawEdge(Transform fromNode, Transform toNode)
    {
        if (fromNode == null || toNode == null) return;
        Gizmos.DrawLine(fromNode.position, toNode.position);
    }
}