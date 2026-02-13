using UnityEngine;

public enum CupidPerk
{
    FireRateBoost,
    DamageBoost,
    MaxAmmo
}

[CreateAssetMenu(fileName = "NewCupid", menuName = "Flower Corp/Cupid Data")]
public class CupidData : ScriptableObject
{
    [Header("Cupid Info")]
    public string cupidName;
    public Sprite cupidSprite;
    public GameObject cupidPrefab;

    [Header("Perk System")]
    public CupidPerk perkType;
    public float perkValue; // 0.2 = 20% for fire rate, or flat number for damage/ammo

    [Header("Visual")]
    public Color cupidColor = Color.white;
}