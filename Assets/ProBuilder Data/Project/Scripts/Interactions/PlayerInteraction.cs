using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 4f;

    [Header("UI")]
    public InteractionUI interactionUI;

    void Update()
    {
        ShowNearbyPrompt();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

        DoorInteractable nearestDoor = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            DoorInteractable door = hit.GetComponentInParent<DoorInteractable>();

            if (door != null)
            {
                float distance = Vector3.Distance(transform.position, door.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestDoor = door;
                }
            }
        }

        if (nearestDoor != null)
        {
            nearestDoor.Interact(gameObject);
        }
        else
        {
            Debug.Log("No door nearby.");
        }
    }

    void ShowNearbyPrompt()
{
    if (interactionUI == null) return;

    Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

    foreach (Collider hit in hits)
    {
        // Door
        DoorInteractable door = hit.GetComponentInParent<DoorInteractable>();
        if (door != null)
        {
            interactionUI.ShowText("Press E to open door");
            return;
        }

        // Keycard
        KeycardPickup keycard = hit.GetComponentInParent<KeycardPickup>();
        if (keycard != null)
        {
            interactionUI.ShowText("Press E to pick up keycard");
            return;
        }

        // Physics Object
        PhysicsObject phys = hit.GetComponentInParent<PhysicsObject>();
        if (phys != null)
        {
            interactionUI.ShowText("Press T to pick up object");
            return;
        }
    }

    interactionUI.ClearText();
}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}