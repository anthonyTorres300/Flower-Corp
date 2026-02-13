using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float smoothSpeed = 0.125f; 
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    private Rigidbody2D rb;
    private Camera cam;

    Vector2 movement;
    Vector2 mousePos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Only apply movement if input is strong enough (ignore tiny drift)
        if (Mathf.Abs(x) > 0.1f) movement.x = x; else movement.x = 0;
        if (Mathf.Abs(y) > 0.1f) movement.y = y; else movement.y = 0;

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = -cam.transform.position.z; // Set distance from camera to world
            mousePos = cam.ScreenToWorldPoint(screenPos);
        }
    }
}