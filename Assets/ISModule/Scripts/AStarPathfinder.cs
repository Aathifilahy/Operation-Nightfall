using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fully functional A* pathfinding script for Zone A using CustomNavMeshGraph.
/// Supports:
/// 1. Static blocked nodes from CustomNavMeshGraph
/// 2. Dynamic blocked edges for throwable objects / DynamicBlockers
/// 3. Returns List<Vector3> for AgentController to follow
/// 4. Debug drawing for last path and blocked edges
/// </summary>
public class AStarPathfinder : MonoBehaviour
{
    [Header("Graph Reference")]
    public CustomNavMeshGraph graph;

    [Header("Dynamic Blocked Edges")]
    public List<BlockedEdge> blockedEdges = new List<BlockedEdge>();

    [Header("Debug")]
    public bool drawLastPath = true;
    public bool drawBlockedEdges = true;

    private List<Vector3> lastPath = new List<Vector3>();

    /// <summary>
    /// Public method used by AgentController or PathTest.
    /// Finds a path from world start position to world goal position.
    /// </summary>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        if (graph == null)
        {
            Debug.LogError("AStarPathfinder: Graph reference is missing.");
            ClearLastPath();
            return null;
        }

        if (graph.Nodes == null || graph.Nodes.Count == 0)
        {
            Debug.LogError("AStarPathfinder: Graph has no nodes.");
            ClearLastPath();
            return null;
        }

        var startNode = graph.GetClosestNode(startPos);
        var goalNode = graph.GetClosestNode(goalPos);

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning("AStarPathfinder: Start or Goal node not found.");
            ClearLastPath();
            return null;
        }

        // NEW: Do not allow pathfinding if start or goal node is blocked.
        if (IsNodeBlocked(startNode))
        {
            Debug.LogWarning("AStarPathfinder: Start node is blocked.");
            ClearLastPath();
            return null;
        }

        if (IsNodeBlocked(goalNode))
        {
            Debug.LogWarning("AStarPathfinder: Goal node is blocked.");
            ClearLastPath();
            return null;
        }

        lastPath = FindPath(startNode, goalNode);

        if (lastPath == null || lastPath.Count == 0)
        {
            ClearLastPath();
            return null;
        }

        return lastPath;
    }

    /// <summary>
    /// Core A* algorithm.
    /// f(n) = g(n) + h(n)
    /// g(n) = actual cost from start to current node
    /// h(n) = estimated cost from current node to goal
    /// </summary>
    private List<Vector3> FindPath(CustomNavMeshGraph.NavMeshNode start, CustomNavMeshGraph.NavMeshNode goal)
    {
        PriorityQueue<CustomNavMeshGraph.NavMeshNode> openSet =
            new PriorityQueue<CustomNavMeshGraph.NavMeshNode>();

        HashSet<int> closedSet = new HashSet<int>();

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> gScore = new Dictionary<int, float>();
        Dictionary<int, float> fScore = new Dictionary<int, float>();

        foreach (var node in graph.Nodes)
        {
            gScore[node.index] = Mathf.Infinity;
            fScore[node.index] = Mathf.Infinity;
        }

        gScore[start.index] = 0f;
        fScore[start.index] = Heuristic(start, goal);

        openSet.Enqueue(start, fScore[start.index]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.index == goal.index)
            {
                return ReconstructPath(cameFrom, current);
            }

            closedSet.Add(current.index);

            foreach (int neighborIndex in current.neighbors)
            {
                if (neighborIndex < 0 || neighborIndex >= graph.Nodes.Count)
                {
                    Debug.LogWarning($"AStarPathfinder: Invalid neighbor index {neighborIndex}");
                    continue;
                }

                // Existing working feature: skip dynamically blocked edges.
                if (IsEdgeBlocked(current.index, neighborIndex))
                    continue;

                if (closedSet.Contains(neighborIndex))
                    continue;

                var neighbor = graph.Nodes[neighborIndex];

                // NEW: skip blocked graph nodes.
                if (IsNodeBlocked(neighbor))
                    continue;

                float movementCost = Vector3.Distance(current.position, neighbor.position);
                float tentativeGScore = gScore[current.index] + movementCost;

                if (tentativeGScore < gScore[neighbor.index])
                {
                    cameFrom[neighbor.index] = current.index;
                    gScore[neighbor.index] = tentativeGScore;
                    fScore[neighbor.index] = tentativeGScore + Heuristic(neighbor, goal);

                    if (openSet.Contains(neighbor))
                    {
                        openSet.UpdatePriority(neighbor, fScore[neighbor.index]);
                    }
                    else
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor.index]);
                    }
                }
            }
        }

        Debug.LogWarning("AStarPathfinder: No path found.");
        return null;
    }

    /// <summary>
    /// Checks whether a graph node is blocked.
    /// This uses the blocked value from CustomNavMeshGraph.
    /// </summary>
    private bool IsNodeBlocked(CustomNavMeshGraph.NavMeshNode node)
    {
        if (node == null)
            return true;

        return node.Blocked;
    }

    /// <summary>
    /// Euclidean distance heuristic.
    /// Suitable for 3D movement because it estimates straight-line distance.
    /// </summary>
    private float Heuristic(CustomNavMeshGraph.NavMeshNode a, CustomNavMeshGraph.NavMeshNode b)
    {
        return Vector3.Distance(a.position, b.position);
    }

    /// <summary>
    /// Reconstructs the final path from goal node back to start node.
    /// </summary>
    private List<Vector3> ReconstructPath(
        Dictionary<int, int> cameFrom,
        CustomNavMeshGraph.NavMeshNode current)
    {
        List<Vector3> totalPath = new List<Vector3>();
        totalPath.Add(current.position);

        while (cameFrom.ContainsKey(current.index))
        {
            current = graph.Nodes[cameFrom[current.index]];
            totalPath.Insert(0, current.position);
        }

        return totalPath;
    }

    /// <summary>
    /// Checks whether an edge is blocked in either direction.
    /// </summary>
    public bool IsEdgeBlocked(int fromIndex, int toIndex)
    {
        foreach (BlockedEdge edge in blockedEdges)
        {
            bool sameDirection = edge.fromIndex == fromIndex && edge.toIndex == toIndex;
            bool oppositeDirection = edge.fromIndex == toIndex && edge.toIndex == fromIndex;

            if (sameDirection || oppositeDirection)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Called by DynamicBlocker when an object blocks movement between two nodes.
    /// </summary>
    public void BlockEdge(int fromIndex, int toIndex)
    {
        if (IsEdgeBlocked(fromIndex, toIndex))
            return;

        blockedEdges.Add(new BlockedEdge(fromIndex, toIndex));
        Debug.Log($"AStarPathfinder: Edge blocked between {fromIndex} and {toIndex}");
    }

    /// <summary>
    /// Called when a dynamic blocker is removed.
    /// </summary>
    public void UnblockEdge(int fromIndex, int toIndex)
    {
        blockedEdges.RemoveAll(edge =>
            (edge.fromIndex == fromIndex && edge.toIndex == toIndex) ||
            (edge.fromIndex == toIndex && edge.toIndex == fromIndex));

        Debug.Log($"AStarPathfinder: Edge unblocked between {fromIndex} and {toIndex}");
    }

    /// <summary>
    /// Clears all dynamic blocked edges.
    /// Useful for testing.
    /// </summary>
    public void ClearBlockedEdges()
    {
        blockedEdges.Clear();
        Debug.Log("AStarPathfinder: All blocked edges cleared.");
    }

    /// <summary>
    /// Clears the last path so old green debug lines do not remain after no path is found.
    /// </summary>
    private void ClearLastPath()
    {
        lastPath = new List<Vector3>();
    }

    private void OnDrawGizmos()
    {
        if (graph == null || graph.Nodes == null)
            return;

        if (drawBlockedEdges)
        {
            Gizmos.color = Color.red;

            foreach (BlockedEdge edge in blockedEdges)
            {
                if (edge.fromIndex < 0 || edge.fromIndex >= graph.Nodes.Count)
                    continue;

                if (edge.toIndex < 0 || edge.toIndex >= graph.Nodes.Count)
                    continue;

                Vector3 from = graph.Nodes[edge.fromIndex].position;
                Vector3 to = graph.Nodes[edge.toIndex].position;

                Gizmos.DrawLine(from, to);
            }
        }

        if (drawLastPath && lastPath != null && lastPath.Count > 1)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < lastPath.Count - 1; i++)
            {
                Gizmos.DrawLine(lastPath[i], lastPath[i + 1]);
                Gizmos.DrawSphere(lastPath[i], 0.15f);
            }

            Gizmos.DrawSphere(lastPath[lastPath.Count - 1], 0.15f);
        }
    }
}

