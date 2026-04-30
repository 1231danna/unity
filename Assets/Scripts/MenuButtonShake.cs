using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonShake : MonoBehaviour, IPointerDownHandler
{
    public float shakeTime = 0.12f;
    public float shakeStrength = 6f;
    public float shakeSpeed = 80f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeTime)
        {
            float offsetX = Mathf.Sin(timer * shakeSpeed) * shakeStrength;
            rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, 0f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }
}