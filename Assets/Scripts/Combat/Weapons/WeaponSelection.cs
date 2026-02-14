using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List manipulation if needed

public class WeaponSelection : MonoBehaviour
{
    [Header("References")]
    public CosmosWeapon cosmosWeaponScript; 
    public WaveSpawner waveSpawner; // **NEW: Reference to the spawner**
    public GameObject selectionPanel;       

    [Header("UI Components")]
    public Button buttonLeft;
    public TMP_Text textLeft;  

    public Button buttonRight;
    public TMP_Text textRight; 

    void Start()
    {
        // Ensure panel is hidden at start of game
        selectionPanel.SetActive(false);
    }

    // Call this from WaveSpawner when a wave ends
    public void ShowSelection()
    {
        selectionPanel.SetActive(true);

        // 1. Pause the game
        Time.timeScale = 0f;

        // 2. Lock player input
        if (cosmosWeaponScript != null)
            cosmosWeaponScript.isInputLocked = true;

        // 4. Generate Options
        GenerateChoices();
    }

    void GenerateChoices()
    {
        var values = System.Enum.GetValues(typeof(CosmosWeapon.WeaponType));
        int count = values.Length;

        // Simple protection against errors if you have fewer than 2 weapons
        if (count < 2) 
        {
            Debug.LogError("Not enough weapons in Enum to generate choices!");
            return;
        }

        // Pick two random DIFFERENT indexes
        int index1 = Random.Range(0, count);
        int index2 = index1;

        while (index2 == index1)
        {
            index2 = Random.Range(0, count);
        }

        CosmosWeapon.WeaponType weapon1 = (CosmosWeapon.WeaponType)values.GetValue(index1);
        CosmosWeapon.WeaponType weapon2 = (CosmosWeapon.WeaponType)values.GetValue(index2);

        textLeft.text = weapon1.ToString();
        textRight.text = weapon2.ToString();

        // Setup Button Clicks
        buttonLeft.onClick.RemoveAllListeners();
        buttonLeft.onClick.AddListener(() => SelectWeapon(weapon1));

        buttonRight.onClick.RemoveAllListeners();
        buttonRight.onClick.AddListener(() => SelectWeapon(weapon2));
    }

    void SelectWeapon(CosmosWeapon.WeaponType selectedType)
    {
        // 1. Equip the chosen weapon
        if(cosmosWeaponScript != null)
            cosmosWeaponScript.EquipWeapon(selectedType);

        // 2. Hide the UI
        selectionPanel.SetActive(false);

        // 3. Resume Game Time
        Time.timeScale = 1f;

        // 4. Unlock Player Input
        if (cosmosWeaponScript != null)
            cosmosWeaponScript.isInputLocked = false;

        // 6. TELL THE WAVE SPAWNER TO CONTINUE
        if(waveSpawner != null)
        {
            waveSpawner.OnWeaponSelected();
        }
    }
}