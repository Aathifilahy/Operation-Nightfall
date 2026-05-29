using UnityEngine;

public class NodeDebug : MonoBehaviour
{
    public GraphLoader loader;

    void Start()
    {
        if (loader == null)
        {
            loader = FindFirstObjectByType<GraphLoader>();
        }

        if (loader == null)
        {
            Debug.LogError("NodeDebug: GraphLoader is not assigned and could not be found in the scene.");
            return;
        }

        if (loader.Nodes == null)
        {
            Debug.LogError("NodeDebug: GraphLoader exists, but Nodes list is null.");
            return;
        }

        Debug.Log($"Loaded nodes count: {loader.Nodes.Count}");
    }
}