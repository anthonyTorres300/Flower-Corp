using UnityEngine;

public class CupidFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float followDistance = 2f;
    public float followSpeed = 4f;
    public float floatHeight = 1.5f;
    public float floatSpeed = 2f;

    [Header("Cupid Data")]
    public CupidData cupidData;

    [Header("Auto Find Target")]
    public bool autoFindActivePlayer = true;

    private Vector3 baseOffset;
    private float floatTimer;

    void Start()
    {
        baseOffset = new Vector3(followDistance, floatHeight, 0);
        floatTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (autoFindActivePlayer)
        {
            FindActivePlayer();
        }

        if (target == null) return;

        FollowPlayer();
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

    void FollowPlayer()
    {
        // bobbing float
        floatTimer += Time.deltaTime * floatSpeed;
        float floatOffset = Mathf.Sin(floatTimer) * 0.3f;

        // position behind and above player
        Vector3 targetPosition = target.position + baseOffset + Vector3.up * floatOffset;

        // smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // face movement direction
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}