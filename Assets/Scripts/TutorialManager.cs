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
        if (tutorialPanel != null) { tutorialPanel.alpha = 0; tutorialPanel.blocksRaycasts = false; }
    }

    public void ShowTutorial()
    {
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
        if (tutorialTargets.Length > 0 && tutorialTargets[0] != null) 
            tutorialTargets[0].SetTutorialTarget(true);
    }

    public void ShowDetailedInstruction(string content)
    {
        if (tutorialText != null) tutorialText.text = content;
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
        
        // 如果当前索引为0(工作板)，点击关闭时触发高光切换
        if (currentIndex == 0) isWaitingForPhotoTransition = true;
    }

    public void CloseTutorial() 
    { 
        HideTutorial(); 
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
        if (tutorialPanel != null) { tutorialPanel.alpha = 0; tutorialPanel.blocksRaycasts = false; }
    }
}