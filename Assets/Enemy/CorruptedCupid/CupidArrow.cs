using UnityEngine;

public class CupidArrow : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 5f;
    public float debuffDuration = 2f;

    Vector2 direction;
    DebuffType debuffType;

    public void Initialize(Vector2 dir)
    {
        direction = dir;
        Destroy(gameObject, lifetime);
    }

    public void SetDebuff(DebuffType type)
    {
        debuffType = type;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDebuffable debuffable = other.GetComponent<IDebuffable>();
        if (debuffable != null)
        {
            debuffable.ApplyDebuff(debuffType, debuffDuration);
            Destroy(gameObject);
        }
    }
}
