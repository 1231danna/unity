using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public CanvasGroup introGroup;
    public TMP_Text introText;
    public RectTransform filmGrainTransform;
    public Image filmGrainImage;

    [TextArea(3, 5)]
    public string[] storyLines;

    [Range(0.01f, 0.2f)]
    public float baseTypingSpeed = 0.05f;
    public float fadeDuration = 1.0f;

    public AudioSource audioSource;
    public AudioClip typeSound;
    public SmoothInteractionCamera camScript;

    public AudioClip mainBGM;

    private string currentVisibleText = "";
    private bool isTypingFinished = false;

    // --- 【开发备注：开始】 不需要跳过功能时，可以删除下面这一行 ---
    private bool skipRequested = false;
    // --- 【开发备注：结束】 ---

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

    // --- 【开发备注：开始】 不需要跳过功能时，可以删除整个 Update 函数 ---
    void Update()
    {
        // 检测鼠标左键点击，设置为请求跳过
        if (Input.GetMouseButtonDown(0))
        {
            skipRequested = true;
        }
    }
    // --- 【开发备注：结束】 ---

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.8f);

        foreach (string line in storyLines)
        {
            // --- 【开发备注：开始】 不需要跳过功能时，可以删除下面这一行 ---
            if (skipRequested) break;
            // --- 【开发备注：结束】 ---

            currentVisibleText = "";
            for (int i = 0; i < line.Length; i++)
            {
                // --- 【开发备注：开始】 不需要跳过功能时，可以删除下面这一行 ---
                if (skipRequested) break;
                // --- 【开发备注：结束】 ---

                currentVisibleText += line[i];

                if (i % 5 == 0 && audioSource != null && typeSound != null)
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f);
                    audioSource.PlayOneShot(typeSound, 0.7f);
                }

                float delay = baseTypingSpeed;
                if ("，。！？…".Contains(line[i].ToString())) delay = 0.35f;
                else if (Random.value > 0.95f) delay = baseTypingSpeed * 2.5f;
                else delay = Random.Range(baseTypingSpeed * 0.9f, baseTypingSpeed * 1.1f);

                yield return new WaitForSeconds(delay);
            }

            // --- 【开发备注：开始】 修改：增加对跳过的判断，防止跳过时产生不必要的停顿 ---
            if (!skipRequested) yield return new WaitForSeconds(1.2f);
            // --- 【开发备注：结束】 ---
        }

        isTypingFinished = true;

        // --- 【开发备注：开始】 修改：跳过时让黑屏瞬间消失 (0.1秒)，否则使用原定淡出时间 ---
        float actualFadeDuration = skipRequested ? 0.1f : fadeDuration;
        // --- 【开发备注：结束】 ---

        float elapsed = 0;
        while (elapsed < actualFadeDuration)
        {
            elapsed += Time.deltaTime;
            introGroup.alpha = 1 - (elapsed / actualFadeDuration);
            yield return null;
        }

        if (camScript != null) camScript.SetCameraFrozen(false);
        introGroup.blocksRaycasts = false;
        
        if (AudioManager.Instance != null && mainBGM != null)
        {
            AudioManager.Instance.PlayBGM(mainBGM);
        }

        gameObject.SetActive(false);

        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null)
        {
            tm.ShowTutorial();
        }
        else
        {
            Debug.LogWarning("cant find tutorial.");
        }
    }

    IEnumerator FilmGrainRoutine()
    {
        while (introGroup.alpha > 0)
        {
            filmGrainImage.color = new Color(1, 1, 1, Random.Range(0.03f, 0.08f));
            filmGrainTransform.anchoredPosition = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            filmGrainTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
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