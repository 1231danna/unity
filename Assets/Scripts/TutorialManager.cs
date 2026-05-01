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
    [Header("启动设置 (场景2请勾选前两项)")]
    [Tooltip("勾选此项，场景一运行会自动等待0.5秒后触发第一步")]
    public bool autoStartTutorial = false;

    [Tooltip("勾选此项，将跳过所有对话框，直接进行高光无缝指引")]
    public bool isSilentMode = false;

    [Header("UI 设置")]
    public CanvasGroup tutorialPanel;
    public TMP_Text tutorialText;

    [Header("引导流程配置")]
    [Tooltip("按顺序配置每个步骤的文字和高光目标")]
    public TutorialStep[] tutorialSteps;

    private int currentIndex = -1;
    private HoverOutline currentActiveTarget;

    void Start()
    {
        if (tutorialPanel != null)
        {
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

        if (autoStartTutorial)
        {
            StartCoroutine(DelayedStart());
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        ShowTutorial();
    }

    public void ShowTutorial()
    {
        currentIndex = 0;
        if (tutorialSteps.Length > 0)
        {
            if (isSilentMode)
            {
                ActivateTarget(currentIndex);
            }
            else
            {
                ShowDetailedInstruction(tutorialSteps[0].dialogueText);
            }
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
            if (isSilentMode)
            {
                ActivateTarget(currentIndex);
            }
            else
            {
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
        ActivateTarget(currentIndex);
    }

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