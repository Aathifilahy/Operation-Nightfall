using System.Collections.Generic;
using UnityEngine;

public class AC_AgentMovementController : MonoBehaviour
{
    public enum AgentState
    {
        Patrol,
        Chase,
        Attack,
        FollowPath
    }

    [Header("Current State")]
    public AgentState currentState = AgentState.Patrol;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    public float rotationSpeed = 8f;
    public float stoppingDistance = 0.2f;
    public float waitTimeAtPoint = 0.5f;

    [Header("Chase Settings")]
    public Transform playerTarget;
    public float chaseSpeed = 5f;
    public float chaseRange = 6f;
    public float attackRange = 1.5f;

    [Header("Path Following Settings")]
    public float pathMoveSpeed = 4f;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private bool hasPath = false;

    private AgentState previousState;

    void Start()
    {
        previousState = currentState;
        Debug.Log("Agent initial state: " + currentState);
    }

    void Update()
    {
        UpdateState();

        if (currentState != previousState)
        {
            Debug.Log("Agent state changed to: " + currentState);
            previousState = currentState;
        }

        switch (currentState)
        {
            case AgentState.Patrol:
                PatrolMovement();
                break;

            case AgentState.Chase:
                ChaseTarget();
                break;

            case AgentState.Attack:
                AttackTarget();
                break;

            case AgentState.FollowPath:
                FollowCalculatedPath();
                break;
        }
    }

    void UpdateState()
    {
        if (hasPath)
        {
            currentState = AgentState.FollowPath;
            return;
        }

        if (playerTarget == null)
        {
            currentState = AgentState.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = AgentState.Attack;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = AgentState.Chase;
        }
        else
        {
            currentState = AgentState.Patrol;
        }
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

        MoveTowardsTarget(direction, patrolSpeed);
        RotateTowardsTarget(direction);
    }

    void ChaseTarget()
    {
        if (playerTarget == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            playerTarget.position.x,
            transform.position.y,
            playerTarget.position.z
        );

        Vector3 direction = targetPosition - transform.position;

        MoveTowardsTarget(direction, chaseSpeed);
        RotateTowardsTarget(direction);
    }

    void AttackTarget()
    {
        if (playerTarget == null)
        {
            return;
        }

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        RotateTowardsTarget(direction);
    }

    void FollowCalculatedPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            hasPath = false;
            currentState = AgentState.Patrol;
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            hasPath = false;
            currentPath.Clear();
            currentPathIndex = 0;
            currentState = AgentState.Patrol;

            Debug.Log("Agent finished following calculated path");
            return;
        }

        Vector3 targetPosition = new Vector3(
            currentPath[currentPathIndex].x,
            transform.position.y,
            currentPath[currentPathIndex].z
        );

        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            currentPathIndex++;
            return;
        }

        MoveTowardsTarget(direction, pathMoveSpeed);
        RotateTowardsTarget(direction);
    }

    public void SetPath(List<Vector3> newPath)
    {
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("SetPath received an empty path");
            return;
        }

        currentPath = new List<Vector3>(newPath);
        currentPathIndex = 0;
        hasPath = true;
        currentState = AgentState.FollowPath;

        Debug.Log("Agent received new path with " + currentPath.Count + " points");
    }

    void MoveTowardsTarget(Vector3 direction, float speed)
    {
        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
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