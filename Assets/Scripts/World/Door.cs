using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Blast door: it sinks into the floor when it opens. It starts without power.
public class Door : MonoBehaviour
{
    [SerializeField] Transform panel;            // the part that moves
    [SerializeField] bool locked = true;
    [SerializeField] float travel = 3.3f;
    [SerializeField] float seconds = 0.8f;

    public bool IsOpen { get; private set; }
    public bool IsLocked => locked;

    public void Unlock()
    {
        locked = false;
    }

    public void Open()
    {
        if (locked)
        {
            Hud.Instance.Toast("The door has no power. Find a lever.");
            return;
        }
        if (IsOpen) return;
        IsOpen = true;
        foreach (NavMeshObstacle obstacle in GetComponentsInChildren<NavMeshObstacle>())
        {
            obstacle.enabled = false;                // the engineer can walk through now
        }
        foreach (Collider c in panel.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;                       // and nothing finds the sunken door any more
        }
        StartCoroutine(Slide(panel.localPosition + Vector3.down * travel));
    }

    // A coroutine: Unity advances it one step (one yield) per frame.
    IEnumerator Slide(Vector3 to)
    {
        Vector3 from = panel.localPosition;
        for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
        {
            panel.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        panel.localPosition = to;
    }
}
