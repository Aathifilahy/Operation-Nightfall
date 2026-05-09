using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AStarPathfinder pathfinder;  // Assign AStarManager here
    public Transform target;            // Set target transform in Inspector
    public float moveSpeed = 3f;

    private List<Vector3> currentPath;
    private int currentIndex = 0;

    void Update()
    {
        if (target == null || pathfinder == null) return;

        // Update path dynamically
        currentPath = pathfinder.FindPath(transform.position, target.position);

        if (currentPath != null && currentPath.Count > 0)
        {
            Vector3 goal = currentPath[currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, goal, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, goal) < 0.1f)
            {
                currentIndex++;
                if (currentIndex >= currentPath.Count) currentIndex = currentPath.Count - 1;
            }
        }
    }
}