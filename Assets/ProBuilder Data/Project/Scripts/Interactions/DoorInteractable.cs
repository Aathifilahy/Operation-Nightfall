using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public enum DoorWidthAxis
    {
        LocalX,
        LocalZ
    }

    public enum HingeSide
    {
        NegativeSide,
        PositiveSide
    }

    [Header("Door Movement")]
    public float openAngle = 90f;
    public float openSpeed = 120f; // degrees per second
    public bool isOpen = false;

    [Header("Auto Hinge Settings")]
    public bool autoCalculateHinge = true;
    public DoorWidthAxis doorWidthAxis = DoorWidthAxis.LocalX;
    public HingeSide hingeSide = HingeSide.NegativeSide;

    [Tooltip("Used only if Auto Calculate Hinge is false.")]
    public Vector3 manualLocalHingePoint = Vector3.zero;

    [Header("Access")]
    public bool requiresKeycard = true;

    [Header("A* Node Blocking")]
    public DoorNodeBlocker doorNodeBlocker;

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    private Vector3 hingeWorldPoint;
    private Vector3 hingeAxisWorld;

    private void Start()
    {
        hingeAxisWorld = transform.up;

        if (autoCalculateHinge)
        {
            hingeWorldPoint = CalculateHingeWorldPoint();
        }
        else
        {
            hingeWorldPoint = transform.TransformPoint(manualLocalHingePoint);
        }

        if (doorNodeBlocker == null)
        {
            doorNodeBlocker = GetComponent<DoorNodeBlocker>();
        }

        currentAngle = isOpen ? openAngle : 0f;
        targetAngle = currentAngle;

        ApplyDoorNodeState();
    }

    private void Update()
    {
        float newAngle = Mathf.MoveTowards(
            currentAngle,
            targetAngle,
            openSpeed * Time.deltaTime
        );

        float deltaAngle = newAngle - currentAngle;

        if (Mathf.Abs(deltaAngle) > 0.001f)
        {
            transform.RotateAround(hingeWorldPoint, hingeAxisWorld, deltaAngle);
            currentAngle = newAngle;
        }
    }

    public void Interact(GameObject player)
    {
        InteractionUI ui = FindFirstObjectByType<InteractionUI>();

        if (requiresKeycard)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if (inventory != null && inventory.hasKeycard)
            {
                ToggleDoor();

                if (ui != null)
                {
                    ui.ShowFeedback(isOpen ? "Door opened." : "Door closed.");
                }
            }
            else
            {
                if (ui != null)
                {
                    ui.ShowFeedback("Door is locked. Need keycard.");
                }

                Debug.Log("Door is locked. Need keycard.");
            }
        }
        else
        {
            ToggleDoor();

            if (ui != null)
            {
                ui.ShowFeedback(isOpen ? "Door opened." : "Door closed.");
            }
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openAngle : 0f;

        ApplyDoorNodeState();

        Debug.Log($"{gameObject.name}: Door is now {(isOpen ? "OPEN" : "CLOSED")}");
    }

    private void ApplyDoorNodeState()
    {
        if (doorNodeBlocker == null)
        {
            Debug.LogWarning($"{gameObject.name}: DoorNodeBlocker is not assigned. Door will not affect A* graph.");
            return;
        }

        if (isOpen)
        {
            doorNodeBlocker.MarkOpen();
        }
        else
        {
            doorNodeBlocker.MarkClosed();
        }
    }

    private Vector3 CalculateHingeWorldPoint()
    {
        Bounds localBounds;
        bool foundBounds = TryGetLocalRendererBounds(out localBounds);

        if (!foundBounds)
        {
            foundBounds = TryGetLocalColliderBounds(out localBounds);
        }

        if (!foundBounds)
        {
            Debug.LogWarning($"{gameObject.name}: Could not calculate hinge automatically. Using object position as hinge.");
            return transform.position;
        }

        Vector3 localHingePoint = localBounds.center;

        if (doorWidthAxis == DoorWidthAxis.LocalX)
        {
            localHingePoint.x = hingeSide == HingeSide.NegativeSide
                ? localBounds.min.x
                : localBounds.max.x;
        }
        else
        {
            localHingePoint.z = hingeSide == HingeSide.NegativeSide
                ? localBounds.min.z
                : localBounds.max.z;
        }

        return transform.TransformPoint(localHingePoint);
    }

    private bool TryGetLocalRendererBounds(out Bounds localBounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        localBounds = new Bounds();
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            Bounds worldBounds = renderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (Vector3 corner in corners)
            {
                Vector3 localCorner = transform.InverseTransformPoint(corner);

                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        return hasBounds;
    }

    private bool TryGetLocalColliderBounds(out Bounds localBounds)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        localBounds = new Bounds();
        bool hasBounds = false;

        foreach (Collider collider in colliders)
        {
            if (collider == null) continue;

            Bounds worldBounds = collider.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (Vector3 corner in corners)
            {
                Vector3 localCorner = transform.InverseTransformPoint(corner);

                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        return hasBounds;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 hingePoint;

        if (Application.isPlaying)
        {
            hingePoint = hingeWorldPoint;
        }
        else
        {
            if (autoCalculateHinge)
            {
                hingePoint = CalculateHingeWorldPoint();
            }
            else
            {
                hingePoint = transform.TransformPoint(manualLocalHingePoint);
            }
        }

        Gizmos.DrawSphere(hingePoint, 0.12f);
        Gizmos.DrawLine(hingePoint, hingePoint + transform.up * 2f);
    }
}