using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;      // Name of the speaker
    [TextArea(3, 10)]                 // Makes the text box bigger in Inspector
    public string sentence;           // The actual text
    public Sprite characterPortrait;  // The face image
}

[System.Serializable]
public class Dialogue
{
    public DialogueLine[] lines;      // A list of lines makes a conversation
}