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
        public GameObject rootGroup;      // 这一步UI的父物体
        public TextMeshProUGUI textMsg;   // 显示的文本内容
        public Button nextBtn;            // “下一步”跳转按钮

        [Tooltip("直接把场景文件拖到这里")]
        public Object sceneToLoad;
    }

    [Header("核心引用")]
    public SmoothInteractionCamera camScript;
    public Transform notebookAnchor;

    [Header("冲刺特效设置")]
    public CanvasGroup transitionOverlay; // 拖入那个黑屏Image
    public float dashMoveSpeed = 40f;     // 冲刺速度（建议加大，更有撞击感）
    public float dashRotateSpeed = 30f;
    public float timeToBlack = 0.5f;      // 0.5秒撞完并黑屏

    [Header("交互步骤配置")]
    public List<StepUI> steps;

    private int currentIdx = 0;

    void Start()
    {
        // 初始确保黑屏是隐藏且透明的
        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 0f;
            transitionOverlay.gameObject.SetActive(false);
        }

        // 初始化UI显示状态与按钮监听
        for (int i = 0; i < steps.Count; i++)
        {
            int index = i;
            StepUI s = steps[index];

            if (s.rootGroup != null) s.rootGroup.SetActive(index == 0); // 默认只显示第一步

            if (s.nextBtn != null)
            {
                s.nextBtn.onClick.AddListener(() => StartCoroutine(TransitionAndLoad(s.sceneToLoad)));
            }
        }
    }

    // --- 核心转场逻辑：急速冲向 AnchorTurn 并黑屏 ---
    IEnumerator TransitionAndLoad(Object sceneObj)
    {
        if (sceneObj == null)
        {
            Debug.LogError("没有分配场景！");
            yield break;
        }

        // 1. 隐藏当前UI，防止连点
        if (steps[currentIdx].rootGroup != null)
            steps[currentIdx].rootGroup.SetActive(false);

        // 2. 设置相机冲刺目标为 AnchorTurn
        if (camScript != null)
        {
            // 在 notebookAnchor 下寻找你创建的那个“深处”锚点 AnchorTurn
            Transform dashTarget = notebookAnchor.Find("AnchorTurn");

            if (dashTarget != null)
            {
                camScript.SetCameraFrozen(false); // 解除相机锁定
                camScript.moveSpeed = dashMoveSpeed; // 覆盖原始速度
                camScript.rotateSpeed = dashRotateSpeed;
                camScript.targetAnchor = dashTarget; // 冲向深处的点
                Debug.Log("冲刺开始：目标 AnchorTurn");
            }
            else
            {
                Debug.LogWarning("未在 notebookAnchor 下找到名为 AnchorTurn 的子物体！");
                // 如果没找到，退而求其次冲向笔记本锚点
                camScript.targetAnchor = notebookAnchor;
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
}