using UnityEngine;

// A console. It shows a message or, for the comms console, sends the rescue signal.
public class Terminal : MonoBehaviour
{
    [SerializeField, TextArea(3, 6)] string message;
    [SerializeField] bool needsPower;
    [SerializeField] bool sendsRescueSignal;
    [SerializeField] Light screenLight;

    bool powered;

    public bool IsPowered => !needsPower || powered;

    void Start()
    {
        if (screenLight != null) screenLight.enabled = IsPowered;
    }

    // Called by whatever powers it (the generator, for example).
    public void PowerOn()
    {
        powered = true;
        if (screenLight != null) screenLight.enabled = true;
    }

    public void Use(Player player)
    {
        if (!IsPowered)
        {
            Hud.Instance.Toast("The screen is dark: no power.");
            return;
        }
        if (sendsRescueSignal)
        {
            int left = Mathf.CeilToInt(player.Oxygen.Remaining);
            GameManager.Instance.Win("Rescue signal sent with " + left + " s of oxygen left.");
            return;
        }
        Hud.Instance.Toast(message, 8f);
    }
}
