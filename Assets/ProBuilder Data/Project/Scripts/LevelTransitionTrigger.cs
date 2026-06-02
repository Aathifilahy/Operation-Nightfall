using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    [Header("Scene Transition")]
    public string sceneToLoad = "Zone_B_Environment";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered transition trigger: " + other.gameObject.name);

        if (isLoading) return;

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Entered object is not tagged Player.");
            return;
        }

        Debug.Log("Player entered trigger. Loading scene: " + sceneToLoad);

        isLoading = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}