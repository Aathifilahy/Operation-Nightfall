using System.Collections;
using TMPro;
using UnityEngine;

public class BombDefuseObjective : MonoBehaviour
{
    [Header("Defuse Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float defuseDuration = 3f;

    [Header("UI")]
    public TextMeshProUGUI objectiveMessageText;
    public GameObject missionCompletePanel;

    [Header("End Game")]
    public bool freezeGameOnComplete = true;
    public bool unlockCursorOnComplete = true;

    private bool playerNearby = false;
    private bool isDefusing = false;
    private bool missionCompleted = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(false);

        if (objectiveMessageText != null)
            objectiveMessageText.text = "";
    }

    private void Update()
    {
        if (missionCompleted) return;

        if (playerNearby && !isDefusing && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(DefuseRoutine());
        }
    }

    private IEnumerator DefuseRoutine()
    {
        isDefusing = true;

        if (objectiveMessageText != null)
            objectiveMessageText.text = "Defusing bomb...";

        yield return new WaitForSeconds(defuseDuration);

        missionCompleted = true;

        if (objectiveMessageText != null)
            objectiveMessageText.text = "Mission Complete";

        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(true);

        Debug.Log("Mission Complete: Bomb defused.");

        if (unlockCursorOnComplete)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (freezeGameOnComplete)
        {
            Time.timeScale = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (missionCompleted) return;

        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (objectiveMessageText != null && !isDefusing)
                objectiveMessageText.text = "Press E to defuse bomb";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (missionCompleted) return;

        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (objectiveMessageText != null && !isDefusing)
                objectiveMessageText.text = "";
        }
    }
}