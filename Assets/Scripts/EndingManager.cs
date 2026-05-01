using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public CanvasGroup endingPanel;
    public CanvasGroup textCanvasGroup;
    public RectTransform endingText;
    public Image displayImage;
    public Sprite finalArchiveSprite;

    public float fadeInTime = 1.5f;
    public float scrollSpeed = 250f;
    public float stayTime = 4.0f;
    public float textFadeOutTime = 2.0f;
    public float startYPosition = -400f;

    private void Start()
    {
        if (endingPanel != null)
        {
            endingPanel.alpha = 0;
            endingPanel.gameObject.SetActive(false);
        }
    }

    public void TryStartEnding()
    {
        if (displayImage != null && displayImage.sprite == finalArchiveSprite)
        {
            StartCoroutine(PlayEnding());
        }
        else
        {
            Debug.Log("Not this Image");
        }
    }

    private IEnumerator PlayEnding()
    {
        endingText.anchoredPosition = new Vector2(0, startYPosition);
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;

        endingPanel.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / fadeInTime;
            endingPanel.alpha = t;
            yield return null;
        }

        while (endingText.anchoredPosition.y < 0)
        {
            endingText.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            yield return null;
        }
        endingText.anchoredPosition = new Vector2(0, 0);

        yield return new WaitForSeconds(stayTime);

        if (textCanvasGroup != null)
        {
            t = 1;
            while (t > 0)
            {
                t -= Time.deltaTime / textFadeOutTime;
                textCanvasGroup.alpha = t;
                yield return null;
            }
        }
    }
}