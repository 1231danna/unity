using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NotebookTurn : MonoBehaviour
{
    [System.Serializable]
    public class StepUI
    {
        public GameObject rootGroup;
        public Button startBtn;
        public TextMeshProUGUI textMsg;
        public Button nextBtn;

        public bool isSceneJump;
        public string sceneNameToLoad; 
    }

    public SmoothInteractionCamera camScript;
    public Transform notebookAnchor;

    public CanvasGroup transitionOverlay;
    public float dashMoveSpeed = 30f;
    public float dashRotateSpeed = 20f;
    public float timeToBlack = 0.5f;

    public List<StepUI> steps;

    private int currentIdx = 0;
    private bool isTyping = false;

    void Start()
    {
        if (transitionOverlay != null)
        {
            transitionOverlay.alpha = 0f;
            transitionOverlay.gameObject.SetActive(false);
        }

        for (int i = 0; i < steps.Count; i++)
        {
            int index = i;
            StepUI s = steps[index];

            if (s.rootGroup != null) s.rootGroup.SetActive(false);

            if (s.startBtn != null)
                s.startBtn.onClick.AddListener(() => StartCoroutine(TypeEffect(s)));

            if (s.nextBtn != null)
            {
                if (s.isSceneJump)
                    s.nextBtn.onClick.AddListener(() => StartCoroutine(TransitionAndLoad(s.sceneNameToLoad)));
                else
                    s.nextBtn.onClick.AddListener(NextStepAction); 
            }
        }

        StartCoroutine(ShowUIDelayed(0, 0.5f));
    }

    void NextStepAction()
    {
        steps[currentIdx].rootGroup.SetActive(false);
        currentIdx++;

        if (currentIdx < steps.Count)
        {
            StartCoroutine(ShowUIDelayed(currentIdx, 0.2f));
        }
    }

    IEnumerator TransitionAndLoad(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("no scene name");
            yield break;
        }

        steps[currentIdx].rootGroup.SetActive(false);

        if (camScript != null)
        {
            Transform dashTarget = notebookAnchor.Find("AnchorTurn");
            if (dashTarget != null)
            {
                camScript.SetCameraFrozen(false);
                camScript.moveSpeed = dashMoveSpeed;
                camScript.rotateSpeed = dashRotateSpeed;
                camScript.targetAnchor = dashTarget;
            }
        }

        if (transitionOverlay != null)
        {
            transitionOverlay.gameObject.SetActive(true);
            transitionOverlay.alpha = 0f;
        }

        float elapsed = 0;
        while (elapsed < timeToBlack)
        {
            elapsed += Time.deltaTime;
            if (transitionOverlay != null) transitionOverlay.alpha = Mathf.Clamp01(elapsed / timeToBlack);
            yield return null;
        }

        SceneManager.LoadScene(targetSceneName);
    }


    IEnumerator TypeEffect(StepUI s)
    {
        isTyping = true;
        s.startBtn.gameObject.SetActive(false);
        s.textMsg.gameObject.SetActive(true);
        s.textMsg.maxVisibleCharacters = 0;

        string fullText = s.textMsg.text;
        int totalCharacters = fullText.Length;
        int currentVisible = 0;

        while (currentVisible < totalCharacters)
        {
            if (Input.GetMouseButtonDown(0))
            {
                s.textMsg.maxVisibleCharacters = totalCharacters;
                break;
            }
            s.textMsg.maxVisibleCharacters = ++currentVisible;
            yield return new WaitForSeconds(0.05f);
        }

        s.nextBtn.gameObject.SetActive(true);
        isTyping = false;
    }

    IEnumerator ShowUIDelayed(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (index < steps.Count)
        {
            var s = steps[index];
            s.rootGroup.SetActive(true);
            s.startBtn.gameObject.SetActive(true);
            s.textMsg.gameObject.SetActive(false);
            s.nextBtn.gameObject.SetActive(false);
        }
    }
}