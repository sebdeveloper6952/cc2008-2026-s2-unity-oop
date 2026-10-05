using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The guns the engineer carries. Keys 1 and 2 switch between them.
// The one in use sits in her right hand and points where she faces.
[DefaultExecutionOrder(100)]                     // runs after ProceduralWalk has posed the arm
public class Loadout : MonoBehaviour
{
    readonly List<Gun> guns = new List<Gun>();
    Transform hand;
    ProceduralWalk walk;

    public Gun Current { get; private set; }
    public IReadOnlyList<Gun> Guns => guns;

    void Awake()
    {
        walk = GetComponentInChildren<ProceduralWalk>();
        hand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
    }

    void Start()
    {
        foreach (Gun gun in GetComponentsInChildren<Gun>(true))
        {
            if (gun == null) continue;           // its script could not load (e.g. an abstract class)
            Carry(gun);                          // the guns she starts with
        }
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null || GameManager.Instance.IsOver) return;
        if (k.digit1Key.wasPressedThisFrame) Equip(0);
        if (k.digit2Key.wasPressedThisFrame) Equip(1);
    }

    void LateUpdate()
    {
        HoldPose();
    }

    public void Add(Gun gun)
    {
        Carry(gun);
        Hud.Instance.Toast("Picked up: " + gun.Name + ". Keys 1 and 2 switch guns.", 4f);
    }

    void Carry(Gun gun)
    {
        guns.Add(gun);
        Equip(guns.Count - 1);
    }

    public void Equip(int index)
    {
        if (index < 0 || index >= guns.Count) return;
        foreach (Gun g in guns)
        {
            g.gameObject.SetActive(false);
        }
        Current = guns[index];
        Current.gameObject.SetActive(true);
        if (walk != null) walk.HoldsGun = true;
        HoldPose();
    }

    // The gun in use goes to the right hand, pointing where the engineer faces.
    public void HoldPose()
    {
        if (Current == null || hand == null) return;
        Current.transform.SetPositionAndRotation(hand.position, Quaternion.LookRotation(transform.forward));
    }
}
