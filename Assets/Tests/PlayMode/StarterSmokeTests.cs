using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// Smoke tests for the starter project: they load the real scene and click with a simulated mouse.
// They test behavior, not code, so they must keep passing after every ticket.
public class StarterSmokeTests
{
    Mouse mouse;
    Keyboard keyboard;
    InputSettings.BackgroundBehavior savedBackground;
    InputSettings.EditorInputBehaviorInPlayMode savedEditorBehavior;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // with no focused window (batch mode) the Input System ignores devices: turn that off
        savedBackground = InputSystem.settings.backgroundBehavior;
        savedEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

        mouse = InputSystem.AddDevice<Mouse>();
        keyboard = InputSystem.AddDevice<Keyboard>();
        SceneManager.LoadScene("Outpost13");
        yield return null;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        InputSystem.RemoveDevice(mouse);
        InputSystem.RemoveDevice(keyboard);
        InputSystem.settings.backgroundBehavior = savedBackground;
        InputSystem.settings.editorInputBehaviorInPlayMode = savedEditorBehavior;
        Time.timeScale = 1f;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Scene_Loads_WithEverythingWired()
    {
        var player = Object.FindFirstObjectByType<Player>();
        Assert.IsNotNull(player);
        Assert.IsNotNull(GameManager.Instance);
        Assert.IsNotNull(Hud.Instance);
        Assert.AreEqual(2, Object.FindObjectsByType<Terminal>(FindObjectsSortMode.None).Length);
        Assert.IsTrue(NavMesh.SamplePosition(player.transform.position, out _, 0.5f, NavMesh.AllAreas), "the player should stand on the NavMesh");
        yield return new WaitForSeconds(0.5f);
        Assert.IsFalse(GameManager.Instance.IsOver);
    }

    [UnityTest]
    public IEnumerator ClickingTheFloor_WalksThere()
    {
        Transform player = Object.FindFirstObjectByType<Player>().transform;
        Vector3 goal = new Vector3(2f, 0f, 0f);
        yield return Click(goal);
        yield return WaitFor(() => Flat(player.position - goal) < 0.4f, 5f);
        Assert.Less(Flat(player.position - goal), 0.4f, "clicking the floor should walk the engineer there");
    }

    [UnityTest]
    public IEnumerator Engineer_FacesTheWaySheWalks()
    {
        Transform player = Object.FindFirstObjectByType<Player>().transform;
        var animator = player.GetComponentInChildren<Animator>();
        yield return Click(new Vector3(3f, 0f, -6.5f));
        yield return new WaitForSeconds(0.4f);

        Vector3 left = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
        Vector3 right = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
        Vector3 modelForward = Vector3.Cross(right - left, Vector3.up).normalized;
        Vector3 velocity = player.GetComponent<NavMeshAgent>().velocity;
        velocity.y = 0f;
        Assert.Greater(velocity.magnitude, 0.5f, "she should be walking");
        Assert.Greater(Vector3.Dot(modelForward, velocity.normalized), 0.7f, "the model should face where it walks");
    }

    [UnityTest]
    public IEnumerator Lever_UnlocksDoor_ThenTheDoorLetsYouThrough()
    {
        var door = Object.FindFirstObjectByType<Door>();
        var lever = Object.FindFirstObjectByType<Lever>();
        Transform player = Object.FindFirstObjectByType<Player>().transform;
        Vector3 inFrontOfDoor = new Vector3(0f, 0f, 7f);
        Vector3 room2 = new Vector3(0f, 0f, 14f);

        Assert.AreNotEqual(NavMeshPathStatus.PathComplete, PathStatus(player.position, room2), "the closed door must block the way");

        yield return Click(inFrontOfDoor);
        yield return WaitFor(() => Flat(player.position - inFrontOfDoor) < 0.4f, 6f);
        yield return Press(Key.E);
        Assert.IsFalse(door.IsOpen, "a door without power must not open");

        yield return Click(lever.transform.position + Vector3.up * 0.7f);   // clicking only walks there...
        yield return WaitFor(() => Flat(player.position - lever.transform.position) < 1.6f, 8f);
        yield return new WaitForSeconds(0.5f);
        Assert.IsFalse(lever.IsPulled, "clicking an object must not use it");
        yield return Press(Key.E);                                         // ...E uses it
        Assert.IsTrue(lever.IsPulled);
        Assert.IsFalse(door.IsLocked);

        yield return Click(inFrontOfDoor);
        yield return WaitFor(() => Flat(player.position - inFrontOfDoor) < 0.4f, 6f);
        yield return Press(Key.E);
        Assert.IsTrue(door.IsOpen);
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(NavMeshPathStatus.PathComplete, PathStatus(player.position, room2), "the open door must let the path through");

        yield return Click(new Vector3(1f, 0f, 9.8f));
        yield return WaitFor(() => player.position.z > 9.3f, 6f);
        Assert.Greater(player.position.z, 9.3f, "clicking past the open door should walk into the north room");
    }

