using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro;          // Required for TextMeshPro

public class WeaponSelection : MonoBehaviour
{
    [Header("References")]
    public CosmosWeapon cosmosWeaponScript; // Drag your Player object here
    public GameObject selectionPanel;       // Drag the Panel containing the buttons here

    [Header("UI Components")]
    public Button buttonLeft;
    public TMP_Text textLeft;   // CHANGED to TMP_Text

    public Button buttonRight;
    public TMP_Text textRight;  // CHANGED to TMP_Text

    void Start()
    {
        // 1. Pause the game immediately
        Time.timeScale = 0f;

        // 2. Lock player input so they can't shoot while choosing
        if (cosmosWeaponScript != null)
            cosmosWeaponScript.isInputLocked = true;

        // 3. Unlock cursor so they can click buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. Generate Options
        GenerateChoices();
    }

    void GenerateChoices()
    {
        // Get all possible weapon types from the Enum
        var values = System.Enum.GetValues(typeof(CosmosWeapon.WeaponType));
        int count = values.Length;

        // Pick two random DIFFERENT indexes
        int index1 = Random.Range(0, count);
        int index2 = index1;

        while (index2 == index1)
        {
            index2 = Random.Range(0, count);
        }

        // Convert indexes back to WeaponType
        CosmosWeapon.WeaponType weapon1 = (CosmosWeapon.WeaponType)values.GetValue(index1);
        CosmosWeapon.WeaponType weapon2 = (CosmosWeapon.WeaponType)values.GetValue(index2);

        // Update Button Text
        textLeft.text = weapon1.ToString();
        textRight.text = weapon2.ToString();

        // Setup Button Clicks
        // We remove old listeners first to be safe
        buttonLeft.onClick.RemoveAllListeners();
        buttonLeft.onClick.AddListener(() => SelectWeapon(weapon1));

        buttonRight.onClick.RemoveAllListeners();
        buttonRight.onClick.AddListener(() => SelectWeapon(weapon2));
    }

    void SelectWeapon(CosmosWeapon.WeaponType selectedType)
    {
        Debug.Log("Selected: " + selectedType);

        // 1. Equip the chosen weapon
        cosmosWeaponScript.EquipWeapon(selectedType);

        // 2. Hide the UI
        selectionPanel.SetActive(false);

        // 3. Resume Game
        Time.timeScale = 1f;

        // 4. Unlock Player Input
        cosmosWeaponScript.isInputLocked = false;

        // 5. Hide Cursor (Optional: Add this if your game is a shooter)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}