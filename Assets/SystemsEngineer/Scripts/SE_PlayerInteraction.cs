using UnityEngine;
using TMPro;

public class SE_PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2.5f;
    public LayerMask interactableLayer;

    [Header("UI")]
    public GameObject interactionPromptObject;
    public TextMeshProUGUI interactionPromptText;

    private GameObject currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed near interactable object: " + currentInteractable.name);
        }
    }

    void CheckForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRange,
            interactableLayer
        );

        if (hits.Length > 0)
        {
            currentInteractable = hits[0].gameObject;
            ShowPrompt("Press E to interact");
        }
        else
        {
            currentInteractable = null;
            HidePrompt();
        }
    }

    void ShowPrompt(string message)
    {
        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(true);
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.text = message;
        }
    }

    void HidePrompt()
    {
        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}