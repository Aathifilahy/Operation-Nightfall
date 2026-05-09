using UnityEngine;

public class NodeDebug : MonoBehaviour
{
    public GraphLoader loader;

    void Start()
    {
        Debug.Log($"Loaded nodes count: {loader.Nodes.Count}");
    }
}