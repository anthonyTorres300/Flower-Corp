using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro;          // Required for TextMeshPro

public class WeaponSelection : MonoBehaviour
{
    [Header("References")]
    public CosmosShoot cosmosWeaponScript; // Drag your Player object here
    public GameObject selectionPanel;       // Drag the Panel containing the buttons here
    public WaveManager waveManager;         // NEW: Drag WaveManager here
    public GameObject hudPanel;             // NEW: The HUD that's blocking clicks

    [Header("UI Components")]
    public Button buttonLeft;
    public TMP_Text textLeft;   // CHANGED to TMP_Text

    public Button buttonRight;
    public TMP_Text textRight;  // CHANGED to TMP_Text

    void Start()
    {
        // DON'T pause with Time.timeScale - it breaks UI button clicks!
        // Instead, just lock player input
        
        // 1. Hide the HUD so it doesn't block button clicks!
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
            Debug.Log("[WEAPON SELECTION] HUD Panel hidden to prevent click blocking");
        }
        
        // 2. Lock player input so they can't shoot while choosing
        if (cosmosWeaponScript != null)
            cosmosWeaponScript.isInputLocked = true;

        // 3. Unlock cursor so they can click buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. Make sure the panel is active
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            Debug.Log("[WEAPON SELECTION] Panel activated");
        }
        else
        {
            Debug.LogError("[WEAPON SELECTION] Selection Panel is NULL! Drag it in the inspector!");
        }

        // 5. Verify buttons are set up
        if (buttonLeft == null || buttonRight == null)
        {
            Debug.LogError("[WEAPON SELECTION] Buttons are NULL! Drag them in the inspector!");
        }

        if (textLeft == null || textRight == null)
        {
            Debug.LogError("[WEAPON SELECTION] Text fields are NULL! Drag them in the inspector!");
        }

        // 6. Generate Options
        GenerateChoices();
    }

    void GenerateChoices()
    {
        Debug.Log("[WEAPON SELECTION] Generating weapon choices...");

        // Get all possible weapon types from the Enum
        var values = System.Enum.GetValues(typeof(WeaponType));
        int count = values.Length;

        // Pick two random DIFFERENT indexes
        int index1 = Random.Range(0, count);
        int index2 = index1;

        while (index2 == index1)
        {
            index2 = Random.Range(0, count);
        }

        // Convert indexes back to WeaponType
        WeaponType weapon1 = (WeaponType)values.GetValue(index1);
        WeaponType weapon2 = (WeaponType)values.GetValue(index2);

        Debug.Log($"[WEAPON SELECTION] Option 1: {weapon1}, Option 2: {weapon2}");

        // Update Button Text
        if (textLeft != null)
            textLeft.text = weapon1.ToString();
        if (textRight != null)
            textRight.text = weapon2.ToString();

        // Setup Button Clicks
        // We remove old listeners first to be safe
        if (buttonLeft != null)
        {
            buttonLeft.onClick.RemoveAllListeners();
            buttonLeft.onClick.AddListener(() => SelectWeapon(weapon1));
            Debug.Log("[WEAPON SELECTION] Left button listener added");
        }

        if (buttonRight != null)
        {
            buttonRight.onClick.RemoveAllListeners();
            buttonRight.onClick.AddListener(() => SelectWeapon(weapon2));
            Debug.Log("[WEAPON SELECTION] Right button listener added");
        }
    }

    void SelectWeapon(WeaponType selectedType)
    {
        Debug.Log($"[WEAPON SELECTION] *** SelectWeapon called with: {selectedType} ***");

        // 1. Equip the chosen weapon
        if (cosmosWeaponScript != null)
        {
            cosmosWeaponScript.EquipWeapon(selectedType);
            Debug.Log($"[WEAPON SELECTION] Weapon equipped: {selectedType}");
        }
        else
        {
            Debug.LogError("[WEAPON SELECTION] cosmosWeaponScript is NULL!");
        }

        // 2. Hide the weapon selection UI
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
            Debug.Log("[WEAPON SELECTION] Selection panel hidden");
        }

        // 3. Show the HUD again
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
            Debug.Log("[WEAPON SELECTION] HUD Panel re-enabled");
        }

        // 4. Unlock Player Input
        if (cosmosWeaponScript != null)
        {
            cosmosWeaponScript.isInputLocked = false;
            Debug.Log("[WEAPON SELECTION] Player input unlocked");
        }

        // 5. START THE WAVE SYSTEM!
        if (waveManager != null)
        {
            waveManager.StartGame();
            Debug.Log("[WEAPON SELECTION] Notified WaveManager to start waves");
        }
        else
        {
            Debug.LogWarning("[WEAPON SELECTION] WaveManager reference is missing! Drag it in the inspector.");
        }

        // 6. Hide Cursor (Optional: Add this if your game is a shooter)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}