using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    public Camera playerCamera;
    public float shootRange = 100f;
    public float damage = 25f;
    public float fireRate = 0.25f;
    public LayerMask shootableLayers;

    [Header("Ammo")]
    public int maxAmmo = 6;
    public float reloadTime = 3f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public InteractionUI interactionUI;

    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading = false;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();

                currentAmmo--;
                nextFireTime = Time.time + fireRate;
                UpdateAmmoUI();

                if (currentAmmo <= 0)
                {
                    StartCoroutine(Reload());
                }
            }
            else
            {
                StartCoroutine(Reload());
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, shootableLayers))
        {
            Debug.Log("Shot hit: " + hit.collider.name);

            // 1. Check if we hit a guard
            GuardHealth guardHealth = hit.collider.GetComponentInParent<GuardHealth>();

            if (guardHealth != null)
            {
                guardHealth.TakeDamage(damage);
                return;
            }

            // 2. Check if we hit the old test target
            Target target = hit.collider.GetComponentInParent<Target>();

            if (target != null)
            {
                target.TakeDamage(damage);
                return;
            }

            Debug.Log("Hit object has no GuardHealth or Target script.");
        }
        else
        {
            Debug.Log("Shot missed");
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;

        if (interactionUI != null)
        {
            interactionUI.ShowFeedback("Reloading...");
        }

        UpdateAmmoUI("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;

        UpdateAmmoUI();

        if (interactionUI != null)
        {
            interactionUI.ShowFeedback("Reload complete.");
        }
    }

    void UpdateAmmoUI(string overrideText = "")
    {
        if (ammoText == null) return;

        if (!string.IsNullOrEmpty(overrideText))
        {
            ammoText.text = overrideText;
        }
        else
        {
            ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
        }
    }
}