using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup introGroup;
    public TMP_Text introText;
    public RectTransform filmGrainTransform;
    public Image filmGrainImage;

    [Header("Story Text")]
    [TextArea(3, 5)]
    public string[] storyLines;

    [Header("Typing Rhythm")]
    [Range(0.01f, 0.2f)]
    public float baseTypingSpeed = 0.05f;
    public float fadeDuration = 1.0f;

    [Header("Component References")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public SmoothInteractionCamera camScript;
    public bool playOnStart = true;

    private string currentVisibleText = "";
    private bool isTypingFinished;
    private bool hasStarted;

    void Start()
    {
        filmGrainImage.raycastTarget = false;
        introText.text = "";

        if (playOnStart)
        {
            PlayIntroSequence();
            return;
        }

        introGroup.alpha = 0f;
        introGroup.blocksRaycasts = false;
        introGroup.gameObject.SetActive(false);
        camScript.SetCameraFrozen(true);
    }

    public void PlayIntroSequence()
    {
        if (hasStarted) return;

        hasStarted = true;
        isTypingFinished = false;
        currentVisibleText = "";
        introText.text = "";
        introGroup.gameObject.SetActive(true);
        introGroup.alpha = 1f;
        introGroup.blocksRaycasts = true;
        camScript.isShowingDocument = true;
        camScript.SetCameraFrozen(true);
        camScript.UpdateUIButtonVisibility();

        StartCoroutine(BlinkCursor());
        StartCoroutine(PlayIntro());
        StartCoroutine(FilmGrainRoutine());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.8f);

        foreach (string line in storyLines)
        {
            yield return TypeLine(line);
            yield return new WaitForSeconds(1.2f);
        }

        isTypingFinished = true;
        yield return FadeOutIntro();

        camScript.isShowingDocument = false;
        camScript.SetCameraFrozen(false);
        camScript.UpdateUIButtonVisibility();
        introGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    IEnumerator TypeLine(string line)
    {
        currentVisibleText = "";

        for (int i = 0; i < line.Length; i++)
        {
            currentVisibleText += line[i];
            PlayTypeSound(i);
            yield return new WaitForSeconds(GetCharacterDelay(line[i]));
        }
    }

    void PlayTypeSound(int index)
    {
        if (index % 5 != 0 || audioSource == null || typeSound == null) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(typeSound, 0.7f);
    }

    float GetCharacterDelay(char character)
    {
        if (",.!?;:".Contains(character.ToString())) return 0.35f;
        if (Random.value > 0.95f) return baseTypingSpeed * 2.5f;
        return Random.Range(baseTypingSpeed * 0.9f, baseTypingSpeed * 1.1f);
    }

    IEnumerator FadeOutIntro()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            introGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        introGroup.alpha = 0f;
    }

    IEnumerator FilmGrainRoutine()
    {
        while (introGroup.alpha > 0f)
        {
            filmGrainImage.color = new Color(1f, 1f, 1f, Random.Range(0.03f, 0.08f));
            filmGrainTransform.anchoredPosition = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            filmGrainTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 4) * 90f);
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator BlinkCursor()
    {
        while (!isTypingFinished)
        {
            introText.text = currentVisibleText + "|";
            yield return new WaitForSeconds(0.3f);
            introText.text = currentVisibleText + " ";
            yield return new WaitForSeconds(0.3f);
        }

        introText.text = currentVisibleText;
    }
}
