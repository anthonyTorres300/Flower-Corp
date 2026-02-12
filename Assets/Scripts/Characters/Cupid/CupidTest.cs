using UnityEngine;

public class CupidTest : MonoBehaviour
{
    public CupidManager manager;
    public CupidData cupidToTest;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            manager.AddCupid(cupidToTest);
            Debug.Log("Added a cupid!");
        }
    }
}