using System.Collections.Generic;
using UnityEngine;

// Placeholder for your graph data
public class CustomNavMeshGraph : MonoBehaviour
{
    [SerializeField]
    public List<NavMeshNode> Nodes = new List<NavMeshNode>();
}

[System.Serializable]  // <- THIS MAKES IT SHOW UP IN THE INSPECTOR
public class NavMeshNode
{
    public Vector3 Position;
    public bool Blocked;
    public List<int> Neighbors = new List<int>();
}