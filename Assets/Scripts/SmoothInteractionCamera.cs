using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;
    public Transform newspaperAnchor;

    [Header("移动平滑度")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    [Header("初始视角特有的鼠标摆动")]
    public float mouseOffsetRange = 15f;
    public float mouseSmoothTime = 2f;

    [Header("全时段呼吸感")]
    public float breatheAmplitude = 0.05f; // 呼吸起伏幅度
    public float breatheSpeed = 0.8f;      // 呼吸频率

    private Transform targetAnchor;
    private Vector2 currentMouseOffset;
    private float breatheTimer;

    void Start()
    {
        // 初始时将目标设为初始位置
        if (initialAnchor != null) 
            targetAnchor = initialAnchor;
    }

    void Update()
    {
        // 1. 点击检测
        if (Input.GetMouseButtonDown(0))
        {
            // 如果点击在 UI 上，则不触发场景物体的射线检测
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
                return;

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 根据点击物体的名称切换锚点
                string name = hit.collider.gameObject.name.ToLower();
                
                if (name.Contains("notebook")) 
                    targetAnchor = notebookAnchor;
                else if (name.Contains("workingboard")) 
                    targetAnchor = workingboardAnchor;
                else if (name.Contains("newspaper")) 
                    targetAnchor = newspaperAnchor;
            }
        }

        // 2. 鼠标偏移计算
        if (targetAnchor == initialAnchor)
        {
            // 只有在初始视角时，才计算基于鼠标位置的轻微摆动偏移
            float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
            float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;
            
            currentMouseOffset.x = Mathf.Lerp(currentMouseOffset.x, mouseX * mouseOffsetRange, Time.deltaTime * mouseSmoothTime);
            currentMouseOffset.y = Mathf.Lerp(currentMouseOffset.y, mouseY * mouseOffsetRange, Time.deltaTime * mouseSmoothTime);
        }
        else
        {
            // 如果不在初始位置，偏移量快速归零，保证局部观察视角不晃动
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, Vector2.zero, Time.deltaTime * 5f);
        }

        // 3. 更新呼吸计时器
        breatheTimer += Time.deltaTime * breatheSpeed;
    }

    void LateUpdate()
    {
        if (targetAnchor == null) return;

        // --- 核心：计算位置 (锚点位置 + 全时段呼吸效果) ---
        float breatheOffset = Mathf.Sin(breatheTimer) * breatheAmplitude;
        Vector3 finalPos = targetAnchor.position + (targetAnchor.up * breatheOffset);

        // --- 核心：计算旋转 ---
        Quaternion finalRot;
        if (targetAnchor == initialAnchor)
        {
            // 初始视角：叠加鼠标偏移产生的旋转
            Quaternion offsetRotation = Quaternion.Euler(-currentMouseOffset.y, currentMouseOffset.x, 0);
            finalRot = targetAnchor.rotation * offsetRotation;
        }
        else
        {
            // 局部视角：直接对齐锚点旋转，不叠加鼠标偏移
            finalRot = targetAnchor.rotation;
        }

        // --- 执行平滑插值移动与旋转 ---
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotateSpeed);
    }

    /// <summary>
    /// 公共方法：供 UI 返回按钮调用，将视角切回初始点
    /// </summary>
    public void BackToInitialView()
    {
        targetAnchor = initialAnchor;
    }
}