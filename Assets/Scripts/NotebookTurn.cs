using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using echo17.EndlessBook;
using UnityEngine.SceneManagement;

public class NotebookTurn : MonoBehaviour
{
    [System.Serializable]
    public class StepUI
    {
        public GameObject rootGroup;
        public Button startBtn;
        public TextMeshProUGUI textMsg;
        public Button nextBtn;

        [Header("交互逻辑选择")]
        public bool isSceneJump;
        public Object sceneToLoad;
        public int pageToTurnTo;
    }

    [Header("核心引用")]
    public EndlessBook book;
    public SmoothInteractionCamera camScript;
    public Transform notebookAnchor;

    [Header("冲刺特效设置")]
    public CanvasGroup transitionOverlay; // 拖入那个黑屏Image
    public float dashMoveSpeed = 30f;     // 既然是急速，建议设大一点
    public float dashRotateSpeed = 20f;
    public float timeToBlack = 0.5f;      // 0.5秒撞完并黑屏

    [Header("交互步骤配置")]
    public List<StepUI> steps;

    private int currentIdx = 0;
    private bool isBookOpened = false;
    private bool isTyping = false;

    void Start()
    {
        // 初始确保黑屏是隐藏且透明的
        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 0f;
            transitionOverlay.gameObject.SetActive(false);
        }

        for (int i = 0; i < steps.Count; i++)
        {
            int index = i;
            StepUI s = steps[index];

            if (s.rootGroup != null) s.rootGroup.SetActive(false);
            if (s.startBtn != null)
                s.startBtn.onClick.AddListener(() => StartCoroutine(TypeEffect(s)));

            if (s.nextBtn != null)
            {
                if (s.isSceneJump)
                    s.nextBtn.onClick.AddListener(() => StartCoroutine(TransitionAndLoad(s.sceneToLoad)));
                else
                    s.nextBtn.onClick.AddListener(NextPageAction);
            }
        }
    }

    void Update()
    {
        if (camScript == null || book == null || notebookAnchor == null) return;

        float distToNotebook = Vector3.Distance(camScript.transform.position, notebookAnchor.position);

        if (!isBookOpened && distToNotebook < 0.1f)
        {
            isBookOpened = true;
            book.SetState(EndlessBook.StateEnum.OpenMiddle);
            StartCoroutine(ShowUIDelayed(0, 1.2f));
        }
    }

    // --- 核心转场逻辑 ---
    IEnumerator TransitionAndLoad(Object sceneObj)
    {
        if (sceneObj == null)
        {
            Debug.LogError("没有分配场景！");
            yield break;
        }

        // 1. 隐藏当前UI
        steps[currentIdx].rootGroup.SetActive(false);

        // 2. 设置相机冲刺目标为 AnchorTurn
        if (camScript != null)
        {
            // 寻找名为 AnchorTurn 的子物体
            Transform dashTarget = notebookAnchor.Find("AnchorTurn");

            if (dashTarget != null)
            {
                camScript.SetCameraFrozen(false);
                camScript.moveSpeed = dashMoveSpeed;
                camScript.rotateSpeed = dashRotateSpeed;
                camScript.targetAnchor = dashTarget; // 冲向深处的点
                Debug.Log("冲刺开始：目标 AnchorTurn");
            }
            else
            {
                Debug.LogWarning("未在 notebookAnchor 下找到名为 AnchorTurn 的物体！将直接黑屏。");
            }
        }

        // 3. 开启黑屏渐变
        if (transitionOverlay != null)
        {
            transitionOverlay.gameObject.SetActive(true);
            transitionOverlay.alpha = 0f;
        }

        float elapsed = 0;
        while (elapsed < timeToBlack)
        {
            elapsed += Time.deltaTime;
            if (transitionOverlay != null)
            {
                transitionOverlay.alpha = Mathf.Clamp01(elapsed / timeToBlack);
            }
            yield return null;
        }

        if (transitionOverlay != null) transitionOverlay.alpha = 1f;

        // 4. 跳转场景
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneObj.name);
    }

    // --- 以下为原有逻辑，保持不变 ---
    IEnumerator TypeEffect(StepUI s)
    {
        isTyping = true;
        s.startBtn.gameObject.SetActive(false);
        s.textMsg.gameObject.SetActive(true);
        s.textMsg.maxVisibleCharacters = 0;
        string fullText = s.textMsg.text;
        int totalCharacters = fullText.Length;
        float timer = 0f;
        float interval = 0.05f;
        int currentVisible = 0;

        while (currentVisible < totalCharacters)
        {
            if (Input.GetMouseButtonDown(0))
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
        yield return new WaitForSeconds(0.1f);
        isTyping = false;
    }

    void NextPageAction()
    {
        steps[currentIdx].rootGroup.SetActive(false);
        book.TurnToPage(steps[currentIdx].pageToTurnTo, (EndlessBook.PageTurnTimeTypeEnum)0, 1.0f, 0, null, null, null);
        currentIdx++;
        StartCoroutine(ShowUIDelayed(currentIdx, 1.2f));
    }

    IEnumerator ShowUIDelayed(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (index < steps.Count)
        {
            var s = steps[index];
            s.rootGroup.SetActive(true);
            s.startBtn.gameObject.SetActive(true);
            s.textMsg.gameObject.SetActive(false);
            s.nextBtn.gameObject.SetActive(false);
        }
    }
}