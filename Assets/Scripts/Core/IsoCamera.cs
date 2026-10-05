using UnityEngine;

// Isometric camera: orthographic, tilted 30 degrees down and turned 45. Follows the engineer.
[RequireComponent(typeof(Camera))]
public class IsoCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float size = 8.5f;
    [SerializeField] float pitch = 30f;
    [SerializeField] float yaw = 45f;
    [SerializeField] float distance = 40f;
    [SerializeField] float smoothing = 6f;
    [SerializeField] Vector3 offset = new Vector3(0f, 1f, 0f);

    void Awake()
    {
        var cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = size;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Start()
    {
        transform.position = Goal();
    }

    void LateUpdate()
    {
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, Goal(), t);
    }

    Vector3 Goal() => target.position + offset - transform.forward * distance;
}
