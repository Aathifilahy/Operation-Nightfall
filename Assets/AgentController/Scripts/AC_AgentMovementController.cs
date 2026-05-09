using UnityEngine;

public class AC_AgentMovementController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;
    public float stoppingDistance = 0.2f;
    public float waitTimeAtPoint = 0.5f;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Update()
    {
        PatrolMovement();
    }

    void PatrolMovement()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Transform targetPoint = patrolPoints[currentPointIndex];

        Vector3 targetPosition = new Vector3(
            targetPoint.position.x,
            transform.position.y,
            targetPoint.position.z
        );

        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            HandleWaitingAtPoint();
            return;
        }

        MoveTowardsTarget(direction);
        RotateTowardsTarget(direction);
    }

    void MoveTowardsTarget(Vector3 direction)
    {
        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void RotateTowardsTarget(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void HandleWaitingAtPoint()
    {
        if (!isWaiting)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
        }

        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            isWaiting = false;
            currentPointIndex++;

            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
            }
        }
    }
}