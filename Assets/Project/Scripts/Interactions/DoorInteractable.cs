using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool isOpen = false;

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

    public void Interact()
    {
        isOpen = !isOpen;
        Debug.Log("Door toggled: " + gameObject.name);
    }
}