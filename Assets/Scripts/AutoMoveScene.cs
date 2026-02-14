using UnityEngine;
using UnityEngine.SceneManagement; 

public class AutoMoveScene : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The name of the scene you want to go to.")]
    public string sceneToLoad = "GameScene"; 

    [Tooltip("Time in seconds before switching.")]
    public float delayTime = 3f;

    void Start()
    {
        // Invoke allows us to call a function after a delay
        Invoke("LoadScene", delayTime);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}