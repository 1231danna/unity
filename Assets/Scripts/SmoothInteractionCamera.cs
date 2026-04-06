using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;

    [Header("UI 按钮引用")]
    public GameObject backButton;
    public GameObject exitButton;

    [Header("移动平滑度")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    [Header("初始视角特有的鼠标摆动")]
    public float mouseOffsetRange = 15f;
    public float mouseSmoothTime = 2f;

    [Header("呼吸感设置")]
    public float defaultBreatheAmplitude = 0.05f; // 初始视角的正常幅度
    public float focusedBreatheAmplitude = 0.01f; // 交互视角的微弱幅度
    public float breatheSpeed = 0.8f;

    [HideInInspector] public Transform targetAnchor;
    private Vector2 currentMouseOffset;
    private float breatheTimer;
    private float currentBreatheAmplitude;
    private bool isFrozen = false;

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
        currentBreatheAmplitude = defaultBreatheAmplitude;
        UpdateUIButtonVisibility();
    }

    // 供外部调用：冻结或解冻相机摆动
    public void SetCameraFrozen(bool state)
    {
        isFrozen = state;
        if (state) currentMouseOffset = Vector2.zero; // 冻结时强制归位
    }

    void Update()
    {
        // 如果处于冻结状态（如正在看报纸UI），跳过点击逻辑和鼠标偏移计算
        if (isFrozen)
        {
            breatheTimer += Time.deltaTime * breatheSpeed;
            return;
        }

        // 1. 点击检测 (仅限初始视角才允许切换)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (targetAnchor == initialAnchor)
            {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    string name = hit.collider.gameObject.name.ToLower();

                    if (name.Contains("notebook"))
                    {
                        targetAnchor = notebookAnchor;
                        UpdateUIButtonVisibility();
                    }
                    else if (name.Contains("workingboard"))
                    {
                        targetAnchor = workingboardAnchor;
                        UpdateUIButtonVisibility();
                    }
                    else if (name.Contains("newspaper"))
                    {
                        // 点击报纸逻辑：不移动相机，直接冻结并开启UI
                        SetCameraFrozen(true);
                        if (exitButton != null) exitButton.SetActive(false);

                        NewspaperManager nm = Object.FindFirstObjectByType<NewspaperManager>();
                        if (nm != null) nm.OnOpenNewspaper();
                    }
                }
            }
        }

        // 2. 动态呼吸幅度切换
        float targetAmp = (targetAnchor == initialAnchor) ? defaultBreatheAmplitude : focusedBreatheAmplitude;
        currentBreatheAmplitude = Mathf.Lerp(currentBreatheAmplitude, targetAmp, Time.deltaTime * moveSpeed);

        // 3. 初始视角的鼠标偏移计算
        if (targetAnchor == initialAnchor)
        {
            float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
            float mouseY = (Input.mousePosition.y / Screen.height) - 0.45f; // 向上偏移
            currentMouseOffset.x = Mathf.Lerp(currentMouseOffset.x, mouseX * mouseOffsetRange, Time.deltaTime * mouseSmoothTime);
            currentMouseOffset.y = Mathf.Lerp(currentMouseOffset.y, mouseY * mouseOffsetRange, Time.deltaTime * mouseSmoothTime);
        }
        else
        {
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, Vector2.zero, Time.deltaTime * 5f);
        }

        breatheTimer += Time.deltaTime * breatheSpeed;
    }

    void UpdateUIButtonVisibility()
    {
        bool isInitial = (targetAnchor == initialAnchor);
        if (backButton != null) backButton.SetActive(!isInitial);
        if (exitButton != null) exitButton.SetActive(isInitial);
    }

    void LateUpdate()
    {
        if (targetAnchor == null) return;

        float breatheOffset = Mathf.Sin(breatheTimer) * currentBreatheAmplitude;
        Vector3 finalPos = targetAnchor.position + (targetAnchor.up * breatheOffset);

        Quaternion finalRot;
        if (targetAnchor == initialAnchor)
        {
            Quaternion offsetRotation = Quaternion.Euler(-currentMouseOffset.y, currentMouseOffset.x, 0);
            finalRot = targetAnchor.rotation * offsetRotation;
        }
        else
        {
            finalRot = targetAnchor.rotation;
        }

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotateSpeed);
    }

    public void BackToInitialView()
    {
        targetAnchor = initialAnchor;
        SetCameraFrozen(false);
        UpdateUIButtonVisibility();
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}