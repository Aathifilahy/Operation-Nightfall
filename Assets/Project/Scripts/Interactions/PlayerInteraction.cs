using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 4f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed");
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
            Debug.Log("Interacting with door: " + nearestDoor.name);
            nearestDoor.Interact();
        }
        else
        {
            Debug.Log("No door nearby.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}