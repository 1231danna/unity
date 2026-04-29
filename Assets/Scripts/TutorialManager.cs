using UnityEngine;
using TMPro;
using System.Collections;

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

    public void ShowTutorial()
    {
        currentIndex = 0;
        if (tutorialSteps.Length > 0)
        {
            ShowDetailedInstruction(tutorialSteps[0].dialogueText);
        }
    }

    public void ShowNextStep()
    {
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

    public void CloseTutorial()
    {
        HideTutorial();
        
        if (currentIndex >= 0 && currentIndex < tutorialSteps.Length)
        {
            HoverOutline nextTarget = tutorialSteps[currentIndex].targetToHighlight;
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
    // 这个就是 PanelController 找不到的那个方法，现在加进来了！
    public bool CanInteractWith(GameObject obj)
    {
        // 1. 如果对话框正在显示，严禁点击后面的任何 3D 物体
        if (tutorialPanel != null && tutorialPanel.blocksRaycasts)
        {
            return false;
        }

        // 2. 如果当前有正在发光的高光目标，严格限制：【只能点它，别的全拦截】
        if (currentActiveTarget != null)
        {
            // 检查：被点击的物体是不是目标本身，或者是不是目标底下的子物体
            bool isTarget = (obj == currentActiveTarget.gameObject) || 
                            (obj.transform.IsChildOf(currentActiveTarget.transform));
            
            return isTarget;
        }

        // 3. 如果指引已经全部结束，或者当前步骤本来就没安排高光，则允许自由探索
        return true;
    }
}