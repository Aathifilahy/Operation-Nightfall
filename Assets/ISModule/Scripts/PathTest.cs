using UnityEngine;
using System.Collections.Generic;

public class PathTest : MonoBehaviour
{
    public AStarPathfinder pathfinder;
    public Transform startPoint;
    public Transform goalPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            List<Vector3> path = pathfinder.FindPath(startPoint.position, goalPoint.position);
            if (path != null)
            {
                Debug.Log("Path found with " + path.Count + " nodes");
            }
            else
            {
                Debug.Log("No path found");
            }
        }
    }
}