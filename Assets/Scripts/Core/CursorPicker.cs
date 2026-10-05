using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// Reads the mouse once per frame: what is under the cursor, and what was clicked.
// Other scripts read CursorPicker.Hit, LeftClick, RightClick and RightHeld.
[DefaultExecutionOrder(-100)]
public class CursorPicker : MonoBehaviour
{
    public static bool HasHit { get; private set; }
    public static RaycastHit Hit { get; private set; }
    public static bool LeftClick { get; private set; }     // pressed this frame
    public static bool RightClick { get; private set; }    // pressed this frame
    public static bool RightHeld { get; private set; }     // held down

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        HasHit = false;
        LeftClick = false;
        RightClick = false;
        RightHeld = false;
        if (mouse == null || cam == null) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 300f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (IsFadedWall(hit.collider)) continue;      // walls hidden by OccluderFader don't block clicks
            Hit = hit;
            HasHit = true;
            break;
        }

        LeftClick = mouse.leftButton.wasPressedThisFrame;
        RightClick = mouse.rightButton.wasPressedThisFrame;
        RightHeld = mouse.rightButton.isPressed;
    }

    static bool IsFadedWall(Collider c)
    {
        if (!c.CompareTag("Occluder")) return false;
        var r = c.GetComponent<Renderer>();
        return r != null && r.shadowCastingMode == ShadowCastingMode.ShadowsOnly;
    }
}
