using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Visualization")]
    public Color gizmoColor = Color.red;
    public float gizmoSize = 1f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize * 2);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}