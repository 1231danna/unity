using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NotebookTurn : MonoBehaviour
{
    [System.Serializable]
    public class StepUI
    {
        public GameObject rootGroup;      // UI 父物体
        public Button startBtn;           // “阅读”按钮
        public TextMeshProUGUI textMsg;   // 文本内容
        public Button nextBtn;            // “下一步/跳转”按钮

        [Header("场景跳转设置")]
        public bool isSceneJump;
        public Object sceneToLoad;
    }

    [Header("核心引用")]
    public SmoothInteractionCamera camScript;
    public Transform notebookAnchor;

    [Header("冲刺转场设置")]
    public CanvasGroup transitionOverlay;
    public float dashMoveSpeed = 40f;
    public float dashRotateSpeed = 30f;
    public float timeToBlack = 0.5f;

    [Header("交互步骤配置")]
    public List<StepUI> steps;

    private int currentIdx = 0;
    private bool isTyping = false;

    void Start()
    {
        // 1. 初始化黑屏状态
        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 0f;
            transitionOverlay.gameObject.SetActive(false);
        }

        // 2. 初始化所有步骤的 UI 和按钮
        for (int i = 0; i < steps.Count; i++)
        {
            int index = i;
            StepUI s = steps[index];

            // 确保第一组显示，其他隐藏
            if (s.rootGroup != null) s.rootGroup.SetActive(index == 0);

            // 初始隐藏文本和 Next 按钮（等待点击阅读）
            if (s.textMsg != null) s.textMsg.gameObject.SetActive(false);
            if (s.nextBtn != null) s.nextBtn.gameObject.SetActive(false);

            // 绑定阅读按钮
            if (s.startBtn != null)
            {
                s.startBtn.gameObject.SetActive(true);
                s.startBtn.onClick.RemoveAllListeners(); // 防止重复绑定
                s.startBtn.onClick.AddListener(() => StartCoroutine(TypeEffect(s)));
            }

            // 绑定 Next 按钮
            if (s.nextBtn != null)
            {
                s.nextBtn.onClick.RemoveAllListeners();
                if (s.isSceneJump)
                {
                    // 如果是跳转场景，绑定冲刺协程
                    s.nextBtn.onClick.AddListener(() => {
                        Debug.Log("点击了跳转按钮");
                        StartCoroutine(TransitionAndLoad(s.sceneToLoad));
                    });
                }
                else
                {
                    // 如果不是，切换到下一组 UI
                    s.nextBtn.onClick.AddListener(NextStepAction);
                }
            }
        }
    }

    // 文字打字机效果
    IEnumerator TypeEffect(StepUI s)
    {
        if (isTyping) yield break;
        isTyping = true;

        s.startBtn.gameObject.SetActive(false);
        s.textMsg.gameObject.SetActive(true);
        s.textMsg.maxVisibleCharacters = 0;

        string fullText = s.textMsg.text;
        int totalCharacters = fullText.Length;
        int currentVisible = 0;
        float timer = 0f;
        float interval = 0.05f;

        while (currentVisible < totalCharacters)
        {
            if (Input.GetMouseButtonDown(0)) // 左键跳过
            {
                s.textMsg.maxVisibleCharacters = totalCharacters;
                break;
            }
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                currentVisible++;
                s.textMsg.maxVisibleCharacters = currentVisible;
            }
            yield return null;
        }

        s.nextBtn.gameObject.SetActive(true);
        isTyping = false;
    }

    // 切换到下一组 UI
    void NextStepAction()
    {
        if (currentIdx < steps.Count)
            steps[currentIdx].rootGroup.SetActive(false);

        currentIdx++;

        if (currentIdx < steps.Count)
        {
            var s = steps[currentIdx];
            s.rootGroup.SetActive(true);
            s.startBtn.gameObject.SetActive(true);
            s.textMsg.gameObject.SetActive(false);
            s.nextBtn.gameObject.SetActive(false);
        }
    }

    // 急速冲刺 + 黑屏转场
    IEnumerator TransitionAndLoad(Object sceneObj)
    {
        if (sceneObj == null)
        {
            Debug.LogError("未在 Steps 中分配跳转场景！");
            yield break;
        }

        Debug.Log("开始冲刺转场...");

        // 1. 隐藏当前步骤 UI
        if (currentIdx < steps.Count)
            steps[currentIdx].rootGroup.SetActive(false);

        // 2. 相机加速并指向 AnchorTurn
        if (camScript != null)
        {
            Transform dashTarget = notebookAnchor.Find("AnchorTurn");
            camScript.SetCameraFrozen(false);
            camScript.moveSpeed = dashMoveSpeed;
            camScript.rotateSpeed = dashRotateSpeed;
            camScript.targetAnchor = (dashTarget != null) ? dashTarget : notebookAnchor;
        }

        // 3. 黑屏淡入
        if (transitionOverlay != null)
        {
            transitionOverlay.gameObject.SetActive(true);
            float elapsed = 0;
            while (elapsed < timeToBlack)
            {
                elapsed += Time.deltaTime;
                transitionOverlay.alpha = Mathf.Clamp01(elapsed / timeToBlack);
                yield return null;
            }
            transitionOverlay.alpha = 1f;
        }

        // 4. 加载场景
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneObj.name);
    }
}