using UnityEngine;

public class CupidFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Geofence Settings")]
    public float fenceRadius = 3.0f;    // The "Leash" length
    public float wanderRadius = 2.0f;   // How far it roams from the player

    [Header("Movement")]
    public float moveSpeed = 2.0f;      // Normal roaming speed
    public float catchUpSpeed = 5.0f;   // Sprint speed when you run away
    public float changePosTime = 2.0f;  // How often it picks a new spot

    private Vector2 targetPosition;
    private float timer;

    void Start()
    {
        // Start by going to the player
        if (player != null) targetPosition = player.position;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. LOGIC: Are we inside the fence or outside?
        if (distanceToPlayer > fenceRadius)
        {
            // --- OUTSIDE FENCE (Catch Up) ---
            // Ignore wandering, just run directly to the player
            transform.position = Vector2.MoveTowards(transform.position, player.position, catchUpSpeed * Time.deltaTime);
        }
        else
        {
            // --- INSIDE FENCE (Roam Around) ---
            // Move towards our current random wander spot
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Timer to pick a NEW random spot
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                PickNewRandomPos();
                timer = changePosTime; // Reset timer
            }
        }
    }

    void PickNewRandomPos()
    {
        // Pick a random point inside a circle around the PLAYER
        // (Random.insideUnitCircle returns a point between 0,0 and 1,1)
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;

        targetPosition = (Vector2)player.position + randomOffset;
    }

    // Draw the "Fence" in the editor so you can see it
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // GREEN circle = The area it wanders in
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, wanderRadius);

            // RED circle = The absolute limit (The Geofence)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, fenceRadius);
        }
    }
}