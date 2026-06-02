using System.Collections.Generic;
using UnityEngine;

public class UCSGraphVisualizer : MonoBehaviour
{
    [Header("Graph Reference")]
    public CustomNavMeshGraph customGraph;

    [Header("Start and Goal")]
    public Transform startPoint;
    public Transform goalPoint;

    [Tooltip("Used only if Start Point is not assigned.")]
    public int startNodeIndex = 0;

    [Tooltip("Used only if Goal Point is not assigned.")]
    public int goalNodeIndex = 1;

    [Header("Controls")]
    public KeyCode toggleDebugKey = KeyCode.V;
    public KeyCode runUCSKey = KeyCode.U;

    [Header("Visual Settings")]
    public bool showDebugAtStart = false;
    public bool drawGraphEdges = true;
    public float nodeSize = 0.25f;
    public float graphLineWidth = 0.03f;
    public float pathLineWidth = 0.12f;
    public float heightOffset = 0.25f;

    private bool debugVisible;

    private GameObject debugRoot;
    private GameObject graphEdgeRoot;
    private GameObject searchResultRoot;

    private Material edgeMaterial;
    private Material visitedMaterial;
    private Material pathMaterial;
    private Material startMaterial;
    private Material goalMaterial;
    private Material blockedMaterial;

    private void Awake()
    {
        CreateMaterials();
        CreateDebugObjects();

        if (customGraph == null)
        {
            customGraph = FindFirstObjectByType<CustomNavMeshGraph>();
        }

        if (customGraph == null)
        {
            Debug.LogError("UCSGraphVisualizer: CustomNavMeshGraph not assigned or found.");
            return;
        }

        customGraph.RefreshNodeIndexes();

        debugVisible = showDebugAtStart;
        debugRoot.SetActive(debugVisible);

        if (drawGraphEdges)
        {
            DrawGraphEdges();
        }

        Debug.Log("UCSGraphVisualizer ready. Press V to toggle debug. Press U to run UCS.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            ToggleDebug();
        }

        if (Input.GetKeyDown(runUCSKey))
        {
            RunUCSAndVisualize();
        }
    }

    private void ToggleDebug()
    {
        debugVisible = !debugVisible;

        if (debugRoot != null)
        {
            debugRoot.SetActive(debugVisible);
        }

        Debug.Log(debugVisible ? "UCS Debug Mode ON" : "UCS Debug Mode OFF");
    }

    private void RunUCSAndVisualize()
    {
        if (customGraph == null || customGraph.Nodes == null || customGraph.Nodes.Count == 0)
        {
            Debug.LogWarning("UCSGraphVisualizer: Graph is empty or missing.");
            return;
        }

        customGraph.RefreshNodeIndexes();

        int startIndex = startPoint != null ? FindNearestWalkableNode(startPoint.position) : startNodeIndex;
        int goalIndex = goalPoint != null ? FindNearestWalkableNode(goalPoint.position) : goalNodeIndex;

        if (!IsValidNodeIndex(startIndex) || !IsValidNodeIndex(goalIndex))
        {
            Debug.LogWarning("UCSGraphVisualizer: Invalid start or goal node index.");
            return;
        }

        debugVisible = true;
        debugRoot.SetActive(true);

        ClearSearchResult();

        List<int> visitedOrder;
        List<int> finalPath = FindUCSPath(startIndex, goalIndex, out visitedOrder);

        DrawVisitedNodes(visitedOrder, startIndex, goalIndex);

        if (finalPath.Count > 0)
        {
            DrawFinalPathRed(finalPath);
            Debug.Log($"UCS path found. Start Node: {startIndex}, Goal Node: {goalIndex}, Path Nodes: {finalPath.Count}");
        }
        else
        {
            Debug.LogWarning($"UCS could not find a path. Start Node: {startIndex}, Goal Node: {goalIndex}");
        }

        Debug.Log($"UCS visited nodes: {visitedOrder.Count}");
    }

