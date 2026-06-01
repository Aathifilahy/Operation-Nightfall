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

    [Header("Muzzle Flash / Gun VFX")]
    public ParticleSystem muzzleFlash;
    public Light muzzleFlashLight;
    public float flashLightDuration = 0.05f;

    [Header("Ammo")]
    public int maxAmmo = 6;
    public float reloadTime = 3f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public InteractionUI interactionUI;

    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading = false;

    private Coroutine muzzleLightCoroutine;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        // Keep muzzle light OFF at the start.
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }

        // Make sure particle does not auto-play.
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
        }
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
        PlayShootEffects();

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

    void PlayShootEffects()
    {
        // Particle muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        // Short light flash
        if (muzzleFlashLight != null)
        {
            if (muzzleLightCoroutine != null)
            {
                StopCoroutine(muzzleLightCoroutine);
            }

            muzzleLightCoroutine = StartCoroutine(MuzzleLightFlash());
        }
    }

    IEnumerator MuzzleLightFlash()
    {
        muzzleFlashLight.enabled = true;

        yield return new WaitForSeconds(flashLightDuration);

        muzzleFlashLight.enabled = false;
        muzzleLightCoroutine = null;
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