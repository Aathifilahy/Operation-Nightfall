using System.Collections.Generic;
using UnityEngine;

public class AC_BFSSearch_Integration : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public AC_PathFollower PathFollower;  // Your guard prefab in the scene
    public Transform NodeA;
    public Transform NodeB;
    public Transform NodeC;
    public Transform NodeD;
    public Transform NodeE;
    public Transform NodeF;

    // Graph stored as Transform nodes
    private Dictionary<Transform, List<Transform>> graph = new Dictionary<Transform, List<Transform>>();

    void Start()
    {
        CreateGraph();

        // Run BFS from NodeA to NodeF
        List<Transform> path = RunBFS(NodeA, NodeF);

        Debug.Log("Final BFS Path: " + string.Join(" -> ", path.ConvertAll(n => n.name)));

        // Assign path to guard
        if (PathFollower != null)
        {
            PathFollower.PathNodes = path;
        }
    }

    void CreateGraph()
    {
        graph[NodeA] = new List<Transform> { NodeB, NodeC };
        graph[NodeB] = new List<Transform> { NodeA, NodeD };
        graph[NodeC] = new List<Transform> { NodeA, NodeE };
        graph[NodeD] = new List<Transform> { NodeB, NodeF };
        graph[NodeE] = new List<Transform> { NodeC, NodeF };
        graph[NodeF] = new List<Transform> { NodeD, NodeE };
    }

    List<Transform> RunBFS(Transform startNode, Transform goalNode)
    {
        Queue<Transform> queue = new Queue<Transform>();
        HashSet<Transform> visited = new HashSet<Transform>();
        Dictionary<Transform, Transform> cameFrom = new Dictionary<Transform, Transform>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        Debug.Log("BFS started from " + startNode.name + " to " + goalNode.name);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            Debug.Log("Visited Node: " + current.name);

            if (current == goalNode)
            {
                Debug.Log("Goal found: " + goalNode.name);
                return BuildPath(cameFrom, startNode, goalNode);
            }

            foreach (Transform neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        Debug.LogWarning("No path found.");
        return new List<Transform>();
    }

    List<Transform> BuildPath(Dictionary<Transform, Transform> cameFrom, Transform startNode, Transform goalNode)
    {
        List<Transform> path = new List<Transform>();
        Transform current = goalNode;

        path.Add(current);

        while (current != startNode)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}