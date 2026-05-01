using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    public float pickupRange = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (Collider hit in hits)
        {
            PlayerInventory player = hit.GetComponent<PlayerInventory>();

            if (player != null)
            {
                player.hasKeycard = true;
                Debug.Log("Keycard picked up!");

                Destroy(gameObject);
                return;
            }
        }
    }
}