using UnityEngine;

// A gun. The engineer starts with the handgun in her hand. A gun left on the floor floats
// and spins until she picks it up.
public class Gun : MonoBehaviour
{
    [SerializeField] string gunName = "Handgun";
    [SerializeField] int damage = 10;
    [SerializeField] float secondsBetweenShots = 0.35f;
    [SerializeField] float range = 25f;

    Transform muzzle;                            // where the shots come out (a child named "Muzzle")
    Light glow;                                  // only while it is on the floor
    Vector3 floorPosition;
    bool held;

    public string Name => gunName;
    public int Damage => damage;
    public float SecondsBetweenShots => secondsBetweenShots;
    public float Range => range;
    public Transform Muzzle => muzzle;
    public bool IsHeld => held;

    void Awake()
    {
        muzzle = transform.Find("Muzzle");
        glow = GetComponentInChildren<Light>(true);
        floorPosition = transform.position;
        if (GetComponentInParent<Player>() != null)
        {
            Hold();                              // already in her hand
        }
        else if (glow != null)
        {
            glow.enabled = true;
        }
    }

    void Update()
    {
        if (held) return;
        transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        transform.position = floorPosition + Vector3.up * (0.9f + Mathf.Sin(Time.time * 2f) * 0.08f);
    }

    public void PickUp(Player player)
    {
        if (held) return;
        Hold();
        transform.SetParent(player.transform, true);
        player.Loadout.Add(this);
    }

    void Hold()
    {
        held = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;                   // nothing finds it on the floor any more
        }
        if (glow != null) glow.enabled = false;
    }
}
