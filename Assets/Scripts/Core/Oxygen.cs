using UnityEngine;

// The game clock: oxygen drops one second per second.
// Any damage the engineer takes is also subtracted from here.
public class Oxygen : MonoBehaviour
{
    [SerializeField] float seconds = 180f;

    public float Remaining { get; private set; }
    public float Max => seconds;
    public bool IsEmpty => Remaining <= 0f;

    void Awake()
    {
        Remaining = seconds;
    }

    void Update()
    {
        if (GameManager.Instance.IsOver) return;
        Drain(Time.deltaTime);
    }

    // Removes 'amount' seconds of oxygen.
    public void Drain(float amount)
    {
        if (IsEmpty || GameManager.Instance.IsOver) return;
        Remaining = Mathf.Max(0f, Remaining - amount);
        if (IsEmpty) GameManager.Instance.Lose("You ran out of oxygen.");
    }
}
