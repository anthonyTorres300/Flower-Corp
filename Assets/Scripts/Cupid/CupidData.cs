using UnityEngine;

public enum cupidPerk { DamageBoost, FireRateBoost, MoveSpeed, MaxAmmo }

[CreateAssetMenu(fileName = "NewCupid", menuName = "Cupid Data")]
public class CupidData : ScriptableObject
{
    public string cupidName;
    public GameObject cupidPrefab; 
    public cupidPerk perkType;     
    public float perkValue;     
}