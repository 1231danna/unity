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
         bigPhotoPanel.SetActive(false);
        if (closePanelButton != null) closePanelButton.SetActive(false);

        if (camScript != null)
        {
            camScript.isShowingDocument = false;
            camScript.UpdateUIButtonVisibility();
        }

        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null)
        {
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