using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    public Transform[] targets;   // NPC'nin gideceği hedefler
    private NavMeshAgent agent;
    private int currentIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targets.Length > 0)
        {
            agent.SetDestination(targets[currentIndex].position);
        }
    }

    void Update()
    {
        if (targets.Length == 0) return;

        // Hedefe yaklaşınca bir sonraki hedefe geç
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentIndex = (currentIndex + 1) % targets.Length;
            agent.SetDestination(targets[currentIndex].position);
        }
    }
}
