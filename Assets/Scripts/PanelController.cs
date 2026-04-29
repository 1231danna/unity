using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelController : MonoBehaviour
{
    public GameObject bigPhotoPanel;
    public Image displayImage;
    public GameObject closePanelButton;

    [Header("引用")]
    public SmoothInteractionCamera camScript;

    private bool hasTriggeredFinalDialogue = false; 

    public void Show(Sprite photo)
    {
        if (photo == null) return;
        displayImage.sprite = photo;
        bigPhotoPanel.SetActive(true);
        if (closePanelButton != null) closePanelButton.SetActive(true);

        if (camScript != null)
        {
            camScript.isShowingDocument = true;
            camScript.UpdateUIButtonVisibility();
        }
    }

    public void Hide()
    {
        // 1. 在隐藏面板之前，先找到 TutorialManager 并把延迟任务交给它
        if (!hasTriggeredFinalDialogue)
        {
            TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
            if (tm != null)
            {
                // 核心修改：用 tm.StartCoroutine 代替 StartCoroutine
                // 让不会被隐藏的 TutorialManager 来倒计时
                tm.StartCoroutine(TriggerDelayedTutorial(tm));
            }
            hasTriggeredFinalDialogue = true; 
        }

        // 2. 放心大胆地隐藏面板，不会再打断倒计时了
        bigPhotoPanel.SetActive(false);
        if (closePanelButton != null) closePanelButton.SetActive(false);

        if (camScript != null)
        {
            camScript.isShowingDocument = false;
            camScript.UpdateUIButtonVisibility();
        }
    }

    // 注意这里接收了 tm 作为参数
    IEnumerator TriggerDelayedTutorial(TutorialManager tm)
    {
        yield return new WaitForSeconds(0.5f); 
        tm.ShowNextStep(); 
    }
}