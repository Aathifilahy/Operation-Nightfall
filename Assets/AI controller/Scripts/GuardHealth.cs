using UnityEngine;

public class GuardHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Death Settings")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 0.2f;

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

        Debug.Log($"{gameObject.name} died.");

        // Stop AI movement/logic
        if (guardController != null)
        {
            guardController.enabled = false;
        }

        // Stop CharacterController movement
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Stop animation for now
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsAttacking", false);
        }

        // Disable all colliders so player cannot keep shooting dead guard
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
}
