using UnityEngine;

// A lever that gives power back to a door.
public class Lever : MonoBehaviour
{
    [SerializeField] Door door;
    [SerializeField] Light statusLight;          // red -> green

    bool pulled;

    public bool IsPulled => pulled;

    public void Pull()
    {
        if (pulled)
        {
            Hud.Instance.Toast("The lever is already on.");
            return;
        }
        pulled = true;
        door.Unlock();
        if (statusLight != null) statusLight.color = new Color(0.3f, 1f, 0.4f);
        Hud.Instance.Toast("Power restored: the north door opens now.");
    }
}
