using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [System.Serializable]
    public struct StoryPage
    {
        [TextArea(3, 10)] // Makes a bigger text box in Inspector
        public string text;
        public Sprite image;
        public float durationAfterText; // How long to wait before next page
    }

    [Header("UI References")]
    public Image displayImage;
    public TextMeshProUGUI displayText;

    [Header("Settings")]
    public float typingSpeed = 0.05f; // Seconds per character
    public StoryPage[] pages; // The list of story segments
    public string nextSceneName = "GameScene"; // Where to go after intro

    private int currentPage = 0;

    void Start()
    {
        // Start the sequence
        if (pages.Length > 0)
        {
            StartCoroutine(PlayPage(pages[0]));
        }
    }

    IEnumerator PlayPage(StoryPage page)
    {
        // 1. Set the Image
        if (page.image != null)
        {
            displayImage.sprite = page.image;
            // Optional: Preserve Aspect Ratio so image isn't squashed
            displayImage.preserveAspect = true;
        }

        // 2. Clear text and start typing
        displayText.text = "";
        foreach (char letter in page.text.ToCharArray())
        {
            displayText.text += letter;
            
            // Play a sound here if you want (e.g., AudioSource.PlayOneShot(typeSound));
            
            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. Wait for reading time
        yield return new WaitForSeconds(page.durationAfterText);

        // 4. Move to next page
        NextPage();
    }

    void NextPage()
    {
        currentPage++;

        if (currentPage < pages.Length)
        {
            StartCoroutine(PlayPage(pages[currentPage]));
        }
        else
        {
            FinishIntro();
        }
    }

    void FinishIntro()
    {
        Debug.Log("Intro Finished! Loading next scene...");
        // UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
    
    // Optional: Allow player to skip text by pressing Space
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Logic to skip typing or skip entire intro could go here
        }
    }
}