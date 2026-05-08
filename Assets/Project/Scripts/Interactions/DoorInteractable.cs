using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool isOpen = false;

    public bool requiresKeycard = true;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Update()
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
                isOpen = !isOpen;

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
            isOpen = !isOpen;

            if (ui != null)
            {
                ui.ShowFeedback(isOpen ? "Door opened." : "Door closed.");
            }
        }
    }
}