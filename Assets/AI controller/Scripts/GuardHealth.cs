using UnityEngine;

public class GuardHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Death Animation")]
    public string deathStateName = "Death";
    public float deathTransitionTime = 0.05f;

    [Header("Death Settings")]
    public bool destroyOnDeath = false; // keep false while testing
    public float destroyDelay = 4f;

    private GuardAStarAgentController guardController;
    private CharacterController characterController;
    private Animator animator;
    private Collider[] colliders;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        guardController = GetComponent<GuardAStarAgentController>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider>();

        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Animator found for guard death animation.");
        }
        else
        {
            Debug.Log($"{gameObject.name}: GuardHealth found Animator: {animator.name}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name} died. Forcing death animation.");

        // Stop guard AI first so it cannot keep setting Speed = 0 idle.
        if (guardController != null)
        {
            guardController.enabled = false;
        }

        // Stop movement.
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Force death animation directly.
        if (animator != null)
        {
            animator.applyRootMotion = false;

            SafeSetFloat("Speed", 0f);
            SafeSetBool("IsChasing", false);
            SafeSetBool("IsAttacking", false);

            animator.ResetTrigger("Attack");

            // This does NOT depend on Any State → Death transition.
            animator.CrossFade(deathStateName, deathTransitionTime, 0, 0f);

            Debug.Log($"{gameObject.name}: CrossFade to death state: {deathStateName}");
        }

        // Disable colliders so dead guard cannot be shot again.
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void SafeSetFloat(string parameterName, float value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parameterName, value);
        }
    }

    private void SafeSetBool(string parameterName, bool value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parameterName, value);
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
}