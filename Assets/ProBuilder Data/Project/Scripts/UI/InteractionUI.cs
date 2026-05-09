using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    public TextMeshProUGUI interactionText;
    public TextMeshProUGUI feedbackText;

    private Coroutine feedbackCoroutine;

    public void ShowText(string message)
    {
        interactionText.text = message;
    }

    public void ClearText()
    {
        interactionText.text = "";
    }

    public void ShowFeedback(string message, float duration = 2f)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message, duration));
    }

    private IEnumerator FeedbackRoutine(string message, float duration)
    {
        feedbackText.text = message;
        yield return new WaitForSeconds(duration);
        feedbackText.text = "";
    }
}