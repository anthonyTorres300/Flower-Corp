using UnityEngine;

public class CorruptedCupid : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float patrolDistance = 2f;

    [Header("Combat")]
    public float attackRange = 6f;
    public float attackCooldown = 2.5f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    Vector2 startPos;
    bool movingRight = true;
    float cooldown;
    Transform target;

    void Start()
    {
        startPos = transform.position;
        target = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackRange && cooldown <= 0f)
        {
            Shoot();
            cooldown = attackCooldown;
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        float dir = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * dir * moveSpeed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= patrolDistance)
            movingRight = !movingRight;
    }

    void Shoot()
    {
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        Vector2 dir = (target.position - firePoint.position).normalized;
        CupidArrow arrowScript = arrow.GetComponent<CupidArrow>();

        arrowScript.Initialize(dir);
        arrowScript.SetDebuff(RandomDebuff());
    }

    DebuffType RandomDebuff()
    {
        int roll = Random.Range(0, 3);
        return (DebuffType)roll;
    }
}
