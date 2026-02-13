using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // assign the active player manually or via script

    [Header("Camera Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Auto-Target Active Player")]
    public bool autoFindTarget = true;

    void LateUpdate()
    {
        // auto find the active player if enabled
        if (autoFindTarget)
        {
            FindActivePlayer();
        }

        if (target == null) return;

        // smoothly move camera to follow target
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void FindActivePlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            SwitchCharacters sc = player.GetComponent<SwitchCharacters>();
            if (sc != null && sc.isActive)
            {
                target = player.transform;
                return;
            }
        }
    }
}