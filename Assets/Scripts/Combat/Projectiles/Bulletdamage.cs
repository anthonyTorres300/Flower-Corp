using UnityEngine;

public class Bulletdamage : MonoBehaviour
{
    public int damage = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        // hit an enemy
        if (other.CompareTag("Enemy"))
        {
            Damageable enemy = other.GetComponent<Damageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // hit a wall
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}