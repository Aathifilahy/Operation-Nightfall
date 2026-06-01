using UnityEngine;

[System.Serializable]
public class Node
{
    public int id;               // Unique number for this node
    public Vector3 worldPosition; // The actual 3D position of the triangle centre

    public Node(int id, Vector3 worldPosition)
    {
        this.id = id;
        this.worldPosition = worldPosition;
    }
}