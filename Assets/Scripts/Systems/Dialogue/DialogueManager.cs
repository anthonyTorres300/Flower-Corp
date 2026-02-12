using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // REQUIRED: Using TextMeshPro for crisp text

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dialogueBox;       // The Panel containing everything
    public Image portraitImage;          // The UI Image for the face
    public TextMeshProUGUI nameText;     // The TMP Text for the name
    public TextMeshProUGUI dialogueText; // The TMP Text for the body

    [Header("Settings")]
    public float typingSpeed = 0.04f;    // How fast the text types

    private Queue<DialogueLine> sentences; // A FIFO queue to manage lines
    private bool isTyping = false;       // To prevent skipping while typing
    private string currentFullSentence = "";

    void Start()
    {
        sentences = new Queue<DialogueLine>();
        dialogueBox.SetActive(false); // Hide UI at start
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueBox.SetActive(true);
        sentences.Clear();

        // Load all lines from the Inspector into the Queue
        foreach (DialogueLine line in dialogue.lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Feature: If player clicks while typing, finish the sentence instantly
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullSentence;
            isTyping = false;
            return;
        }

        // If no more lines, close the box
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue();

        // Update UI Elements
        nameText.text = currentLine.characterName;
        portraitImage.sprite = currentLine.characterPortrait;

        // Hide the portrait image if none is assigned
        if (currentLine.characterPortrait == null)
            portraitImage.gameObject.SetActive(false);
        else
            portraitImage.gameObject.SetActive(true);

        // Start the typing coroutine
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        currentFullSentence = sentence;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);
        Debug.Log("End of conversation.");

        // Optional: Resume game time here if you paused it
        // Time.timeScale = 1f;
    }
}