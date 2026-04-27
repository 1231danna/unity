using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoOverlayPlayer : MonoBehaviour
{
    private const int RenderWidth = 1280;
    private const int RenderHeight = 720;
    private static readonly Vector2 DisplaySize = new Vector2(700f, 394f);

    private static VideoOverlayPlayer instance;

    private GameObject root;
    private RawImage videoImage;
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private RenderTexture renderTexture;
    private SmoothInteractionCamera activeCameraScript;

    public static void Play(string videoFileName, SmoothInteractionCamera cameraScript)
    {
        if (instance == null)
        {
            GameObject playerObject = new GameObject(nameof(VideoOverlayPlayer));
            instance = playerObject.AddComponent<VideoOverlayPlayer>();
        }

        instance.Show(videoFileName, cameraScript);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    private void Update()
    {
        if (root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    private void Show(string videoFileName, SmoothInteractionCamera cameraScript)
    {
        activeCameraScript = cameraScript;
        root.SetActive(true);

        if (activeCameraScript != null)
        {
            activeCameraScript.isShowingDocument = true;
            activeCameraScript.UpdateUIButtonVisibility();
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video file not found: {videoPath}");
            Hide();
            return;
        }

        videoPlayer.Stop();
        videoPlayer.url = videoPath;
        videoPlayer.Prepare();
    }

    private void Hide()
    {
        videoPlayer.Stop();
        root.SetActive(false);

        if (activeCameraScript != null)
        {
            activeCameraScript.isShowingDocument = false;
            activeCameraScript.UpdateUIButtonVisibility();
        }
    }

    private void BuildOverlay()
    {
        root = new GameObject("VideoOverlay");
        root.transform.SetParent(transform, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        GameObject dimmerObject = new GameObject("Dimmer");
        dimmerObject.transform.SetParent(root.transform, false);
        RectTransform dimmerRect = dimmerObject.AddComponent<RectTransform>();
        dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
        dimmerRect.offsetMin = Vector2.zero;
        dimmerRect.offsetMax = Vector2.zero;
        Image dimmer = dimmerObject.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.82f);

        GameObject videoObject = new GameObject("VideoImage");
        videoObject.transform.SetParent(root.transform, false);
        RectTransform videoRect = videoObject.AddComponent<RectTransform>();
        videoRect.anchorMin = new Vector2(0.5f, 0.5f);
        videoRect.anchorMax = new Vector2(0.5f, 0.5f);
        videoRect.pivot = new Vector2(0.5f, 0.5f);
        videoRect.anchoredPosition = Vector2.zero;
        videoRect.sizeDelta = DisplaySize;
        videoImage = videoObject.AddComponent<RawImage>();
        videoImage.color = Color.white;

        CreateCloseButton();
        CreateVideoPlayer();
        root.SetActive(false);
    }

    private void CreateCloseButton()
    {
        GameObject buttonObject = new GameObject("CloseButton");
        buttonObject.transform.SetParent(root.transform, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-36f, -28f);
        buttonRect.sizeDelta = new Vector2(56f, 40f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.86f);

        Button closeButton = buttonObject.AddComponent<Button>();
        closeButton.onClick.AddListener(Hide);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = "X";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = 24;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void CreateVideoPlayer()
    {
        renderTexture = new RenderTexture(RenderWidth, RenderHeight, 0);
        renderTexture.Create();
        videoImage.texture = renderTexture;

        audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += _ => Hide();
    }

    private void OnVideoPrepared(VideoPlayer preparedPlayer)
    {
        preparedPlayer.Play();
    }
}
