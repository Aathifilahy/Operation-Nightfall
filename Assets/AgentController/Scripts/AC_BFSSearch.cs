using System.Collections.Generic;
using UnityEngine;

public class AC_BFSSearch : MonoBehaviour
{
    private Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

    void Start()
    {
        CreateTestGraph();

        List<string> finalPath = RunBFS("A", "F");

        Debug.Log("Final BFS Path: " + string.Join(" -> ", finalPath));
    }

    void CreateTestGraph()
    {
        graph["A"] = new List<string> { "B", "C" };
        graph["B"] = new List<string> { "A", "D" };
        graph["C"] = new List<string> { "A", "E" };
        graph["D"] = new List<string> { "B", "F" };
        graph["E"] = new List<string> { "C", "F" };
        graph["F"] = new List<string> { "D", "E" };
    }

    List<string> RunBFS(string startNode, string goalNode)
    {
        Queue<string> queue = new Queue<string>();
        HashSet<string> visited = new HashSet<string>();
        Dictionary<string, string> cameFrom = new Dictionary<string, string>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        Debug.Log("BFS started from " + startNode + " to " + goalNode);

        while (queue.Count > 0)
        {
            string currentNode = queue.Dequeue();

            Debug.Log("Visited Node: " + currentNode);

            if (currentNode == goalNode)
            {
                Debug.Log("Goal found: " + goalNode);
                return BuildPath(cameFrom, startNode, goalNode);
            }

            foreach (string neighbour in graph[currentNode])
            {
                if (!visited.Contains(neighbour))
                {
                    visited.Add(neighbour);
                    cameFrom[neighbour] = currentNode;
                    queue.Enqueue(neighbour);
                }
            }
        }

        Debug.LogWarning("No path found.");
        return new List<string>();
    }

    List<string> BuildPath(Dictionary<string, string> cameFrom, string startNode, string goalNode)
    {
        List<string> path = new List<string>();
        string currentNode = goalNode;

        path.Add(currentNode);

        while (currentNode != startNode)
        {
            currentNode = cameFrom[currentNode];
            path.Add(currentNode);
        }

        path.Reverse();
        return path;
    }
}