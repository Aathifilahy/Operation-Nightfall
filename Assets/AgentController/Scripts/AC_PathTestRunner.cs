using System.Collections.Generic;
using UnityEngine;

public class AC_PathTestRunner : MonoBehaviour
{
    public AC_AgentMovementController agentMovement;

    void Start()
    {
        List<Vector3> testPath = new List<Vector3>
        {
            new Vector3(-3f, 1f, -3f),
            new Vector3(3f, 1f, -3f),
            new Vector3(3f, 1f, 3f),
            new Vector3(-3f, 1f, 3f)
        };

        if (agentMovement != null)
        {
            agentMovement.SetPath(testPath);
        }
        else
        {
            Debug.LogWarning("Agent Movement is not assigned in AC_PathTestRunner");
        }
    }
}