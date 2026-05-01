using System.Collections;
using UnityEngine;

public class MenuSlideIn : MonoBehaviour
{
    public RectTransform panel;

    [Header("Slide Settings")]
    public float startOffsetX = 700f;
    public float duration = 2.2f;
    public float delay = 0.4f;

    private Vector2 targetPos;

    private void Awake()
    {
        if (panel == null)
            panel = GetComponent<RectTransform>();

        targetPos = panel.anchoredPosition;
        panel.anchoredPosition = targetPos + Vector2.left * startOffsetX;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(delay);

        float timer = 0f;
        Vector2 startPos = panel.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            
            t = EaseOutCubic(t);

            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        panel.anchoredPosition = targetPos;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}