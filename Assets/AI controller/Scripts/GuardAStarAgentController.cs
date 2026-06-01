using System.Collections.Generic;
using UnityEngine;

public class GuardAStarAgentController : MonoBehaviour
{
    public enum GuardState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    public AStarPathfinder pathfinder;
    public Transform player;
    public PlayerHealth playerHealth;
    public Transform[] patrolPoints;

    [Tooltip("Optional. If assigned, movement will use CharacterController.Move(). If empty, transform movement is used.")]
    public CharacterController characterController;

    [Tooltip("Optional. Only needed if the guard has animations.")]
    public Animator animator;

    [Header("Current State")]
    public GuardState currentState = GuardState.Patrol;
    public bool logStateChanges = true;

    [Header("Movement Settings")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 4.5f;
    public float rotationSpeed = 8f;
    public float waypointReachDistance = 0.3f;
    public float patrolPointReachDistance = 1.5f;
    public float waitTimeAtPatrolPoint = 0.5f;

    [Tooltip("Recommended false if patrol points are not exactly on graph nodes.")]
    public bool appendExactPatrolPointToPath = false;

    [Tooltip("Recommended false. A* takes the guard to the graph node near the player, then Attack mode handles close movement.")]
    public bool appendExactChaseTargetToPath = false;

    [Header("Detection Settings")]
    public float detectionRange = 12f;
    public float lostRange = 18f;

    [Tooltip("If true, guard returns to patrol when player escapes beyond Lost Range. If false, guard keeps chasing after detecting player once.")]
    public bool returnToPatrolWhenPlayerEscapes = false;

    [Header("Attack Settings")]
    [Tooltip("Distance where guard stops using A* and starts local close-range approach.")]
    public float attackRange = 4f;

    [Tooltip("Guard leaves Attack only after player goes beyond this range. Must be greater than Attack Range.")]
    public float attackExitRange = 5f;

    [Tooltip("Actual distance required to damage player.")]
    public float damageRange = 1.4f;

    public float attackCooldown = 1.2f;
    public float damageAmount = 10f;

    [Header("Path Recalculation")]
    public float chaseRepathInterval = 0.5f;

    [Header("Optional Animation Parameters")]
    public bool useAnimator = false;
    public string speedParameter = "Speed";
    public string chasingParameter = "IsChasing";
    public string attackingParameter = "IsAttacking";
    public string attackTriggerParameter = "Attack";

    [Header("Animation Debug")]
    public float currentAnimatorSpeedValue;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaitingAtPatrolPoint = false;

    private float chaseRepathTimer = 0f;
    private float nextAttackTime = 0f;

    private GuardState previousState;

    private void OnEnable()
    {
        DoorwayBlockZone.OnGraphBlockStateChanged += HandleGraphChanged;
        DoorNodeBlocker.OnDoorGraphStateChanged += HandleGraphChanged;
    }

    private void OnDisable()
    {
        DoorwayBlockZone.OnGraphBlockStateChanged -= HandleGraphChanged;
        DoorNodeBlocker.OnDoorGraphStateChanged -= HandleGraphChanged;
    }

    private void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (attackExitRange <= attackRange)
        {
            attackExitRange = attackRange + 1f;
        }

        previousState = currentState;

        ChangeState(GuardState.Patrol, true);
    }

    private void Update()
    {
        if (pathfinder == null)
        {
            Debug.LogWarning($"{gameObject.name}: AStarPathfinder is not assigned.");
            UpdateAnimator(0f);
            return;
        }

        float distanceToPlayer = Mathf.Infinity;

        if (player != null)
        {
            distanceToPlayer = FlatDistance(transform.position, player.position);
        }

        switch (currentState)
        {
            case GuardState.Patrol:
                if (player != null && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GuardState.Chase);
                }
                else
                {
                    UpdatePatrol();
                }
                break;

            case GuardState.Chase:
                if (player == null)
                {
                    ChangeState(GuardState.Patrol);
                    return;
                }

                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(GuardState.Attack);
                }
                else if (returnToPatrolWhenPlayerEscapes && distanceToPlayer > lostRange)
                {
                    ChangeState(GuardState.Patrol);
                }
                else
                {
                    UpdateChase();
                }
                break;

