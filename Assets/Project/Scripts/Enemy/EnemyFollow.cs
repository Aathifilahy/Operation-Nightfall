using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFollow : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing on " + gameObject.name);
            return;
        }

        // Find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found. Make sure your player capsule has the tag 'Player'.");
            return;
        }
        player = playerObj.transform;
    }

    void Update()
    {
        if (agent == null || player == null) return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " is not on NavMesh. Check its position.");
        }
    }
}