    [UnityTest]
    public IEnumerator Handgun_StartsInHand_FiresOncePerClick()
    {
        var player = Object.FindFirstObjectByType<Player>();
        Assert.IsNotNull(player.Loadout.Current, "she starts with a gun in her hand");
        Assert.AreEqual("Handgun", player.Loadout.Current.Name);
        Assert.IsTrue(player.Loadout.Current.IsHeld);

        var growth = Object.FindFirstObjectByType<AlienGrowth>();
        Object.FindFirstObjectByType<ClickToMove>().Warp(new Vector3(-4f, 0f, -1f));
        yield return new WaitForSeconds(0.5f);
        Vector3 aim = growth.transform.position + Vector3.up * 1.2f;
        yield return Click(aim, right: true, hold: 1f);               // holding fires one shot only
        Assert.IsTrue(growth != null, "the handgun fires once per click");
        for (int i = 0; i < 5 && growth != null; i++)
        {
            yield return new WaitForSeconds(0.4f);
            yield return Click(aim, right: true);
        }
        yield return null;
        Assert.IsTrue(growth == null, "four handgun shots burn the growth");
    }

    [UnityTest]
    public IEnumerator Oxygen_RunsOut_Loses()
    {
        Object.FindFirstObjectByType<Player>().Oxygen.Drain(10000f);
        yield return null;
        Assert.IsTrue(GameManager.Instance.IsOver);
        Assert.IsFalse(GameManager.Instance.Won);
    }

    [UnityTest]
    public IEnumerator CommsTerminal_NeedsPower_ThenSendsSignal_Wins()
    {
        var comms = GameObject.Find("CommsTerminal").GetComponent<Terminal>();
        Object.FindFirstObjectByType<ClickToMove>().Warp(new Vector3(-3.4f, 0f, 17.9f));
        yield return new WaitForSeconds(0.8f);                    // camera catches up

        yield return Press(Key.E);
        Assert.IsFalse(GameManager.Instance.IsOver, "without power the console does nothing");

        comms.PowerOn();
        yield return Press(Key.E);
        Assert.IsTrue(GameManager.Instance.IsOver);
        Assert.IsTrue(GameManager.Instance.Won);
    }

    // ------------------------------------------------------------ helpers

    IEnumerator Click(Vector3 world, bool right = false, float hold = 0f)
    {
        Vector2 screen = Camera.main.WorldToScreenPoint(world);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
        yield return null;
        yield return null;
        var down = new MouseState { position = screen }.WithButton(right ? MouseButton.Right : MouseButton.Left);
        InputSystem.QueueStateEvent(mouse, down);
        if (hold > 0f)
        {
            yield return new WaitForSeconds(hold);
        }
        else
        {
            yield return null;
            yield return null;
        }
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
        yield return null;
    }

    IEnumerator Press(Key key)
    {
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    static IEnumerator WaitFor(Func<bool> condition, float timeout)
    {
        for (float t = 0f; t < timeout && !condition(); t += Time.deltaTime) yield return null;
    }

    static NavMeshPathStatus PathStatus(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
        return path.status;
    }

    static float Flat(Vector3 v)
    {
        v.y = 0f;
        return v.magnitude;
    }
}
