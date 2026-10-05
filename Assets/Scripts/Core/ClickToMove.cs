using UnityEngine;
using UnityEngine.AI;

// Walks the engineer around the NavMesh (Unity's pathfinding).
// It never reads the mouse: other scripts tell it where to go.
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    NavMeshAgent agent;

    public float Speed01 => agent.speed > 0f ? agent.velocity.magnitude / agent.speed : 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (GameManager.Instance.IsOver && agent.hasPath) Stop();
    }

    // Walk to a point (or as close to it as the floor allows).
    public void MoveTo(Vector3 point)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Fx.Ping(hit.position);
        }
    }

    public void Stop()
    {
        if (agent.hasPath) agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void Face(Vector3 point)
    {
        Vector3 d = point - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
    }

    public void Warp(Vector3 position)
    {
        agent.Warp(position);
    }
}
