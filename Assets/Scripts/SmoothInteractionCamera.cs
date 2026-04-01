using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;
    public Transform newspaperAnchor;

    [Header("手动关联 UI (请把 Canvas 拖进来)")]
    public PanelController panelController; // 只需要加这一行，用来控制大图

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

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
    }

    void Update()
    {
        // 如果大图正在显示，相机完全静止，不处理点击
        if (panelController != null && panelController.bigPhotoPanel != null && panelController.bigPhotoPanel.activeSelf) 
        {
            return; 
        }

        // 1. 点击检测
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                string name = hit.collider.gameObject.name.ToLower();
                if (name.Contains("notebook")) targetAnchor = notebookAnchor;
                else if (name.Contains("workingboard")) targetAnchor = workingboardAnchor;
                else if (name.Contains("newspaper")) targetAnchor = newspaperAnchor;
            }
        }

        // 2. 初始视角鼠标偏移
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

    // --- 重点：这就是你原本的返回函数，我现在给它加了 UI 关闭逻辑 ---
    public void BackToInitialView()
    {
        // 原有逻辑：相机回去
        targetAnchor = initialAnchor;

        // 新增逻辑：如果 UI 开着，就关掉它
        if (panelController != null)
        {
            panelController.Hide(); 
        }
    }
}