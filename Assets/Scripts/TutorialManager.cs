using UnityEngine;
using TMPro; 
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI 设置")]
    public CanvasGroup tutorialPanel;
    public TMP_Text tutorialText; 

    [Header("引导目标序列")]
    public HoverOutline[] tutorialTargets; 

    private int currentIndex = 0;
    private bool isWaitingForPhotoTransition = false; 

    void Start()
    {
        HideTutorial();
    }

    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
            StartCoroutine(FadeInTutorial());
        }
        
        // 激活列表里的第一个目标
        if (tutorialTargets.Length > 0) tutorialTargets[0].SetTutorialTarget(true);
    }

    public void ShowDetailedInstruction(string content)
    {
        if (tutorialText != null) tutorialText.text = content;
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
        
        // 标记：如果当前是工作板(索引0)，那么关掉对话框时就要触发高光切换
        if (currentIndex == 0) isWaitingForPhotoTransition = true;
    }

    public void CloseTutorial() 
    { 
        HideTutorial(); 

        // 如果之前是工作板阶段，关闭后自动切换到照片高光 (索引1)
        if (isWaitingForPhotoTransition)
        {
            SwitchToTarget(1); 
            isWaitingForPhotoTransition = false;
        }
    }

    public void SwitchToTarget(int index)
    {
        foreach (var target in tutorialTargets)
        {
            if (target != null) target.SetTutorialTarget(false);
        }
        currentIndex = index;
        if (currentIndex < tutorialTargets.Length && tutorialTargets[currentIndex] != null)
            tutorialTargets[currentIndex].SetTutorialTarget(true);
    }

    IEnumerator FadeInTutorial()
    {
        float elapsed = 0;
        while (elapsed < 0.5f) { elapsed += Time.deltaTime; tutorialPanel.alpha = elapsed / 0.5f; yield return null; }
        tutorialPanel.blocksRaycasts = true;
    }

    private void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0;
            tutorialPanel.blocksRaycasts = false;
            tutorialPanel.gameObject.SetActive(false);
        }

        foreach (HoverOutline target in tutorialTargets)
        {
            if (target != null)
            {
                target.SetTutorialTarget(false);
            }
        }
    }
}
