using UnityEngine;

public class CosmosBullets : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 20f;
    public int damage = 20;
    public float lifetime = 3f;

    private Rigidbody2D rb;
    private CosmosShoot weaponScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // find the weapon that fired this
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            CosmosShoot weapon = player.GetComponent<CosmosShoot>();
            if (weapon != null)
            {
                weaponScript = weapon;
                damage = weapon.damage;
                bulletSpeed = weapon.bulletSpeed;
                break;
            }
        }

        // move forward
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = transform.up * bulletSpeed;
        }

        // auto destroy
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // hit enemy
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

        // hit wall
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}