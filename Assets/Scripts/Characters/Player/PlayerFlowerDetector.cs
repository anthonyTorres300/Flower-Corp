using UnityEngine;

public class PlayerFlowerDetector : MonoBehaviour
{
    public bool isStandingInInk = false;

    // This triggers when the player's collider overlaps with the splat's trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Flower"))
        {
            isStandingInInk = true;
            // Example: Slow the player down or change their color
            Debug.Log("Standing in flower!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ink"))
        {
            isStandingInInk = false;
        }
    }
}