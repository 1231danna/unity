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
        // 1. 先安全地隐藏面板
        bigPhotoPanel.SetActive(false);
        if (closePanelButton != null) closePanelButton.SetActive(false);

        if (camScript != null)
        {
            camScript.isShowingDocument = false;
            camScript.UpdateUIButtonVisibility();
        }

        // 2. 智能化触发下一步
        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null)
        {
            // 通过判断此时有没有物体正在发光，来决定要不要推进引导
            // 这样在以后自由探索时，关掉照片就不会乱弹对话框了
            if (tm.CanInteractWith(gameObject) == false || tm.tutorialPanel.alpha == 0)
            {
                tm.StartCoroutine(TriggerDelayedTutorial(tm));
            }
        }
    }

    IEnumerator TriggerDelayedTutorial(TutorialManager tm)
    {
        yield return new WaitForSeconds(0.5f); 
        tm.ShowNextStep(); 
    }
}