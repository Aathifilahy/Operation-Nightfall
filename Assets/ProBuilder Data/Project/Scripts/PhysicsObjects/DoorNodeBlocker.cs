using System;
using UnityEngine;

/// <summary>
/// Attach this to a door object.
/// It blocks/unblocks one A* graph node based on door state.
/// Door closed = node blocked.
/// Door open = node unblocked.
/// No DynamicBlocker tag needed.
/// No DoorwayBlockZone needed for doors.
/// </summary>
public class DoorNodeBlocker : MonoBehaviour
{
    public static event Action OnDoorGraphStateChanged;

    [Header("Graph Reference")]
    public CustomNavMeshGraph graph;

    [Header("Doorway Node Controlled By This Door")]
    public int doorwayNodeIndex = -1;

    [Header("Debug")]
    public bool logChanges = true;

    private bool hasAppliedState = false;
    private bool currentBlockedState = false;

    public void MarkOpen()
    {
        SetNodeBlocked(false);
    }

    public void MarkClosed()
    {
        SetNodeBlocked(true);
    }

    public void SetDoorOpen(bool open)
    {
        SetNodeBlocked(!open);
    }

    public void SetDoorClosed(bool closed)
    {
        SetNodeBlocked(closed);
    }

    private void SetNodeBlocked(bool blocked)
    {
        if (graph == null)
        {
            Debug.LogError($"{gameObject.name}: DoorNodeBlocker graph reference is missing.");
            return;
        }

        if (graph.Nodes == null || doorwayNodeIndex < 0 || doorwayNodeIndex >= graph.Nodes.Count)
        {
            Debug.LogError($"{gameObject.name}: Invalid doorway node index {doorwayNodeIndex}.");
            return;
        }

        if (hasAppliedState &&
            currentBlockedState == blocked &&
            graph.Nodes[doorwayNodeIndex].Blocked == blocked)
        {
            return;
        }

        graph.Nodes[doorwayNodeIndex].Blocked = blocked;

        currentBlockedState = blocked;
        hasAppliedState = true;

        OnDoorGraphStateChanged?.Invoke();

        if (logChanges)
        {
            string state = blocked ? "BLOCKED" : "UNBLOCKED";
            Debug.Log($"{gameObject.name}: Doorway node {doorwayNodeIndex} {state} by door state.");
        }
    }
}