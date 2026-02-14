using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}