/// <summary>
/// Unity-serializable edge class.
/// Better than List<(int, int)> because Unity Inspector can show this.
/// </summary>
[System.Serializable]
public class BlockedEdge
{
    public int fromIndex;
    public int toIndex;

    public BlockedEdge(int fromIndex, int toIndex)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;
    }
}

/// <summary>
/// Simple priority queue for A* open set.
/// Lowest priority value is dequeued first.
/// </summary>
public class PriorityQueue<T>
{
    private List<PriorityQueueElement<T>> elements = new List<PriorityQueueElement<T>>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new PriorityQueueElement<T>(item, priority));
        SortByPriority();
    }

    public T Dequeue()
    {
        T item = elements[0].item;
        elements.RemoveAt(0);
        return item;
    }

    public bool Contains(T item)
    {
        foreach (var element in elements)
        {
            if (EqualityComparer<T>.Default.Equals(element.item, item))
                return true;
        }

        return false;
    }

    public void UpdatePriority(T item, float newPriority)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(elements[i].item, item))
            {
                elements[i].priority = newPriority;
                SortByPriority();
                return;
            }
        }
    }

    private void SortByPriority()
    {
        elements.Sort((a, b) => a.priority.CompareTo(b.priority));
    }
}

public class PriorityQueueElement<T>
{
    public T item;
    public float priority;

    public PriorityQueueElement(T item, float priority)
    {
        this.item = item;
        this.priority = priority;
    }
}