using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("UI 引用")]
    public CanvasGroup introGroup;
    public TMP_Text introText;
    public RectTransform filmGrainTransform;
    public Image filmGrainImage;

    [Header("文字内容")]
    [TextArea(3, 5)]
    public string[] storyLines;

    [Header("打字节奏")]
    [Range(0.01f, 0.2f)]
    public float baseTypingSpeed = 0.05f; // 文字蹦出的基础速度
    public float fadeDuration = 1.0f;     // 最后黑屏隐去的时间

    [Header("组件引用")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public SmoothInteractionCamera camScript;

    private string currentVisibleText = "";
    private bool isTypingFinished = false;

    void Start()
    {
        // 初始设置
        if (camScript != null) camScript.SetCameraFrozen(true);
        if (filmGrainImage != null) filmGrainImage.raycastTarget = false;

        introGroup.alpha = 1f;
        introText.text = "";

        // 启动三大核心逻辑
        StartCoroutine(BlinkCursor());
        StartCoroutine(PlayIntro());
        StartCoroutine(FilmGrainRoutine());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.8f);

        foreach (string line in storyLines)
        {
            currentVisibleText = "";
            for (int i = 0; i < line.Length; i++)
            {
                currentVisibleText += line[i];

                // --- 关键优化：解决声音太快的问题 ---
                // i % 2 == 0 意味着每两个字响一次声音。
                // 这样文字虽然出得快，但声音节奏是稳定的“哒、哒、哒”，不会挤在一起。
                if (i % 5 == 0 && audioSource != null && typeSound != null)
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f); // 极小范围抖动，保持稳重
                    audioSource.PlayOneShot(typeSound, 0.7f);
                }

                // --- 极速打字节奏控制 ---
                float delay = baseTypingSpeed;

                // 遇到标点：稍微停顿，给眼睛反应时间
                if ("，。！？…".Contains(line[i].ToString()))
                {
                    delay = 0.35f;
                }
                // 5% 的小概率模拟细微的机械迟滞
                else if (Random.value > 0.95f)
                {
                    delay = baseTypingSpeed * 2.5f;
                }
                else
                {
                    delay = Random.Range(baseTypingSpeed * 0.9f, baseTypingSpeed * 1.1f);
                }

                yield return new WaitForSeconds(delay);
            }
            yield return new WaitForSeconds(1.2f); // 换行停留
        }

        isTypingFinished = true;

        // 淡出黑屏
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            introGroup.alpha = 1 - (elapsed / fadeDuration);
            yield return null;
        }

        if (camScript != null) camScript.SetCameraFrozen(false);
        introGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    // 噪点跳动协程：已经调慢了，不再晃眼
    IEnumerator FilmGrainRoutine()
    {
        while (introGroup.alpha > 0)
        {
            // 降低透明度上限，看起来更舒服
            filmGrainImage.color = new Color(1, 1, 1, Random.Range(0.03f, 0.08f));
            // 减小位移范围，防止图片乱跑
            filmGrainTransform.anchoredPosition = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            filmGrainTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);

            // 关键：0.15s 跳一次，非常稳定的老胶片感
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator BlinkCursor()
    {
        while (!isTypingFinished)
        {
            introText.text = currentVisibleText + "|";
            yield return new WaitForSeconds(0.3f);
            introText.text = currentVisibleText + " ";
            yield return new WaitForSeconds(0.3f);
        }
        introText.text = currentVisibleText;
    }
}
