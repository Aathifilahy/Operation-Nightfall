using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;

    [Header("Movement Speed Values")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float animationSmoothTime = 0.1f;

    [Header("Debug")]
    public float currentMoveSpeed;
    public float animatorSpeedValue;

    private Vector3 lastPosition;

    private void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (playerAnimator == null)
            return;

        Vector3 currentPosition = transform.position;

        Vector3 movementDelta = currentPosition - lastPosition;
        movementDelta.y = 0f;

        currentMoveSpeed = movementDelta.magnitude / Time.deltaTime;

        if (currentMoveSpeed < 0.1f)
        {
            animatorSpeedValue = 0f;
        }
        else if (currentMoveSpeed < runSpeed - 1f)
        {
            animatorSpeedValue = 0.5f;
        }
        else
        {
            animatorSpeedValue = 1f;
        }

        playerAnimator.SetFloat("Speed", animatorSpeedValue, animationSmoothTime, Time.deltaTime);

        lastPosition = currentPosition;
    }
}