using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI 设置")]
    public CanvasGroup tutorialPanel;

    [Header("引导目标序列 (在面板里拖入工作板、书本等)")]
    public HoverOutline[] tutorialTargets; 

    private int currentIndex = 0;

    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0;
            tutorialPanel.blocksRaycasts = false;
        }
    }

    public void ShowTutorial()
    {
        if (tutorialPanel != null) StartCoroutine(FadeInTutorial());
        
        // 激活列表里的第一个目标
        if (tutorialTargets.Length > 0) tutorialTargets[0].SetTutorialTarget(true);
    }

    public void MoveToNextTarget()
    {
        // 关掉当前目标
        if (currentIndex < tutorialTargets.Length) 
            tutorialTargets[currentIndex].SetTutorialTarget(false);

        currentIndex++;

        // 开启下一个目标
        if (currentIndex < tutorialTargets.Length)
        {
            tutorialTargets[currentIndex].SetTutorialTarget(true);
        }
        else
        {
            HideTutorial(); // 引导结束
        }
    }

    public void CloseTutorial() => HideTutorial();

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