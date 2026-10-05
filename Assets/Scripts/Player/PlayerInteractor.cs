using UnityEngine;
using UnityEngine.InputSystem;

// Click to walk. Stand next to a door, lever or console and press E to use it.
//
// [!] This file works, but it is badly designed: every new kind of object forces you to
//     EDIT it in three places (the if-chains on tags). Ticket T0.
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] float useRange = 1.8f;

    Player player;
    ClickToMove movement;

    void Awake()
    {
        player = GetComponent<Player>();
        movement = GetComponent<ClickToMove>();
    }

    void Update()
    {
        if (GameManager.Instance.IsOver) return;

        // clicking only walks: to the floor, or up to whatever was clicked
        if (CursorPicker.LeftClick && CursorPicker.HasHit)
        {
            movement.MoveTo(CursorPicker.Hit.point);
        }

        Collider target = FindNearest();
        if (target == null)
        {
            Hud.Instance.HidePrompt();
            return;
        }
        Hud.Instance.ShowPrompt("[E] " + PromptFor(target), target.transform);

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            movement.Face(target.transform.position);
            Use(target);
        }
    }

    Collider FindNearest()
    {
        Collider best = null;
        float bestDistance = float.MaxValue;
        foreach (Collider c in Physics.OverlapSphere(transform.position, useRange))
        {
            if (!IsUsable(c)) continue;
            float d = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
            if (d < bestDistance)
            {
                best = c;
                bestDistance = d;
            }
        }
        return best;
    }

    // (1) which things can be used? a list of types...
    bool IsUsable(Collider c)
    {
        return c.CompareTag("Door") || c.CompareTag("Lever") || c.CompareTag("Terminal");
    }

    // (2) what does the label say? it depends on the type...
    string PromptFor(Collider c)
    {
        if (c.CompareTag("Door")) return "Open door";
        if (c.CompareTag("Lever")) return "Pull lever";
        if (c.CompareTag("Terminal")) return "Use console";
        return "";
    }

    // (3) what happens? it depends on the type...
    void Use(Collider c)
    {
        if (c.CompareTag("Door"))
        {
            c.GetComponentInParent<Door>().Open();
        }
        else if (c.CompareTag("Lever"))
        {
            c.GetComponent<Lever>().Pull();
        }
        else if (c.CompareTag("Terminal"))
        {
            c.GetComponent<Terminal>().Use(player);
        }
    }
}
