using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public float damageAmount = 25f;
    public float damageCooldown = 1f;

    private float nextDamageTime = 0f;

    void OnTriggerStay(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && Time.time >= nextDamageTime)
        {
            health.TakeDamage(damageAmount);
            nextDamageTime = Time.time + damageCooldown;

            Debug.Log("Damage zone damaged player.");
        }
    }
}