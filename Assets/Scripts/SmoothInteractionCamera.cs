using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor; // 改为 public，供其他脚本判断
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

    [HideInInspector] public Transform targetAnchor; // 改为 public，方便外部判断当前目标
    private Vector2 currentMouseOffset;
    private float breatheTimer;
    private bool isFrozen = false; 

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
        UpdateUIButtonVisibility();
    }

    public void SetCameraFrozen(bool state)
    {
        isFrozen = state;
        if (state) currentMouseOffset = Vector2.zero;
    }

    void Update()
    {
        if (isFrozen) return;

        // 1. 点击检测 (处理视角切换)
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