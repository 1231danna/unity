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
        public GameObject rootGroup;      // 这一步的UI父物体
        public Button startBtn;           // “阅读”按钮
        public TextMeshProUGUI textMsg;   // 文本内容
        public Button nextBtn;            // “下一步”按钮

        [Header("交互逻辑选择")]
        public bool isSceneJump;          // 勾选这个，点击nextBtn就跳场景

        [Tooltip("直接把场景文件拖到这里")]
        public Object sceneToLoad;        // 支持直接拖拽场景文件！

        [Tooltip("如果不跳场景，书会翻到这一页")]
        public int pageToTurnTo;
    }

    [Header("核心引用")]
    public EndlessBook book;
    public SmoothInteractionCamera camScript;
    public Transform notebookAnchor;

    [Header("交互步骤配置")]
    public List<StepUI> steps;

    private int currentIdx = 0;
    private bool isBookOpened = false;
    private bool isTyping = false;

    void Start()
    {
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
                    s.nextBtn.onClick.AddListener(() => LoadSpecificScene(s.sceneToLoad));
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

    // --- 核心改动：自动识别拖进去的场景名字 ---
    void LoadSpecificScene(Object sceneObj)
    {
        if (sceneObj != null)
        {
            Debug.Log("准备跳转场景: " + sceneObj.name);
            SceneManager.LoadScene(sceneObj.name);
        }
        else
        {
            Debug.LogError("你勾选了跳转，但没拖场景文件进去！");
        }
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