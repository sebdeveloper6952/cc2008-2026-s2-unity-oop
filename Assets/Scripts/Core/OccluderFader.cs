using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// When a wall (tag "Occluder") hides the engineer, it is hidden too (its shadow stays).
public class OccluderFader : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float radius = 0.9f;

    readonly HashSet<Renderer> hidden = new HashSet<Renderer>();
    readonly HashSet<Renderer> current = new HashSet<Renderer>();

    void LateUpdate()
    {
        current.Clear();
        Vector3 to = target.position + Vector3.up;
        Vector3 dir = transform.forward;
        Vector3 from = to - dir * 60f;
        foreach (RaycastHit hit in Physics.SphereCastAll(from, radius, dir, 59f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("Occluder")) continue;
            foreach (Renderer r in hit.collider.GetComponentsInChildren<Renderer>()) current.Add(r);
        }

        foreach (Renderer r in hidden)
        {
            if (r != null && !current.Contains(r)) r.shadowCastingMode = ShadowCastingMode.On;
        }
        foreach (Renderer r in current)
        {
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
        hidden.Clear();
        hidden.UnionWith(current);
    }
}
