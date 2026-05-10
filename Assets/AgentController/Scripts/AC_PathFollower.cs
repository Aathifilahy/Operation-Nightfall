using System.Collections.Generic;
using UnityEngine;

public class AC_PathFollower : MonoBehaviour
{
    public List<Transform> PathNodes;      // The BFS path nodes
    public float MoveSpeed = 3f;           // Walking speed
    public float RotationSpeed = 5f;       // Smooth rotation
    private int currentIndex = 0;

    void Update()
    {
        if (PathNodes == null || PathNodes.Count == 0) return;

        Transform targetNode = PathNodes[currentIndex];
        Vector3 direction = (targetNode.position - transform.position).normalized;

        // Move towards the target node
        transform.position += direction * MoveSpeed * Time.deltaTime;

        // Rotate smoothly toward target node
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        // Check if reached the node
        if (Vector3.Distance(transform.position, targetNode.position) < 0.2f)
        {
            currentIndex++;
            if (currentIndex >= PathNodes.Count)
                currentIndex = 0; // Loop the path or stop if you want
        }
    }
}