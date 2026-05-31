using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    [Header("Door Movement")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool isOpen = false;

    [Header("Access")]
    public bool requiresKeycard = true;

    [Header("A* Node Blocking")]
    public DoorNodeBlocker doorNodeBlocker;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));

        if (doorNodeBlocker == null)
        {
            doorNodeBlocker = GetComponent<DoorNodeBlocker>();
        }

        ApplyDoorNodeState();
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
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
}