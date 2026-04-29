using UnityEngine;
using TMPro;
using System.Collections;

// 新增：将文字和对应的高光目标绑定在一起
[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 5)]
    [Tooltip("这段引导的文字内容")]
    public string dialogueText; 
    
    [Tooltip("这段对话【关闭后】，需要亮起高光的物体。如果没有就留空。")]
    public HoverOutline targetToHighlight;
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI 设置")]
    public CanvasGroup tutorialPanel;
    public TMP_Text tutorialText;

    [Header("引导流程配置")]
    [Tooltip("按顺序配置每个步骤的文字和高光目标")]
    public TutorialStep[] tutorialSteps; 

    private int currentIndex = -1;
    private HoverOutline currentActiveTarget; // 记录当前亮起的高光物体

    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
            tutorialPanel.alpha = 0;
            tutorialPanel.blocksRaycasts = false;
        }
    }

    // 1. 开场动画调用
    public void ShowTutorial()
    {
        currentIndex = 0;
        if (tutorialSteps.Length > 0)
        {
            ShowDetailedInstruction(tutorialSteps[0].dialogueText);
        }
    }

    // 2. 推进到下一步 (点击物体 / 关闭面板时调用)
    public void ShowNextStep()
    {
        // 弹出新对话前，强制关闭当前正亮着的高光
        if (currentActiveTarget != null)
        {
            currentActiveTarget.SetTutorialTarget(false);
            currentActiveTarget = null;
        }

        currentIndex++;
        if (currentIndex < tutorialSteps.Length)
        {
            ShowDetailedInstruction(tutorialSteps[currentIndex].dialogueText);
        }
    }

    private void ShowDetailedInstruction(string content)
    {
        if (tutorialText != null) tutorialText.text = content;
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
    }

    // 3. 绑定到 UI 对话框上的 [关闭/Next按钮]
    public void CloseTutorial()
    {
        HideTutorial();
        
        // 【核心逻辑】：对话框关闭时，激活当前步骤配对的高光物体
        if (currentIndex >= 0 && currentIndex < tutorialSteps.Length)
        {
            HoverOutline nextTarget = tutorialSteps[currentIndex].targetToHighlight;
            if (nextTarget != null)
            {
                nextTarget.SetTutorialTarget(true);
                currentActiveTarget = nextTarget; // 记录下来，方便下一步关闭
            }
        }
    }

    IEnumerator FadeInTutorial()
    {
        float elapsed = 0;
        tutorialPanel.blocksRaycasts = true; // 允许点击关闭按钮
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            tutorialPanel.alpha = elapsed / 0.5f;
            yield return null;
        }
    }

    private void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0;
            tutorialPanel.blocksRaycasts = false;
        }
    }
}