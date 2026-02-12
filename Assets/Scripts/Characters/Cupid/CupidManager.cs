using UnityEngine;
using System.Collections.Generic;

public class CupidManager : MonoBehaviour
{
    [Header("Active Cupids")]
    public List<CupidData> currentCupids = new List<CupidData>(); // The list of cupids following you
    public Transform cupidSpawnPoint; // Where they appear (usually behind player)

    [Header("Player Components")]
    public CosmosWeapon weaponScript;   // Drag your weapon script here
    // public PlayerMovement movementScript; // Drag movement script here if you have one

    // Base stats to remember what we started with
    private float baseFireRate;
    private float baseDamage = 1.0f; // Default multiplier

    void Start()
    {
        if (weaponScript != null)
        {
            baseFireRate = weaponScript.fireRate;
            // baseDamage = weaponScript.damage; (If you added damage to CosmosWeapon)
        }
    }

    // Call this to add a new Cupid!
    public void AddCupid(CupidData newCupid)
    {
        // 1. Add to list
        currentCupids.Add(newCupid);

        // 2. Spawn the visual object
        if (newCupid.cupidPrefab != null)
        {
            GameObject cupidObj = Instantiate(newCupid.cupidPrefab, cupidSpawnPoint.position, Quaternion.identity);

            // Setup the follow script automatically
            CupidFollow followScript = cupidObj.GetComponent<CupidFollow>();
            if (followScript == null) followScript = cupidObj.AddComponent<CupidFollow>();

            followScript.player = this.transform; // Tell cupid to follow ME
            followScript.fenceRadius = Random.Range(2f, 4f); // Randomize leash so they don't stack
            followScript.wanderRadius = 1.5f;
        }

        // 3. Recalculate all stats
        UpdatePlayerStats();
    }

    void UpdatePlayerStats()
    {
        float totalFireRateBonus = 0f;
        float totalDamageBonus = 0f;
        int totalAmmoBonus = 0;

        // Loop through every cupid we have and add up the numbers
        foreach (CupidData cupid in currentCupids)
        {
            switch (cupid.perkType)
            {
                case cupidPerk.FireRateBoost:
                    totalFireRateBonus += cupid.perkValue;
                    break;
                case cupidPerk.DamageBoost:
                    totalDamageBonus += cupid.perkValue;
                    break;
                case cupidPerk.MaxAmmo:
                    totalAmmoBonus += (int)cupid.perkValue;
                    break;
            }
        }

        // Apply to Weapon Script
        if (weaponScript != null)
        {
            // Example: Lower fire rate is faster. 
            // If base is 0.5s and we have 20% bonus, we reduce delay by 20%
            weaponScript.fireRate = baseFireRate * (1.0f - totalFireRateBonus);

            // Add max ammo (Be careful not to reset current ammo improperly)
            weaponScript.maxAmmo += totalAmmoBonus;

            Debug.Log($"Stats Updated! Cupids: {currentCupids.Count} | FireRate Bonus: {totalFireRateBonus * 100}%");
        }
    }
}