using UnityEngine;
using System.Collections.Generic;

public class PathTest : MonoBehaviour
{
    public AStarPathfinder pathfinder;
    public Transform startPoint;
    public Transform goalPoint;

    public bool runOnStart = true;
    public KeyCode testKey = KeyCode.P;

    private List<Vector3> lastPath;

    void Start()
    {
        if (runOnStart)
        {
            TestPath();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log("P pressed. Testing A* path again...");
            TestPath();
        }
    }

    void TestPath()
    {
        if (pathfinder == null)
        {
            Debug.LogError("PathTest: Pathfinder is not assigned.");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("PathTest: StartPoint is not assigned.");
            return;
        }

        if (goalPoint == null)
        {
            Debug.LogError("PathTest: GoalPoint is not assigned.");
            return;
        }

        lastPath = pathfinder.FindPath(startPoint.position, goalPoint.position);

        if (lastPath != null && lastPath.Count > 0)
        {
            Debug.Log("Path found with " + lastPath.Count + " nodes.");

            for (int i = 0; i < lastPath.Count; i++)
            {
                Debug.Log("Path node " + i + ": " + lastPath[i]);
            }
        }
        else
        {
            Debug.LogWarning("No path found.");
        }
    }

    void OnDrawGizmos()
    {
        if (lastPath == null || lastPath.Count == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < lastPath.Count; i++)
        {
            Gizmos.DrawSphere(lastPath[i], 0.25f);

            if (i < lastPath.Count - 1)
            {
                Gizmos.DrawLine(lastPath[i], lastPath[i + 1]);
            }
        }
    }
}