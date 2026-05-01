using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " took damage. Health: " + health);

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}