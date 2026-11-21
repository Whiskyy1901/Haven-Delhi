using System.Collections;
using UnityEngine;

public class SpawnPop : MonoBehaviour
{
    public void Play(float duration)
    {
        StartCoroutine(Pop(Mathf.Max(0.01f, duration)));
    }

    private IEnumerator Pop(float duration)
    {
        Vector3 targetScale = transform.localScale;
        Enemy enemy = GetComponent<Enemy>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (enemy != null) enemy.enabled = false;
        if (rb != null) rb.simulated = false;

        transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = targetScale * EaseOutBack(Mathf.Clamp01(t));
            yield return null;
        }

        transform.localScale = targetScale;

        if (rb != null) rb.simulated = true;
        if (enemy != null) enemy.enabled = true;

        Destroy(this);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.7f;
        const float c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}