            case GuardState.Attack:
                if (player == null)
                {
                    ChangeState(GuardState.Patrol);
                    return;
                }

                if (distanceToPlayer > attackExitRange)
                {
                    ChangeState(GuardState.Chase);
                }
                else
                {
                    UpdateAttack(distanceToPlayer);
                }
                break;
        }
    }

    private void ChangeState(GuardState newState, bool force = false)
    {
        if (!force && currentState == newState)
            return;

        currentState = newState;
        isWaitingAtPatrolPoint = false;
        waitTimer = 0f;

        if (logStateChanges && (force || currentState != previousState))
        {
            Debug.Log($"{gameObject.name}: State changed to {currentState}");
        }

        previousState = currentState;

        switch (currentState)
        {
            case GuardState.Patrol:
                ClearCurrentPath();
                CalculatePathToCurrentPatrolPoint();
                break;

            case GuardState.Chase:
                ClearCurrentPath();
                chaseRepathTimer = 0f;
                RecalculatePathToPlayer();
                break;

            case GuardState.Attack:
                ClearCurrentPath();
                UpdateAnimator(0f);
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            UpdateAnimator(0f);
            return;
        }

        if (isWaitingAtPatrolPoint)
        {
            UpdateAnimator(0f);

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaitingAtPatrolPoint = false;
                MoveToNextPatrolPoint();
            }

            return;
        }

        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            CalculatePathToCurrentPatrolPoint();
        }

        bool pathFinished = FollowCurrentPath(patrolSpeed);

        if (pathFinished || HasReachedCurrentPatrolPoint())
        {
            BeginPatrolWait();
        }
    }

    private void UpdateChase()
    {
        chaseRepathTimer -= Time.deltaTime;

        if (chaseRepathTimer <= 0f || currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            RecalculatePathToPlayer();
        }

        FollowCurrentPath(chaseSpeed);
    }

    private void UpdateAttack(float distanceToPlayer)
    {
        ClearCurrentPath();

        if (player == null)
        {
            UpdateAnimator(0f);
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        FaceTarget(player.position);

        // If guard is in attack state but still not close enough to damage,
        // it runs closer to the player.
        if (distanceToPlayer > damageRange)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                Vector3 moveDirection = direction.normalized;
                Vector3 movement = moveDirection * chaseSpeed * Time.deltaTime;

                MoveGuard(movement);
                RotateTowards(moveDirection);
                UpdateAnimator(chaseSpeed);
            }

            return;
        }

        // Close enough to actually attack.
        UpdateAnimator(0f);

        if (Time.time >= nextAttackTime)
        {
            TriggerAttackAnimation();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"{gameObject.name}: Attacked player for {damageAmount} damage.");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: PlayerHealth is not assigned.");
            }

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void CalculatePathToCurrentPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: Patrol point {currentPatrolIndex} is missing.");
            return;
        }

        CalculatePathTo(targetPoint.position, appendExactPatrolPointToPath);
    }

    private void RecalculatePathToPlayer()
    {
        if (player == null)
            return;

        chaseRepathTimer = chaseRepathInterval;
        CalculatePathTo(player.position, appendExactChaseTargetToPath);
    }

    private bool CalculatePathTo(Vector3 destination, bool appendExactDestination)
    {
        if (pathfinder == null)
            return false;

        List<Vector3> newPath = pathfinder.FindPath(transform.position, destination);

        if (newPath == null || newPath.Count == 0)
        {
            ClearCurrentPath();
            Debug.LogWarning($"{gameObject.name}: No A* path found to {destination}");
            return false;
        }

        currentPath = new List<Vector3>(newPath);

        if (appendExactDestination)
        {
            Vector3 lastPoint = currentPath[currentPath.Count - 1];

            if (FlatDistance(lastPoint, destination) > waypointReachDistance)
            {
                currentPath.Add(destination);
            }
        }

        currentPathIndex = 0;
        SkipAlreadyReachedWaypoints();

        return true;
    }

    private bool FollowCurrentPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            UpdateAnimator(0f);
            return true;
        }

        SkipAlreadyReachedWaypoints();

        if (currentPathIndex >= currentPath.Count)
        {
            UpdateAnimator(0f);
            return true;
        }

        Vector3 targetPosition = currentPath[currentPathIndex];
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= waypointReachDistance)
        {
            currentPathIndex++;

            if (currentPathIndex >= currentPath.Count)
            {
                UpdateAnimator(0f);
                return true;
            }

            return false;
        }

        Vector3 moveDirection = direction.normalized;
        Vector3 movement = moveDirection * speed * Time.deltaTime;

        if (movement.magnitude > distance)
        {
            movement = moveDirection * distance;
        }

        MoveGuard(movement);
        RotateTowards(moveDirection);
        UpdateAnimator(speed);

        return false;
    }

    private void MoveGuard(Vector3 movement)
    {
        if (characterController != null && characterController.enabled)
        {
            characterController.Move(movement);
        }
        else
        {
            transform.position += movement;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        RotateTowards(direction);
    }

    private void SkipAlreadyReachedWaypoints()
    {
        while (currentPath != null &&
               currentPathIndex < currentPath.Count &&
               FlatDistance(transform.position, currentPath[currentPathIndex]) <= waypointReachDistance)
        {
            currentPathIndex++;
        }
    }

    private bool HasReachedCurrentPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return false;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
            return false;

        return FlatDistance(transform.position, targetPoint.position) <= patrolPointReachDistance;
    }

    private void BeginPatrolWait()
    {
        ClearCurrentPath();

        isWaitingAtPatrolPoint = true;
        waitTimer = waitTimeAtPatrolPoint;

        UpdateAnimator(0f);
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        CalculatePathToCurrentPatrolPoint();
    }

    private void HandleGraphChanged()
    {
        if (pathfinder == null)
            return;

        if (currentState == GuardState.Patrol)
        {
            CalculatePathToCurrentPatrolPoint();
        }
        else if (currentState == GuardState.Chase)
        {
            RecalculatePathToPlayer();
        }
    }

    private void ClearCurrentPath()
    {
        currentPath.Clear();
        currentPathIndex = 0;
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void UpdateAnimator(float movementSpeed)
    {
        if (!useAnimator || animator == null)
            return;

        float animatorSpeed = 0f;

        if (movementSpeed <= 0.1f)
        {
            animatorSpeed = 0f; // Idle
        }
        else if (currentState == GuardState.Patrol)
        {
            animatorSpeed = 0.5f; // Walk
        }
        else
        {
            animatorSpeed = 1f; // Run for chase and close attack approach
        }

        currentAnimatorSpeedValue = animatorSpeed;

        SetAnimatorFloat(speedParameter, animatorSpeed);
        SetAnimatorBool(chasingParameter, currentState == GuardState.Chase);
        SetAnimatorBool(attackingParameter, currentState == GuardState.Attack);
    }

    private void TriggerAttackAnimation()
    {
        if (!useAnimator || animator == null)
            return;

        SetAnimatorTrigger(attackTriggerParameter);
    }

    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (string.IsNullOrEmpty(parameterName))
            return;

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parameterName, value);
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (string.IsNullOrEmpty(parameterName))
            return;

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetAnimatorTrigger(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
            return;

        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(parameterName);
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, detectionRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(center, lostRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(center, attackExitRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, damageRange);

        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawSphere(currentPath[i], 0.12f);
            }

            Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 0.12f);
        }
    }
}