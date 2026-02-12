using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro; // REQUIRED: Uses TextMeshPro

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;

    [Header("Settings")]
    public float delayBeforeLoad = 1.0f; // Artificial delay so players can read the tip

    // A large array of strings for your tips
    [TextArea(2, 5)]
    public string[] randomTips = new string[]
    {
        "Tip: Don't forget to reload your weapon before entering a new room.",
        "Tip: Shotguns are devastating at close range, but weak from afar.",
        "Lore: The Cosmos Federation was founded in the year 3042.",
        "Tip: Red barrels explode when shot. Use them to your advantage!",
        "Tip: Keep moving! Standing still makes you an easy target.",
        "Tip: Some enemies have energy shields. Use energy weapons to deplete them.",
        "Fact: Space is silent, but your guns are loud.",
        "Tip: Check every corner for loot chests.",
        "Tip: You can switch weapons using the number keys or mouse wheel.",
        "Tip: If you're low on health, look for green medical stations.",
        "Tip: Bosses often have a specific pattern. Learn it to survive.",
        "Tip: Snipers have high damage but a slow fire rate. Make every shot count.",
        "Tip: You can dash through some projectiles if you time it right.",
        "Tip: Upgrade your armor to survive longer in the deep sectors.",
        "Lore: The alien species known as 'The Void' consume entire planets.",
        "Tip: Press 'R' to reload manually.",
        "Tip: Headshots deal double damage to most organic enemies.",
        "Tip: Explosives can hurt you too. Watch your spacing!",
        "Tip: Rare weapons have special passive abilities.",
        "Tip: Don't hoard your grenades; use them when overwhelmed.",
        "Tip: The Burst Rifle is excellent for mid-range combat.",
        "Tip: conserve ammo by using your pistol on weaker enemies.",
        "Tip: Listen for audio cues to know when enemies are spawning.",
        "Tip: The map generates differently every time you die.",
        "Lore: There are rumors of a secret level hidden in Sector 7.",
        "Tip: Pause the game to adjust volume or sensitivity settings.",
        "Tip: Crouch behind cover to avoid taking damage.",
        "Tip: Enemies with yellow health bars are elites. Be careful.",
        "Tip: Completing challenges unlocks new starting gear.",
        "Tip: Always destroy the spawner nests first!"
    };

    // Call this function to start loading a level (e.g. from a button)
    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    // Call this version to load by name
    public void LoadLevel(string sceneName)
    {
        // Find the index of the scene by name
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (sceneIndex == -1)
        {
            Debug.LogError("Scene " + sceneName + " not found!");
            return;
        }
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // 1. Show the loading screen UI
        loadingScreen.SetActive(true);

        // 2. Pick a random tip
        if (tipText != null && randomTips.Length > 0)
        {
            int randomIndex = Random.Range(0, randomTips.Length);
            tipText.text = randomTips[randomIndex];
        }

        // 3. Optional: Wait a moment so the player sees the screen
        yield return new WaitForSeconds(delayBeforeLoad);

        // 4. Start loading the scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // 5. Update the progress bar while loading
        while (!operation.isDone)
        {
            // Unity's progress stops at 0.9, so we normalize it
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (slider != null)
                slider.value = progress;

            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }
    }
}