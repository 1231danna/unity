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
    public float defaultBreatheAmplitude = 0.05f;
    public float focusedBreatheAmplitude = 0.01f;
    public float breatheSpeed = 0.8f;

    [HideInInspector] public Transform targetAnchor;
    [HideInInspector] public bool isShowingDocument = false;

    private Vector2 currentMouseOffset;
    private float breatheTimer;
    private float currentBreatheAmplitude;
    private bool isFrozen = true;

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
        currentBreatheAmplitude = defaultBreatheAmplitude;
        UpdateUIButtonVisibility();
        SetCameraFrozen(true);
    }

    public void SetCameraFrozen(bool state) => isFrozen = state;

    void Update()
    {
        // --- 【新增逻辑】：点击交互检测 ---
        if (Input.GetMouseButtonDown(0) && !isFrozen)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
                if (tm != null)
                {
                    // 1. 点击工作板触发第一段话
                    if (hit.collider.gameObject.name == "Workingboard")
                    {
                        tm.ShowDetailedInstruction("Notes from the front. Might be worth checking before I write.");
                    }
                    // 2. 点击照片触发第二段话
                    else if (hit.collider.gameObject.name == "model_0.004")
                    {
                        tm.ShowDetailedInstruction("That should be enough for the report. Now, the headline.");
                    }
                }
            }
        }

        // --- 【原有逻辑】：相机呼吸与鼠标偏移 ---
        if (targetAnchor == initialAnchor && !isFrozen)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            Vector2 targetOffset = new Vector2((mousePos.x - 0.5f) * mouseOffsetRange, (mousePos.y - 0.5f) * mouseOffsetRange);
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, targetOffset, Time.deltaTime * mouseSmoothTime);
            currentBreatheAmplitude = Mathf.Lerp(currentBreatheAmplitude, defaultBreatheAmplitude, Time.deltaTime * 5f);
        }
        else
        {
            currentBreatheAmplitude = Mathf.Lerp(currentBreatheAmplitude, focusedBreatheAmplitude, Time.deltaTime * 5f);
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, Vector2.zero, Time.deltaTime * 5f);
        }
        breatheTimer += Time.deltaTime * breatheSpeed;
    }

    public void UpdateUIButtonVisibility()
    {
        if (isShowingDocument)
        {
            if (backButton != null) backButton.SetActive(false);
            if (exitButton != null) exitButton.SetActive(false);
            return;
        }
        bool isInitial = (targetAnchor == initialAnchor);
        if (backButton != null) backButton.SetActive(!isInitial);
        if (exitButton != null) exitButton.SetActive(isInitial);
    }

    void LateUpdate()
    {
        if (targetAnchor == null) return;
        float breatheOffset = Mathf.Sin(breatheTimer) * currentBreatheAmplitude;
        Vector3 finalPos = targetAnchor.position + (targetAnchor.up * breatheOffset);
        Quaternion finalRot = (targetAnchor == initialAnchor) ? targetAnchor.rotation * Quaternion.Euler(-currentMouseOffset.y, currentMouseOffset.x, 0) : targetAnchor.rotation;
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotateSpeed);
        UpdateUIButtonVisibility();
    }

    public void BackToInitialView() { targetAnchor = initialAnchor; SetCameraFrozen(false); }
    public void ExitGame() { Application.Quit(); }
}