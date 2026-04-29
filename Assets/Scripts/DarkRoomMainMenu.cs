using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DarkRoomMainMenu : MonoBehaviour
{
    private static readonly Color NormalTextColor = new Color(0.28f, 0.25f, 0.21f, 1f);
    private static readonly Color HoverTextColor = new Color(0.55f, 0.08f, 0.08f, 1f);
    private static readonly Color PressedTextColor = new Color(0.35f, 0.04f, 0.04f, 1f);
    private static readonly Color PaperTextColor = new Color(0.18f, 0.15f, 0.12f, 1f);
    private static readonly Color PaperImageColor = new Color(1f, 1f, 1f, 0.78f);
    private const float MenuDimmerAlpha = 0.06f;
    private static readonly Vector2 PlainButtonSize = new(370f, 56f);
    private static readonly Vector2 PaperButtonSize = new(220f, 107f);
    private static readonly Vector4 ButtonTextMargin = new(28f, 8f, 28f, 8f);

    [Header("Scene References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private IntroManager introManager;
    [SerializeField] private SmoothInteractionCamera camScript;
    [SerializeField] private Transform mainMenuAnchor;
    [SerializeField] private Transform gameStartAnchor;

    [Header("Menu Assets")]
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private TMP_FontAsset menuFont;

    private GameObject menuPanel;
    private GameObject mainContent;
    private GameObject staffContent;

    void Awake()
    {
        ResolveReferences();
        ValidateRequiredReferences();
        ApplyMenuCameraPose();
    }

    void Start()
    {
        PrepareSceneForMenu();
        BuildMenu();
        ShowMainMenu();
    }

    private void ResolveReferences()
    {
        if (targetCanvas == null) targetCanvas = FindScreenCanvas();
        if (introManager == null) introManager = Object.FindFirstObjectByType<IntroManager>();
        ResolveCameraReference();
        if (gameStartAnchor == null) gameStartAnchor = camScript.initialAnchor;
    }

    private void ResolveCameraReference()
    {
        if (camScript == null) camScript = Object.FindFirstObjectByType<SmoothInteractionCamera>();
    }

    private void ValidateRequiredReferences()
    {
        if (targetCanvas == null) throw new MissingReferenceException("DarkRoomMainMenu requires a target Canvas.");
        if (introManager == null) throw new MissingReferenceException("DarkRoomMainMenu requires an IntroManager.");
        if (camScript == null) throw new MissingReferenceException("DarkRoomMainMenu requires a SmoothInteractionCamera.");
        if (mainMenuAnchor == null) throw new MissingReferenceException("Assign Main Menu Anchor on DarkRoomMainMenu.");
        if (gameStartAnchor == null) throw new MissingReferenceException("DarkRoomMainMenu requires a game start anchor.");

    }

    private void ApplyMenuCameraPose()
    {
        camScript.targetAnchor = mainMenuAnchor;
        camScript.transform.SetPositionAndRotation(mainMenuAnchor.position, mainMenuAnchor.rotation);
    }

    private Canvas FindScreenCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return canvas;
        }

        return canvases[0];
    }

    private void PrepareSceneForMenu()
    {
        ApplyMenuCameraPose();
        camScript.isShowingDocument = true;
        camScript.SetCameraFrozen(true);
        camScript.UpdateUIButtonVisibility();

        introManager.playOnStart = false;
        introManager.introGroup.alpha = 0f;
        introManager.introGroup.blocksRaycasts = false;
        introManager.introGroup.gameObject.SetActive(false);
    }

    private void BuildMenu()
    {
        menuPanel = CreateRect("MainMenuPanel", targetCanvas.transform);
        StretchToParent(menuPanel.GetComponent<RectTransform>());

        Image dimmer = menuPanel.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, MenuDimmerAlpha);
        dimmer.raycastTarget = true;

        RectTransform paperRect = CreatePaper(menuPanel.transform);
        mainContent = CreateRect("MenuContent", paperRect);
        staffContent = CreateRect("ProductionStaffContent", paperRect);
        StretchToParent(mainContent.GetComponent<RectTransform>());
        StretchToParent(staffContent.GetComponent<RectTransform>());

        BuildMainContent(mainContent.transform);
        BuildStaffContent(staffContent.transform);
        menuPanel.transform.SetAsLastSibling();
    }

    private RectTransform CreatePaper(Transform parent)
    {
        GameObject paper = CreateRect("MenuPaper", parent);
        RectTransform rect = paper.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(18f, 0f);
        rect.sizeDelta = new Vector2(620f, 880f);

        Image image = paper.AddComponent<Image>();
        image.sprite = paperSprite;
        image.color = PaperImageColor;
        image.preserveAspect = false;
        image.raycastTarget = true;
        return rect;
    }

    private void BuildMainContent(Transform parent)
    {
        CreateText(parent, "The Daily Herald", 64f, new Vector2(0f, 25f), new Vector2(500f, 96f), FontStyles.Normal);
        CreateText(parent, "SPECIAL EDITION", 25f, new Vector2(0f, -40f), new Vector2(390f, 44f), FontStyles.Normal);
        CreateText(parent, "Western Front, 1916", 25f, new Vector2(0f, -82f), new Vector2(390f, 44f), FontStyles.Normal);

        CreateMenuButton(parent, "New Game", new Vector2(0f, -178f), StartNewGame, false);
        CreateMenuButton(parent, "Production staff", new Vector2(0f, -248f), ShowStaff, false);
        CreateMenuButton(parent, "Exit", new Vector2(0f, -318f), ExitGame, false);
    }

    private void BuildStaffContent(Transform parent)
    {
        CreateText(parent, "Production Staff", 44f, new Vector2(0f, 235f), new Vector2(400f, 76f), FontStyles.Normal);
        CreateText(parent, "Game Design\nArt\nProgramming\nUI\nSpecial Thanks", 28f, new Vector2(0f, 45f), new Vector2(380f, 270f), FontStyles.Normal);
        CreateMenuButton(parent, "Back", new Vector2(0f, -240f), ShowMainMenu, true);
    }

    private TMP_Text CreateText(Transform parent, string content, float size, Vector2 position, Vector2 dimensions, FontStyles style)
    {
        GameObject textObject = CreateRect(content, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = menuFont;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = PaperTextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action, bool useButtonBackground)
    {
        GameObject buttonObject = CreateRect(label + " Button", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = useButtonBackground ? PaperButtonSize : PlainButtonSize;

        Image hitArea = buttonObject.AddComponent<Image>();
        hitArea.sprite = useButtonBackground ? buttonSprite : null;
        hitArea.color = useButtonBackground ? Color.white : new Color(1f, 1f, 1f, 0f);
        hitArea.preserveAspect = false;

        Button button = buttonObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(action);

        TMP_Text text = CreateText(buttonObject.transform, label, 40f, Vector2.zero, rect.sizeDelta, FontStyles.Normal);
        text.color = NormalTextColor;
        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.fontSizeMax = 40f;
        text.enableWordWrapping = false;
        text.margin = ButtonTextMargin;

        DarkRoomMenuHover hover = buttonObject.AddComponent<DarkRoomMenuHover>();
        hover.SetTarget(text, NormalTextColor, HoverTextColor, PressedTextColor);
    }

    private GameObject CreateRect(string name, Transform parent)
    {
        GameObject rectObject = new GameObject(name, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);
        return rectObject;
    }

    private void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void ShowMainMenu()
    {
        mainContent.SetActive(true);
        staffContent.SetActive(false);
    }

    private void ShowStaff()
    {
        mainContent.SetActive(false);
        staffContent.SetActive(true);
    }

    private void StartNewGame()
    {
        camScript.targetAnchor = gameStartAnchor;
        menuPanel.SetActive(false);
        introManager.PlayIntroSequence();
    }

    private void ExitGame()
    {
        Debug.Log("Exit game selected from DarkRoom main menu.");
        Application.Quit();
    }

}

public class DarkRoomMenuHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private TMP_Text targetText;
    private Color normalColor;
    private Color hoverColor;
    private Color pressedColor;
    private bool isHovering;

    public void SetTarget(TMP_Text text, Color normal, Color hover, Color pressed)
    {
        targetText = text;
        normalColor = normal;
        hoverColor = hover;
        pressedColor = pressed;
        targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetText.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetText.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetText.color = isHovering ? hoverColor : normalColor;
    }
}
