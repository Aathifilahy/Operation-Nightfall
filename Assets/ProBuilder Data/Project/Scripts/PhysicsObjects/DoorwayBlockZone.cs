using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to an invisible trigger zone placed at a doorway.
/// This version does NOT depend only on OnTriggerEnter/Exit.
/// It repeatedly checks whether a DynamicBlocker box is currently inside the doorway zone.
/// 
/// If a blocker is inside long enough, the doorway node is blocked.
/// If no blocker is inside, the doorway node is unblocked.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DoorwayBlockZone : MonoBehaviour
{
    public static event Action OnGraphBlockStateChanged;

    [Header("Graph Reference")]
    public CustomNavMeshGraph graph;

    [Header("Doorway Node To Block")]
    public int doorwayNodeIndex = -1;

    [Header("Blocker Settings")]
    public string blockerTag = "DynamicBlocker";

    [Tooltip("How long a blocker must stay inside before the doorway is considered blocked.")]
    public float blockConfirmTime = 0.3f;

    [Tooltip("How often the zone checks for blockers.")]
    public float scanInterval = 0.1f;

    [Header("Debug")]
    public bool logChanges = true;

    private BoxCollider triggerCollider;
    private bool currentlyBlocked = false;

    private float scanTimer = 0f;

    // Tracks how long each blocker has stayed inside the doorway zone.
    private Dictionary<GameObject, float> blockerInsideTimes = new Dictionary<GameObject, float>();

    private void Reset()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        ScanDoorwayZone();
    }

    private void Update()
    {
        scanTimer += Time.deltaTime;

        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            ScanDoorwayZone();
        }
    }

    private void ScanDoorwayZone()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
        }

        Vector3 worldCenter = transform.TransformPoint(triggerCollider.center);
        Vector3 worldHalfExtents = Vector3.Scale(triggerCollider.size, transform.lossyScale) * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            worldCenter,
            worldHalfExtents,
            transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        HashSet<GameObject> blockersCurrentlyInside = new HashSet<GameObject>();

        foreach (Collider hit in hits)
        {
            GameObject blockerRoot = GetBlockerRoot(hit);

            if (blockerRoot == null)
                continue;

            blockersCurrentlyInside.Add(blockerRoot);
        }

        // Remove blockers that are no longer inside.
        List<GameObject> oldBlockers = new List<GameObject>(blockerInsideTimes.Keys);

        foreach (GameObject oldBlocker in oldBlockers)
        {
            if (oldBlocker == null || !blockersCurrentlyInside.Contains(oldBlocker))
            {
                blockerInsideTimes.Remove(oldBlocker);
            }
        }

        // Add/update blockers that are currently inside.
        foreach (GameObject blocker in blockersCurrentlyInside)
        {
            if (!blockerInsideTimes.ContainsKey(blocker))
            {
                blockerInsideTimes.Add(blocker, 0f);
            }

            blockerInsideTimes[blocker] += scanInterval;
        }

        bool shouldBlock = false;

        foreach (var pair in blockerInsideTimes)
        {
            if (pair.Value >= blockConfirmTime)
            {
                shouldBlock = true;
                break;
            }
        }

        SetDoorwayBlocked(shouldBlock);
    }

    private GameObject GetBlockerRoot(Collider other)
    {
        if (other == null)
            return null;

        DynamicBlockerEvent dynamicBlocker = other.GetComponentInParent<DynamicBlockerEvent>();

        if (dynamicBlocker != null)
        {
            if (IsTaggedAsBlocker(dynamicBlocker.gameObject) ||
                IsTaggedAsBlocker(other.gameObject) ||
                IsTaggedAsBlocker(other.transform.root.gameObject))
            {
                return dynamicBlocker.gameObject;
            }
        }

        PhysicsObject physicsObject = other.GetComponentInParent<PhysicsObject>();

        if (physicsObject != null)
        {
            if (IsTaggedAsBlocker(physicsObject.gameObject) ||
                IsTaggedAsBlocker(other.gameObject) ||
                IsTaggedAsBlocker(other.transform.root.gameObject))
            {
                return physicsObject.gameObject;
            }
        }

        if (IsTaggedAsBlocker(other.gameObject))
        {
            return other.gameObject;
        }

        if (IsTaggedAsBlocker(other.transform.root.gameObject))
        {
            return other.transform.root.gameObject;
        }

        return null;
    }

    private bool IsTaggedAsBlocker(GameObject obj)
    {
        return obj != null && obj.CompareTag(blockerTag);
    }

    private void SetDoorwayBlocked(bool blocked)
    {
        if (graph == null)
        {
            Debug.LogError($"{gameObject.name}: Graph reference is missing.");
            return;
        }

        if (graph.Nodes == null || doorwayNodeIndex < 0 || doorwayNodeIndex >= graph.Nodes.Count)
        {
            Debug.LogError($"{gameObject.name}: Invalid doorway node index {doorwayNodeIndex}.");
            return;
        }

        if (currentlyBlocked == blocked && graph.Nodes[doorwayNodeIndex].Blocked == blocked)
            return;

        graph.Nodes[doorwayNodeIndex].Blocked = blocked;
        currentlyBlocked = blocked;

        OnGraphBlockStateChanged?.Invoke();

        if (logChanges)
        {
            string state = blocked ? "BLOCKED" : "UNBLOCKED";
            Debug.Log($"{gameObject.name}: Doorway node {doorwayNodeIndex} {state}. Blockers inside: {blockerInsideTimes.Count}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}