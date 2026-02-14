using UnityEngine;
using UnityEngine.SceneManagement; // Essential for loading scenes

public class SceneChanger : MonoBehaviour
{
    [Tooltip("Type the exact name of the scene you want to load here.")]
    public string sceneToLoad;

    // Connect this function to your UI Button
    public void ChangeScene()
    {
        // 1. Check if the scene name is valid
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene name is empty! Please type a scene name in the Inspector.");
            return;
        }

        // 2. Load the scene
        SceneManager.LoadScene(sceneToLoad);
    }
    
    // Optional: A specific function just for quitting the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}