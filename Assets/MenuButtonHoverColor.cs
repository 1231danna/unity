using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtonHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public TMP_Text targetText;

    public Color normalColor = new Color(0.12f, 0.08f, 0.03f);
    public Color hoverColor = Color.red;
    public Color pressedColor = new Color(0.45f, 0f, 0f);

    private void Reset()
    {
        targetText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>();

        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = hoverColor;
    }
}