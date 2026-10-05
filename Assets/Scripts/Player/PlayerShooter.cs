using UnityEngine;

// Right-click fires the gun in hand at whatever is under the cursor.
//
// [!] This file works, but it is badly designed:
//     - every new thing that can be damaged forces you to add another if here. Ticket T0.
//     - every gun has to fire the same way, one straight shot. A shotgun fires six. Ticket T6.
public class PlayerShooter : MonoBehaviour
{
    ClickToMove movement;
    Loadout loadout;
    float nextShot;

    void Awake()
    {
        movement = GetComponent<ClickToMove>();
        loadout = GetComponent<Loadout>();
    }

    void Update()
    {
        if (GameManager.Instance.IsOver) return;
        if (!CursorPicker.RightClick || !CursorPicker.HasHit) return;

        Gun gun = loadout.Current;
        if (gun == null)
        {
            Hud.Instance.Toast("No gun in hand.");
            return;
        }
        if (Time.time < nextShot) return;
        nextShot = Time.time + gun.SecondsBetweenShots;

        movement.Stop();
        movement.Face(CursorPicker.Hit.point);
        loadout.HoldPose();                      // the gun turns with her before the shot leaves it
        Shoot(gun, CursorPicker.Hit.point);
    }

    public void Shoot(Gun gun, Vector3 target)
    {
        Vector3 origin = gun.Muzzle.position;
        Vector3 direction = (target - origin).normalized;
        Vector3 end = origin + direction * gun.Range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, gun.Range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;

            if (hit.collider.CompareTag("Growth"))
            {
                hit.collider.GetComponent<AlienGrowth>().Burn(gun.Damage);
            }
        }

        Fx.Laser(origin, end);
        Fx.Spark(end);
    }
}
