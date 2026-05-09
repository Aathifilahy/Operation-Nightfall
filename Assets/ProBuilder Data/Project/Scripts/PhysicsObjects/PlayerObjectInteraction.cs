using UnityEngine;

public class PlayerObjectInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform carryPoint;
    public InteractionUI interactionUI;

    [Header("Settings")]
    public float pickupRange = 3f;
    public float carryMoveSpeed = 12f;
    public float throwForce = 8f;

    private PhysicsObject heldObject;
    private DynamicBlockerEvent heldDynamicEvent; // <- reference to event script

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }

        if (Input.GetMouseButtonDown(0) && heldObject != null)
        {
            ThrowObject();
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            MoveHeldObject();
        }
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        PhysicsObject nearestObject = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            PhysicsObject physicsObject = hit.GetComponentInParent<PhysicsObject>();

            if (physicsObject != null)
            {
                float distance = Vector3.Distance(transform.position, physicsObject.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestObject = physicsObject;
                }
            }
        }

        if (nearestObject != null)
        {
            heldObject = nearestObject;
            heldDynamicEvent = heldObject.GetComponent<DynamicBlockerEvent>(); // <- cache event

            heldObject.Rigidbody.useGravity = false;
            heldObject.Rigidbody.velocity = Vector3.zero;
            heldObject.Rigidbody.angularVelocity = Vector3.zero;

            interactionUI?.ShowFeedback("Object picked up.");
            Debug.Log("Picked up: " + heldObject.name);
        }
        else
        {
            interactionUI?.ShowFeedback("No object nearby.");
            Debug.Log("No physics object nearby.");
        }
    }

    void MoveHeldObject()
    {
        if (carryPoint == null || heldObject == null) return;

        Vector3 targetPosition = carryPoint.position;
        Vector3 direction = targetPosition - heldObject.transform.position;

        heldObject.Rigidbody.velocity = direction * carryMoveSpeed;
    }

    void DropObject()
    {
        if (heldObject == null) return;

        heldObject.Rigidbody.useGravity = true;

        // Trigger dynamic blocker event if present
        heldDynamicEvent?.TriggerDrop();

        interactionUI?.ShowFeedback("Object dropped.");
        Debug.Log("Dropped: " + heldObject.name);

        heldObject = null;
        heldDynamicEvent = null;
    }

    void ThrowObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.Rigidbody;

        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Trigger dynamic blocker event if present
        heldDynamicEvent?.TriggerThrow();

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        interactionUI?.ShowFeedback("Object thrown.");
        Debug.Log("Object thrown.");

        heldObject = null;
        heldDynamicEvent = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}