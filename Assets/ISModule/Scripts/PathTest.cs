using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Temporary testing script for A* pathfinding.
/// Recalculates path when:
/// 1. Play Mode starts
/// 2. P key is pressed
/// 3. DoorwayBlockZone changes graph state for boxes
/// 4. DoorNodeBlocker changes graph state for doors
/// </summary>
public class PathTest : MonoBehaviour
{
    [Header("References")]
    public AStarPathfinder pathfinder;
    public Transform startPoint;
    public Transform goalPoint;

    [Header("Settings")]
    public bool runOnStart = true;
    public KeyCode testKey = KeyCode.P;

    [Header("Debug Drawing")]
    public bool drawPathGizmos = true;

    private List<Vector3> lastPath;
    private bool recalculateRequested = false;

    private void OnEnable()
    {
        DoorwayBlockZone.OnGraphBlockStateChanged += RequestRecalculate;
        DoorNodeBlocker.OnDoorGraphStateChanged += RequestRecalculate;
    }

    private void OnDisable()
    {
        DoorwayBlockZone.OnGraphBlockStateChanged -= RequestRecalculate;
        DoorNodeBlocker.OnDoorGraphStateChanged -= RequestRecalculate;
    }

    private void Start()
    {
        if (runOnStart)
        {
            TestPath();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log("P pressed. Testing A* path again...");
            TestPath();
        }

        if (recalculateRequested)
        {
            recalculateRequested = false;
            Debug.Log("Graph changed. Recalculating A* path...");
            TestPath();
        }
    }

    private void RequestRecalculate()
    {
        recalculateRequested = true;
    }

    private void TestPath()
    {
        if (pathfinder == null)
        {
            Debug.LogError("PathTest: Pathfinder is not assigned.");
            lastPath = null;
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("PathTest: StartPoint is not assigned.");
            lastPath = null;
            return;
        }

        if (goalPoint == null)
        {
            Debug.LogError("PathTest: GoalPoint is not assigned.");
            lastPath = null;
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
            lastPath = null;
            Debug.LogWarning("No path found.");
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawPathGizmos)
            return;

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