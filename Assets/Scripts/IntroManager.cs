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
    // 0.05 是非常快的语速，0.03 就会快得像残影
    [Range(0.01f, 0.2f)]
    public float baseTypingSpeed = 0.05f;
    public float fadeDuration = 1.0f; // 既然快，淡出也快一点，干脆利落

    [Header("组件引用")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public SmoothInteractionCamera camScript;

    private string currentVisibleText = "";
    private bool isTypingFinished = false;

    void Start()
    {
        if (camScript != null) camScript.SetCameraFrozen(true);
        if (filmGrainImage != null) filmGrainImage.raycastTarget = false;

        introGroup.alpha = 1f;
        introText.text = "";

        StartCoroutine(BlinkCursor());
        StartCoroutine(PlayIntro());
        StartCoroutine(FilmGrainRoutine());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.5f); // 缩短开场等待

        foreach (string line in storyLines)
        {
            currentVisibleText = "";
            for (int i = 0; i < line.Length; i++)
            {
                currentVisibleText += line[i];

                if (audioSource != null && typeSound != null)
                {
                    // 速度快了，音调可以稍微高一点，显得清脆
                    audioSource.pitch = Random.Range(1.0f, 1.15f);
                    audioSource.PlayOneShot(typeSound, 0.6f); // 音量稍减，防止高频刺耳
                }

                // --- 极速节奏控制 ---
                float delay = baseTypingSpeed;

                // 遇到标点：短暂停顿，不要断了节奏感
                if ("，。！？…".Contains(line[i].ToString()))
                {
                    delay = 0.3f;
                }
                // 只有很小的概率（5%）会稍微迟疑一下，保证大部分时间是连打
                else if (Random.value > 0.95f)
                {
                    delay = baseTypingSpeed * 3.0f;
                }
                // 平时基本保持匀速，通过 0.01s 的微差消除机械感
                else
                {
                    delay = Random.Range(baseTypingSpeed * 0.9f, baseTypingSpeed * 1.1f);
                }

                yield return new WaitForSeconds(delay);
            }
            yield return new WaitForSeconds(1.0f); // 换行也快点
        }

        isTypingFinished = true;

        // 快速淡出，1秒结束战斗
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

    // 噪点跳动变快，增加动态感
    IEnumerator FilmGrainRoutine()
    {
        while (introGroup.alpha > 0)
        {
            filmGrainImage.color = new Color(1, 1, 1, Random.Range(0.04f, 0.12f));
            filmGrainTransform.anchoredPosition = new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
            filmGrainTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
            yield return new WaitForSeconds(0.06f); // 约16帧，更流畅
        }
    }

    IEnumerator BlinkCursor()
    {
        while (!isTypingFinished)
        {
            introText.text = currentVisibleText + "|";
            yield return new WaitForSeconds(0.2f); // 光标闪烁变快
            introText.text = currentVisibleText + " ";
            yield return new WaitForSeconds(0.2f);
        }
        introText.text = currentVisibleText;
    }
}