    private List<int> FindUCSPath(int startIndex, int goalIndex, out List<int> visitedOrder)
    {
        visitedOrder = new List<int>();

        Dictionary<int, float> costSoFar = new Dictionary<int, float>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();

        List<int> frontier = new List<int>();

        frontier.Add(startIndex);
        costSoFar[startIndex] = 0f;
        cameFrom[startIndex] = -1;

        while (frontier.Count > 0)
        {
            int currentIndex = GetLowestCostNode(frontier, costSoFar);
            frontier.Remove(currentIndex);

            if (!visitedOrder.Contains(currentIndex))
            {
                visitedOrder.Add(currentIndex);
            }

            if (currentIndex == goalIndex)
            {
                break;
            }

            CustomNavMeshGraph.NavMeshNode currentNode = customGraph.Nodes[currentIndex];

            if (currentNode.neighbors == null)
                continue;

            foreach (int neighbourIndex in currentNode.neighbors)
            {
                if (!IsValidNodeIndex(neighbourIndex))
                    continue;

                CustomNavMeshGraph.NavMeshNode neighbourNode = customGraph.Nodes[neighbourIndex];

                if (neighbourNode.Blocked)
                    continue;

                float edgeCost = Vector3.Distance(currentNode.position, neighbourNode.position);
                float newCost = costSoFar[currentIndex] + edgeCost;

                if (!costSoFar.ContainsKey(neighbourIndex) || newCost < costSoFar[neighbourIndex])
                {
                    costSoFar[neighbourIndex] = newCost;
                    cameFrom[neighbourIndex] = currentIndex;

                    if (!frontier.Contains(neighbourIndex))
                    {
                        frontier.Add(neighbourIndex);
                    }
                }
            }
        }

        return ReconstructPath(startIndex, goalIndex, cameFrom);
    }

    private int GetLowestCostNode(List<int> frontier, Dictionary<int, float> costSoFar)
    {
        int bestNode = frontier[0];
        float bestCost = costSoFar[bestNode];

        for (int i = 1; i < frontier.Count; i++)
        {
            int nodeIndex = frontier[i];
            float nodeCost = costSoFar[nodeIndex];

            if (nodeCost < bestCost)
            {
                bestNode = nodeIndex;
                bestCost = nodeCost;
            }
        }

        return bestNode;
    }

    private List<int> ReconstructPath(int startIndex, int goalIndex, Dictionary<int, int> cameFrom)
    {
        List<int> path = new List<int>();

        if (!cameFrom.ContainsKey(goalIndex))
        {
            return path;
        }

        int currentIndex = goalIndex;

        while (currentIndex != -1)
        {
            path.Add(currentIndex);
            currentIndex = cameFrom[currentIndex];
        }

        path.Reverse();

        if (path.Count == 0 || path[0] != startIndex)
        {
            path.Clear();
        }

        return path;
    }

