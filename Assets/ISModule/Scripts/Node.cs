using System.Collections.Generic;
using UnityEngine;

// Node for A* pathfinding
public class Node
{
    public Vector3 Position;
    public List<Node> neighbors;  // Used by AStarPathfinder
    public float GCost;
    public float HCost;
    public Node Parent;
    public bool Walkable;

    public float FCost { get { return GCost + HCost; } }

    public Node(Vector3 pos, bool walkable = true)
    {
        Position = pos;
        Walkable = walkable;
        neighbors = new List<Node>();
    }
}