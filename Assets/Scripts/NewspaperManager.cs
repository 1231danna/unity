using TMPro;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperManager : MonoBehaviour
{
    private const int BodyLineLength = 22;
    private const int BodyMaxLines = 3;

    [Header("UI References")]
    public TMP_Text titleSlot;
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public GameObject speakerPortrait;
    public Button closeButton;

    [Header("Speaker Portrait Layout")]
    [SerializeField]
    private Vector2 speakerPortraitSize = new(640f, 640f);
    [SerializeField]
    private Vector2 speakerPortraitOffset = new(-740f, 85f);

    [Header("Camera Logic")]
    public SmoothInteractionCamera camScript;

    [Header("Publish Stamp Animation")]
    [SerializeField]
    private Vector2 publishImpactOffset = new(0f, -28f);
    [SerializeField]
    private Vector3 publishStartScale = new(1.08f, 1.08f, 1f);
    [SerializeField]
    private Vector3 publishImpactScale = new(0.9f, 1.12f, 1f);
    [SerializeField]
    private float publishLiftRotation = -9f;
    [SerializeField]
    private float publishImpactRotation = 4f;
    [SerializeField]
    private float publishLiftDuration = 0.08f;
    [SerializeField]
    private float publishImpactDuration = 0.11f;
    [SerializeField]
    private float publishRecoverDuration = 0.16f;

    private bool isCurrentSelectionCorrect = false;
    private bool hasSelected = false;
    public bool isGameCompleted = false;
    private bool isPublishing = false;

    private Button publishButton;
    private RectTransform publishButtonRect;
    private Image publishButtonImage;
    private Vector2 publishButtonBasePosition;
    private Vector3 publishButtonBaseScale = Vector3.one;
    private Quaternion publishButtonBaseRotation = Quaternion.identity;

    void Awake()
    {
        CacheSpeakerPortraitReference();
        CachePublishButtonReference();
        ConfigureSpeakerPortrait();
    }

    void Start()
    {
        if (closeButton != null) closeButton.interactable = false;
        SetDialogueVisible(false);
    }

    public void SetCurrentSelection(TitleOption option)
    {
        if (isGameCompleted || option == null) return;

        titleSlot.text = FormatNewspaperTitle(option.titleText);
        isCurrentSelectionCorrect = option.isCorrect;
        hasSelected = true;
        SetDialogueVisible(false);
    }

    private string FormatNewspaperTitle(string rawTitle)
    {
        int headlineEnd = rawTitle.IndexOf('!', rawTitle.IndexOf('!') + 1);
        if (headlineEnd < 0) return rawTitle;

        string headline = FormatHeadline(rawTitle[..(headlineEnd + 1)]);
        string body = rawTitle[(headlineEnd + 1)..].Trim();
        return
            $"<align=\"center\"><line-height=82%><b>{headline}</b></line-height></align>\n" +
            $"<size=10%>\n</size><align=\"left\"><line-height=78%><size=38%>{FormatBody(body)}</size></line-height></align>";
    }

    private string FormatHeadline(string rawHeadline)
    {
        string headline = rawHeadline.Trim();
        int kickerEnd = headline.IndexOf('!');
        if (kickerEnd < 0 || kickerEnd >= headline.Length - 1)
        {
            return $"<size=92%>{headline.ToUpperInvariant()}</size>";
        }

        string kicker = headline[..(kickerEnd + 1)].Trim().ToUpperInvariant();
        string mainHeadline = BreakBeforeLastWord(headline[(kickerEnd + 1)..].Trim().ToUpperInvariant());
        return $"<size=76%>{kicker}</size>\n<size=96%>{mainHeadline}</size>";
    }

    private string BreakBeforeLastWord(string text)
    {
        int lastSpaceIndex = text.LastIndexOf(' ');
        if (lastSpaceIndex < 0) return text;

        return text[..lastSpaceIndex] + "\n" + text[(lastSpaceIndex + 1)..];
    }

    private string FormatBody(string body)
    {
        string compactBody = body.Replace(" ", "").Trim();
        StringBuilder builder = new();
        for (int lineIndex = 0, index = 0; lineIndex < BodyMaxLines && index < compactBody.Length; lineIndex++, index += BodyLineLength)
        {
            if (builder.Length > 0) builder.Append('\n');

            int lineLength = Mathf.Min(BodyLineLength, compactBody.Length - index);
            builder.Append(compactBody, index, lineLength);
        }

        return builder.ToString();
    }

    public void OnPublish()
    {
        if (isGameCompleted || !hasSelected || isPublishing) return;

        StartCoroutine(PlayPublishStampEffect());
    }

    public void OnOpenNewspaper()
    {
        if (camScript != null)
        {
            camScript.SetCameraFrozen(true);
            if (camScript.exitButton != null) camScript.exitButton.SetActive(false);
        }

        ConfigureSpeakerPortrait();
        ResetPublishButtonVisual();
        gameObject.SetActive(true);

        if (isGameCompleted)
        {
            SetDialogueVisible(true);
            dialogueText.text = "Excellent! You solved it.";
            if (closeButton != null) closeButton.interactable = true;
        }
        else
        {
            SetDialogueVisible(false);
            if (closeButton != null) closeButton.interactable = false;
        }
    }

    public void CloseNewspaper()
    {
        if (!isCurrentSelectionCorrect && !isGameCompleted) return;

        if (camScript != null)
        {
            camScript.SetCameraFrozen(false);
            if (camScript.exitButton != null) camScript.exitButton.SetActive(true);
        }

        if (!isGameCompleted)
        {
            hasSelected = false;
            isCurrentSelectionCorrect = false;
            titleSlot.text = "";
            SetDialogueVisible(false);
        }

        gameObject.SetActive(false);
    }

    private void SetDialogueVisible(bool isVisible)
    {
        ConfigureSpeakerPortrait();

        if (dialogueBox != null) dialogueBox.SetActive(isVisible);
        if (speakerPortrait != null)
        {
            speakerPortrait.SetActive(isVisible);
            speakerPortrait.transform.SetAsLastSibling();

            Image portraitImage = speakerPortrait.GetComponent<Image>();
            if (portraitImage != null)
            {
                portraitImage.enabled = isVisible;
            }
        }
    }

    private void CacheSpeakerPortraitReference()
    {
        if (speakerPortrait != null)
        {
            return;
        }

        Transform portraitTransform = transform.Find("Dialogue/LeaderPortrait");
        if (portraitTransform == null)
        {
            portraitTransform = transform.Find("LeaderPortrait");
        }

        if (portraitTransform != null)
        {
            speakerPortrait = portraitTransform.gameObject;
        }
    }

    private void CachePublishButtonReference()
    {
        if (publishButton != null && publishButtonRect != null)
        {
            return;
        }

        Transform publishTransform = transform.Find("NewspaperContent/ButtonPublish");
        if (publishTransform == null)
        {
            return;
        }

        publishButton = publishTransform.GetComponent<Button>();
        publishButtonRect = publishTransform as RectTransform;
        publishButtonImage = publishTransform.GetComponent<Image>();

        if (publishButtonRect == null)
        {
            return;
        }

        publishButtonBasePosition = publishButtonRect.anchoredPosition;
        publishButtonBaseScale = publishButtonRect.localScale;
        publishButtonBaseRotation = publishButtonRect.localRotation;
    }

    private void ConfigureSpeakerPortrait()
    {
        CacheSpeakerPortraitReference();

        if (dialogueBox == null || speakerPortrait == null)
        {
            return;
        }

        RectTransform dialogueRect = dialogueBox.GetComponent<RectTransform>();
        RectTransform portraitRect = speakerPortrait.GetComponent<RectTransform>();
        if (dialogueRect == null || portraitRect == null)
        {
            return;
        }

        if (portraitRect.parent != dialogueRect)
        {
            portraitRect.SetParent(dialogueRect, false);
        }

        portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
        portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        portraitRect.pivot = new Vector2(0.5f, 0.5f);
        portraitRect.anchoredPosition = speakerPortraitOffset;
        portraitRect.sizeDelta = speakerPortraitSize;
        portraitRect.localScale = Vector3.one;
    }

    private IEnumerator PlayPublishStampEffect()
    {
        isPublishing = true;
        CachePublishButtonReference();

        if (publishButton != null)
        {
            publishButton.interactable = false;
        }

        if (publishButtonRect == null)
        {
            ApplyPublishResult();
            isPublishing = false;
            yield break;
        }

        ResetPublishButtonVisual();

        Vector2 liftPosition = publishButtonBasePosition - (publishImpactOffset * 0.35f);
        Quaternion liftRotation = Quaternion.Euler(0f, 0f, publishLiftRotation);
        Quaternion impactRotation = Quaternion.Euler(0f, 0f, publishImpactRotation);

        yield return AnimatePublishButton(
            publishButtonBasePosition,
            liftPosition,
            publishButtonBaseScale,
            publishStartScale,
            publishButtonBaseRotation,
            liftRotation,
            publishLiftDuration,
            false);

        yield return AnimatePublishButton(
            liftPosition,
            publishButtonBasePosition + publishImpactOffset,
            publishStartScale,
            publishImpactScale,
            liftRotation,
            impactRotation,
            publishImpactDuration,
            true);

        ApplyPublishResult();

        yield return AnimatePublishButton(
            publishButtonBasePosition + publishImpactOffset,
            publishButtonBasePosition,
            publishImpactScale,
            publishButtonBaseScale,
            impactRotation,
            publishButtonBaseRotation,
            publishRecoverDuration,
            false);

        ResetPublishButtonVisual();

        if (publishButton != null)
        {
            publishButton.interactable = true;
        }

        isPublishing = false;
    }

    private IEnumerator AnimatePublishButton(
        Vector2 fromPosition,
        Vector2 toPosition,
        Vector3 fromScale,
        Vector3 toScale,
        Quaternion fromRotation,
        Quaternion toRotation,
        float duration,
        bool darkenOnImpact)
    {
        float elapsed = 0f;
        Color baseColor = publishButtonImage != null ? publishButtonImage.color : Color.white;
        Color targetColor = darkenOnImpact
            ? new Color(0.82f, 0.74f, 0.68f, baseColor.a)
            : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            publishButtonRect.anchoredPosition = Vector2.LerpUnclamped(fromPosition, toPosition, easedT);
            publishButtonRect.localScale = Vector3.LerpUnclamped(fromScale, toScale, easedT);
            publishButtonRect.localRotation = Quaternion.LerpUnclamped(fromRotation, toRotation, easedT);

            if (publishButtonImage != null)
            {
                publishButtonImage.color = Color.LerpUnclamped(baseColor, targetColor, easedT);
            }

            yield return null;
        }

        publishButtonRect.anchoredPosition = toPosition;
        publishButtonRect.localScale = toScale;
        publishButtonRect.localRotation = toRotation;

        if (publishButtonImage != null)
        {
            publishButtonImage.color = targetColor;
        }
    }

    private void ApplyPublishResult()
    {
        SetDialogueVisible(true);

        if (isCurrentSelectionCorrect)
        {
            dialogueText.text = "Excellent! You solved it.";
            if (closeButton != null) closeButton.interactable = true;
            isGameCompleted = true;
        }
        else
        {
            dialogueText.text = "Wrong! You are trapped here!";
            if (closeButton != null) closeButton.interactable = false;
        }
    }

    private void ResetPublishButtonVisual()
    {
        CachePublishButtonReference();

        if (publishButtonRect == null)
        {
            return;
        }

        publishButtonRect.anchoredPosition = publishButtonBasePosition;
        publishButtonRect.localScale = publishButtonBaseScale;
        publishButtonRect.localRotation = publishButtonBaseRotation;

        if (publishButtonImage != null)
        {
            publishButtonImage.color = Color.white;
        }
    }
}
