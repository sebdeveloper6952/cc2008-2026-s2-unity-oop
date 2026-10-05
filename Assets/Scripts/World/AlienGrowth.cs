using UnityEngine;

// Alien growth blocking the way. Shooting burns it.
public class AlienGrowth : MonoBehaviour
{
    [SerializeField] int health = 40;

    public void Burn(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Fx.Explosion(transform.position, 0.6f);
            Destroy(gameObject);
        }
    }
}
