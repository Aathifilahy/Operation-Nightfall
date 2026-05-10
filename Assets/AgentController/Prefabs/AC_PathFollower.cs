using System.Collections.Generic;
using UnityEngine;

public class AC_PathFollower_ZoneA : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> pathPoints;    // Assign from BFS output or patrol points
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    private int currentTargetIndex = 0;

    void Update()
    {
        if (pathPoints == null || pathPoints.Count == 0) return;

        Transform targetPoint = pathPoints[currentTargetIndex];

        // Move towards target
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate smoothly towards target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if reached current target
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= pathPoints.Count)
            {
                currentTargetIndex = 0; // Loop patrol
            }
        }
    }
}