    private int FindNearestWalkableNode(Vector3 worldPosition)
    {
        int nearestIndex = -1;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < customGraph.Nodes.Count; i++)
        {
            CustomNavMeshGraph.NavMeshNode node = customGraph.Nodes[i];

            if (node == null)
                continue;

            if (node.Blocked)
                continue;

            float distance = Vector3.Distance(worldPosition, node.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private bool IsValidNodeIndex(int index)
    {
        return customGraph != null &&
               customGraph.Nodes != null &&
               index >= 0 &&
               index < customGraph.Nodes.Count &&
               customGraph.Nodes[index] != null;
    }

    private void DrawGraphEdges()
    {
        if (customGraph == null || customGraph.Nodes == null)
            return;

        ClearChildren(graphEdgeRoot.transform);

        HashSet<string> drawnEdges = new HashSet<string>();

        for (int i = 0; i < customGraph.Nodes.Count; i++)
        {
            CustomNavMeshGraph.NavMeshNode currentNode = customGraph.Nodes[i];

            if (currentNode == null || currentNode.neighbors == null)
                continue;

            foreach (int neighbourIndex in currentNode.neighbors)
            {
                if (!IsValidNodeIndex(neighbourIndex))
                    continue;

                string edgeKey = i < neighbourIndex ? $"{i}_{neighbourIndex}" : $"{neighbourIndex}_{i}";

                if (drawnEdges.Contains(edgeKey))
                    continue;

                drawnEdges.Add(edgeKey);

                CustomNavMeshGraph.NavMeshNode neighbourNode = customGraph.Nodes[neighbourIndex];

                DrawLine(
                    graphEdgeRoot.transform,
                    currentNode.position + Vector3.up * heightOffset,
                    neighbourNode.position + Vector3.up * heightOffset,
                    edgeMaterial,
                    graphLineWidth,
                    $"GraphEdge_{i}_{neighbourIndex}"
                );
            }
        }
    }

    private void DrawVisitedNodes(List<int> visitedOrder, int startIndex, int goalIndex)
    {
        foreach (int nodeIndex in visitedOrder)
        {
            if (!IsValidNodeIndex(nodeIndex))
                continue;

            CustomNavMeshGraph.NavMeshNode node = customGraph.Nodes[nodeIndex];

            Material materialToUse = visitedMaterial;

            if (node.Blocked)
            {
                materialToUse = blockedMaterial;
            }

            if (nodeIndex == startIndex)
            {
                materialToUse = startMaterial;
            }

            if (nodeIndex == goalIndex)
            {
                materialToUse = goalMaterial;
            }

            CreateSphereMarker(
                searchResultRoot.transform,
                node.position + Vector3.up * heightOffset,
                nodeSize,
                materialToUse,
                $"VisitedNode_{nodeIndex}"
            );
        }
    }

    private void DrawFinalPathRed(List<int> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            int currentIndex = path[i];
            int nextIndex = path[i + 1];

            if (!IsValidNodeIndex(currentIndex) || !IsValidNodeIndex(nextIndex))
                continue;

            Vector3 start = customGraph.Nodes[currentIndex].position + Vector3.up * (heightOffset + 0.25f);
            Vector3 end = customGraph.Nodes[nextIndex].position + Vector3.up * (heightOffset + 0.25f);

            DrawLine(
                searchResultRoot.transform,
                start,
                end,
                pathMaterial,
                pathLineWidth,
                $"RED_UCS_Path_{currentIndex}_{nextIndex}"
            );
        }
    }

    private void CreateDebugObjects()
    {
        debugRoot = new GameObject("UCS_Debug_Visuals");
        graphEdgeRoot = new GameObject("Graph_Edges");
        searchResultRoot = new GameObject("UCS_Search_Result");

        graphEdgeRoot.transform.SetParent(debugRoot.transform);
        searchResultRoot.transform.SetParent(debugRoot.transform);
    }

    private void ClearSearchResult()
    {
        ClearChildren(searchResultRoot.transform);
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void CreateSphereMarker(Transform parent, Vector3 position, float size, Material material, string objectName)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = objectName;
        sphere.transform.SetParent(parent);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * size;

        Collider collider = sphere.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = sphere.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    private void DrawLine(Transform parent, Vector3 start, Vector3 end, Material material, float width, string objectName)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(parent);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.material = material;
        lineRenderer.widthMultiplier = width;
        lineRenderer.useWorldSpace = true;
    }

    private void CreateMaterials()
    {
        edgeMaterial = CreateMaterial(new Color(0.45f, 0.45f, 0.45f, 1f));
        visitedMaterial = CreateMaterial(new Color(1f, 0.55f, 0f, 1f));

        // Final UCS path line color: RED
        pathMaterial = CreateMaterial(Color.red);

        startMaterial = CreateMaterial(new Color(0f, 0.35f, 1f, 1f));
        goalMaterial = CreateMaterial(new Color(1f, 0f, 0f, 1f));
        blockedMaterial = CreateMaterial(new Color(0.05f, 0.05f, 0.05f, 1f));
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        material.color = color;

        return material;
    }
}