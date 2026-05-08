using UnityEngine;

public class SE_ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float followSmoothness = 10f;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}