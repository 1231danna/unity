using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;
    public Transform newspaperAnchor;

    [Header("UI 按钮引用")]
    public GameObject backButton;
    public GameObject exitButton;

    [Header("移动平滑度")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    [Header("初始视角特有的鼠标摆动")]
    public float mouseOffsetRange = 15f;
    public float mouseSmoothTime = 2f;

    [Header("全时段呼吸感")]
    public float breatheAmplitude = 0.05f;
    public float breatheSpeed = 0.8f;

    private Transform targetAnchor;
    private Vector2 currentMouseOffset;
    private float breatheTimer;
    private bool isFrozen = false; // 镜头锁定开关

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
        UpdateUIButtonVisibility();
    }

    // 提供给 Manager 调用
    public void SetCameraFrozen(bool state)
    {
        isFrozen = state;
        // 锁定后强制清空偏移量，防止镜头歪着卡死
        if (state) currentMouseOffset = Vector2.zero;
    }

    void Update()
    {
        // 【关键改动】如果处于锁定状态，直接跳过所有交互逻辑
        if (isFrozen) return;

        // 1. 点击检测
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                string name = hit.collider.gameObject.name.ToLower();

                if (name.Contains("notebook")) targetAnchor = notebookAnchor;
                else if (name.Contains("workingboard")) targetAnchor = workingboardAnchor;
                else if (name.Contains("newspaper"))
                {
                    targetAnchor = newspaperAnchor;
                    // 通知 Manager 冻结相机并处理 UI
                    NewspaperManager nm = Object.FindFirstObjectByType<NewspaperManager>();
                    if (nm != null) nm.OnOpenNewspaper();
                }

                UpdateUIButtonVisibility();
            }
        }

        // 2. 鼠标偏移计算
        if (targetAnchor == initialAnchor)
        {
            float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
            float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;
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

        float breatheOffset = Mathf.Sin(breatheTimer) * breatheAmplitude;
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