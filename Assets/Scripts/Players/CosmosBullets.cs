using UnityEngine;

public class CosmosBullets : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "AmmoCrate"; // The tag of the object to shoot
    public int ammoToGive = 5;             // How much ammo to restore

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Check if we hit the specific "AmmoCrate" object
        if (hitInfo.CompareTag(targetTag))
        {
            // Find the player weapon script to give ammo
            // (We use FindObjectOfType because there is usually only one player)
            CosmosWeapon playerWeapon = FindObjectOfType<CosmosWeapon>();

            if (playerWeapon != null)
            {
                playerWeapon.AddAmmo(ammoToGive);
            }

            // Destroy the crate
            Destroy(hitInfo.gameObject);

            // Destroy the bullet (so it doesn't go through)
            Destroy(gameObject);
        }

        // 2. Destroy bullet on hitting walls or other obstacles
        // (Make sure the Player is not tagged "Untagged" or it might hit the player!)
        else if (!hitInfo.CompareTag("Player") && !hitInfo.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}