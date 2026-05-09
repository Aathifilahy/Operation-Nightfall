using UnityEngine;

public static class Heuristic
{
    // Euclidean distance heuristic
    public static float Estimate(Node fromNode, Node toNode)
    {
        return Vector3.Distance(fromNode.Position, toNode.Position);
    }

    // Optional: Manhattan distance for grid-like movement
    public static float Manhattan(Node fromNode, Node toNode)
    {
        Vector3 diff = fromNode.Position - toNode.Position;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z);
    }
}