using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    public GraphLoader graphLoader; // Assign in Inspector
    public bool showDebugGizmos = true;

    private List<Node> lastPath = new List<Node>();

    void Awake()
    {
        if (graphLoader == null)
            Debug.LogError("GraphLoader not assigned!");
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        Node startNode = graphLoader.GetClosestNode(startPos);
        Node goalNode = graphLoader.GetClosestNode(goalPos);

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning(
                $"Pathfinding Failed: StartNode is {(startNode == null ? "NULL" : "OK")}, " +
                $"GoalNode is {(goalNode == null ? "NULL" : "OK")}. " +
                "Check if points are within the graph bounds.");
            return null;
        }

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = Heuristic.Estimate(startNode, goalNode);
        startNode.Parent = null;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == goalNode)
            {
                List<Vector3> path = ReconstructPath(goalNode);
                lastPath = new List<Node>();
                foreach (var n in path)
                    lastPath.Add(graphLoader.GetClosestNode(n));
                return path;
            }

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = currentNode.GCost + Vector3.Distance(currentNode.Position, neighbor.Position);

                if (!openSet.Contains(neighbor) || tentativeG < neighbor.GCost)
                {
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = Heuristic.Estimate(neighbor, goalNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("No path found!");
        return null;
    }

    private List<Vector3> ReconstructPath(Node goalNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = goalNode;
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        if (graphLoader != null && graphLoader.Nodes != null)
        {
            Gizmos.color = Color.red;
            foreach (var node in graphLoader.Nodes)
            {
                if (node == null || node.neighbors == null)
                    continue;

                foreach (var neighbor in node.neighbors)
                {
                    if (neighbor == null)
                        continue;

                    Gizmos.DrawLine(node.Position, neighbor.Position);
                }
            }
        }

        if (lastPath != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < lastPath.Count - 1; i++)
            {
                Gizmos.DrawLine(lastPath[i].Position, lastPath[i + 1].Position);
            }
        }
    }
}