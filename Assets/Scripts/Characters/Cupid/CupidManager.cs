using UnityEngine;
using System.Collections.Generic;

public class CupidManager : MonoBehaviour
{
    [Header("Active Cupids")]
    public List<CupidData> currentCupids = new List<CupidData>();
    public Transform cupidSpawnPoint;

    [Header("Player Components")]
    public CosmosShoot cosmosWeapon;
    public LilyShooter lilyWeapon;

    [Header("Wave System")]
    public WaveManager waveManager;

    // base stats
    private float cosmosBaseFireRate;
    private int cosmosBaseDamage;
    private int cosmosBaseAmmo;
    private float lilyBaseFireRate;
    private int lilyBaseDamage;
    private int lilyBaseAmmo;

    void Start()
    {
        if (cosmosWeapon != null)
        {
            cosmosBaseFireRate = cosmosWeapon.fireRate;
            cosmosBaseDamage = cosmosWeapon.damage;
            cosmosBaseAmmo = cosmosWeapon.maxAmmo;
        }

        if (lilyWeapon != null)
        {
            lilyBaseFireRate = lilyWeapon.fireRate;
            lilyBaseDamage = 5; // lily doesn't have public damage, default to 5
            lilyBaseAmmo = lilyWeapon.maxAmmo;
        }
    }

    public void AddCupid(CupidData newCupid)
    {
        currentCupids.Add(newCupid);

        // Notify wave manager that a cupid was saved!
        if (waveManager != null)
        {
            waveManager.OnCupidSaved();
        }

        // spawn cupid companion
        if (newCupid.cupidPrefab != null && cupidSpawnPoint != null)
        {
            GameObject cupidObj = Instantiate(newCupid.cupidPrefab, cupidSpawnPoint.position, Quaternion.identity);

            CupidFollow followScript = cupidObj.GetComponent<CupidFollow>();
            if (followScript == null) followScript = cupidObj.AddComponent<CupidFollow>();

            followScript.followDistance = Random.Range(2f, 4f);
        }

        UpdatePlayerStats();
    }

    public void RemoveCupid(CupidData cupidToRemove)
    {
        currentCupids.Remove(cupidToRemove);
        UpdatePlayerStats();
    }

    void UpdatePlayerStats()
    {
        float totalFireRateBonus = 0f;
        float totalDamageBonus = 0f;
        int totalAmmoBonus = 0;

        // sum up all bonuses
        foreach (CupidData cupid in currentCupids)
        {
            switch (cupid.perkType)
            {
                case CupidPerk.FireRateBoost:
                    totalFireRateBonus += cupid.perkValue;
                    break;
                case CupidPerk.DamageBoost:
                    totalDamageBonus += cupid.perkValue;
                    break;
                case CupidPerk.MaxAmmo:
                    totalAmmoBonus += (int)cupid.perkValue;
                    break;
            }
        }

        // apply to cosmos
        if (cosmosWeapon != null)
        {
            cosmosWeapon.fireRate = cosmosBaseFireRate * (1.0f - totalFireRateBonus);
            cosmosWeapon.damage = cosmosBaseDamage + (int)totalDamageBonus;
            cosmosWeapon.maxAmmo = cosmosBaseAmmo + totalAmmoBonus;
        }

        // apply to lily
        if (lilyWeapon != null)
        {
            lilyWeapon.fireRate = lilyBaseFireRate * (1.0f - totalFireRateBonus);
            // lily doesn't have public damage field, would need to add it
            lilyWeapon.maxAmmo = lilyBaseAmmo + totalAmmoBonus;
        }
    }
}