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
    [HideInInspector] public bool isShowingDocument = false; // 新增：是否正在显示档案面板

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

    public void SetCameraFrozen(bool state)
    {
        isFrozen = state;
    }

    void Update()
    {
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

    // 核心修改：增加对档案状态的判断
    public void UpdateUIButtonVisibility()
    {
        // 如果正在看档案，强制隐藏所有相机自带按钮
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

        // 每一帧或状态改变时同步 UI 状态
        UpdateUIButtonVisibility();
    }

    public void BackToInitialView()
    {
        targetAnchor = initialAnchor;
        SetCameraFrozen(false);
    }

    public void ExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}