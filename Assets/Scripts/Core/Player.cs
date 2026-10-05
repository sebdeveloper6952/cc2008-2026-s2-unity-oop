using UnityEngine;

// The engineer on duty. Gathers the player's parts so other objects can use them:
// player.Oxygen, player.Inventory, player.Loadout.
[RequireComponent(typeof(Oxygen), typeof(Inventory), typeof(Loadout))]
public class Player : MonoBehaviour
{
    public Oxygen Oxygen { get; private set; }
    public Inventory Inventory { get; private set; }
    public Loadout Loadout { get; private set; }

    void Awake()
    {
        Oxygen = GetComponent<Oxygen>();
        Inventory = GetComponent<Inventory>();
        Loadout = GetComponent<Loadout>();
    }
}
