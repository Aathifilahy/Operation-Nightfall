using UnityEngine;

public class SE_DoorInteractable : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openAngle,
            transform.eulerAngles.z
        );
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            openSpeed * Time.deltaTime
        );
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            Debug.Log("Door opened: " + gameObject.name);
        }
        else
        {
            Debug.Log("Door closed: " + gameObject.name);
        }
    }
}