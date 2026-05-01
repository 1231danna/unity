using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 5)]
    public string dialogueText;

    public HoverOutline targetToHighlight;
}

public class TutorialManager : MonoBehaviour
{

    public bool autoStartTutorial = false;
    public bool isSilentMode = false;
    public CanvasGroup tutorialPanel;
    public TMP_Text tutorialText;
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