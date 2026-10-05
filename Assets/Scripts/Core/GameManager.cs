using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Knows whether the game is over, and how. R restarts once it is.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsOver { get; private set; }
    public bool Won { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        Hud.Instance.Toast("Restore the generator and call for rescue before your oxygen runs out.\n" +
                           "Click to walk. Press E to use what is next to you. Right-click to shoot.", 7f);
    }

    void Update()
    {
        if (IsOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Win(string message) => End(true, message);
    public void Lose(string message) => End(false, message);

    void End(bool won, string message)
    {
        if (IsOver) return;
        IsOver = true;
        Won = won;
        Hud.Instance.ShowEnd(won, message);
        Time.timeScale = 0f;
    }
}
