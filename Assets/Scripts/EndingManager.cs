using UnityEngine;
using UnityEngine.UI; // 必须引用 UI
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("UI 引用")]
    public CanvasGroup endingPanel;
    public CanvasGroup textCanvasGroup;
    public RectTransform endingText;

    [Header("显示大图的 UI 组件")]
    [Tooltip("把那个用来放大显示档案图片的 Image 组件拖进来")]
    public Image displayImage;

    [Header("终章判定素材")]
    [Tooltip("把那张【终章大图】的 Sprite 拖到这里")]
    public Sprite finalArchiveSprite;

    [Header("效果参数")]
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

    // --- 通用关闭按钮绑定此函数 (无需任何参数) ---
    public void TryStartEnding()
    {
        // 核心判定：检查当前 UI 显示的 Sprite 是不是我们指定的终章 Sprite
        if (displayImage != null && displayImage.sprite == finalArchiveSprite)
        {
            StartCoroutine(PlayEnding());
        }
        else
        {
            Debug.Log("检测到关闭动作，但当前大图不是终章素材。");
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
        // 游戏结束，保持全黑
    }
}