using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    public Camera playerCamera;
    public float shootRange = 100f;
    public float damage = 25f;
    public float fireRate = 0.25f;

    [Header("Effects")]
    public LayerMask shootableLayers;

    private float nextFireTime;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, shootableLayers))
        {
            Debug.Log("Shot hit: " + hit.collider.name);

            Target target = hit.collider.GetComponent<Target>();

            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("Shot missed");
        }
    }
}