using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement; // Required for SceneManager
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [System.Serializable]
    public struct StoryPage
    {
        [TextArea(3, 10)] 
        public string text;
        public Sprite image;
        public float durationAfterText; 
    }

    [Header("UI References")]
    public Image displayImage;
    public TextMeshProUGUI displayText;

    [Header("Settings")]
    public float typingSpeed = 0.05f; 
    public StoryPage[] pages; 
    public string nextSceneName = "TitleScreen"; 

    private int currentPage = 0;

    void Start()
    {
        if (pages.Length > 0)
        {
            StartCoroutine(PlaySequence());
        }
        else
        {
            FinishIntro();
        }
    }

    void Update()
    {
        // SKIP LOGIC: If Enter is pressed, skip everything and load immediately
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            FinishIntro();
        }
    }

    IEnumerator PlaySequence()
    {
        // Loop through every page in the array
        for (int i = 0; i < pages.Length; i++)
        {
            StoryPage page = pages[i];

            // 1. Set Image
            if (displayImage != null && page.image != null)
            {
                displayImage.sprite = page.image;
                displayImage.preserveAspect = true;
            }

            // 2. Clear text and Type out
            if (displayText != null)
            {
                displayText.text = "";
                foreach (char letter in page.text.ToCharArray())
                {
                    displayText.text += letter;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            // 3. Wait for reading time
            yield return new WaitForSeconds(page.durationAfterText);
        }

        // Once the loop is done, finish
        FinishIntro();
    }

    void FinishIntro()
    {
        Debug.Log("Intro Finished/Skipped! Loading next scene...");
        SceneManager.LoadScene(nextSceneName);
    }
}