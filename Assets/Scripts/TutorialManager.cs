using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 5)]
    [Tooltip("这段引导的文字内容")]
    public string dialogueText; 
    
    [Tooltip("这段对话【关闭后】（或静默模式下直接）需要亮起高光的物体。如果没有就留空。")]
    public HoverOutline targetToHighlight;
}

public class TutorialManager : MonoBehaviour
{
    [Header("模式设置")]
    [Tooltip("勾选此项，将跳过所有对话框，直接进行高光无缝指引（场景2专用）")]
    public bool isSilentMode = false;

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
            // 如果是静默模式，直接把 UI 彻底关掉
            if (isSilentMode)
            {
                tutorialPanel.gameObject.SetActive(false);
            }
            else
            {
                tutorialPanel.gameObject.SetActive(true);
                tutorialPanel.alpha = 0;
                tutorialPanel.blocksRaycasts = false;
            }
        }
    }

    public void ShowTutorial()
    {
        currentIndex = 0;
        if (tutorialSteps.Length > 0)
        {
            if (isSilentMode)
            {
                // 静默模式：不弹文字，直接亮起第 0 个高光（工作板）
                ActivateTarget(currentIndex);
            }
            else
            {
                // 正常模式：弹第一段文字
                ShowDetailedInstruction(tutorialSteps[0].dialogueText);
            }
        }
    }

    public void ShowNextStep()
    {
        // 无论什么模式，进入下一步时先关掉当前的高光
        if (currentActiveTarget != null)
        {
            currentActiveTarget.SetTutorialTarget(false);
            currentActiveTarget = null;
        }

        currentIndex++;
        if (currentIndex < tutorialSteps.Length)
        {
            if (isSilentMode)
            {
                // 静默模式：跳过文字，直接亮起下一个高光
                ActivateTarget(currentIndex);
            }
            else
            {
                // 正常模式：弹出下一段文字
                ShowDetailedInstruction(tutorialSteps[currentIndex].dialogueText);
            }
        }
    }

    private void ShowDetailedInstruction(string content)
    {
        if (tutorialText != null) tutorialText.text = content;
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
    }

    public void CloseTutorial()
    {
        HideTutorial();
        
        // 正常模式下，关闭对话框时亮起高光
        ActivateTarget(currentIndex);
    }

    // 提取出一个专门用来亮起高光的方法，方便两种模式复用
    private void ActivateTarget(int index)
    {
        if (index >= 0 && index < tutorialSteps.Length)
        {
            HoverOutline nextTarget = tutorialSteps[index].targetToHighlight;
            if (nextTarget != null)
            {
                nextTarget.SetTutorialTarget(true);
                currentActiveTarget = nextTarget; 
            }
        }
    }

    IEnumerator FadeInTutorial()
    {
        float elapsed = 0;
        tutorialPanel.blocksRaycasts = true; 
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

    // --- 门卫机制：判断某个物体当前是否允许被交互 ---
    public bool CanInteractWith(GameObject obj)
    {
        if (tutorialPanel != null && tutorialPanel.blocksRaycasts)
        {
            return false;
        }

        if (currentActiveTarget != null)
        {
            bool isTarget = (obj == currentActiveTarget.gameObject) || 
                            (obj.transform.IsChildOf(currentActiveTarget.transform));
            return isTarget;
        }

        return true;
    }
}