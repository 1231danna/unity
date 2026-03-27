using UnityEngine;
using UnityEngine.EventSystems; // 必须引用，用来检测是否点到了UI

public class SmoothInteractionCamera : MonoBehaviour
{
    [Header("相机目标锚点")]
    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;
    public Transform newspaperAnchor;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private Transform targetAnchor;

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
    }

    void Update()
    {
        // 1. 处理点击
        if (Input.GetMouseButtonDown(0))
        {
            // 重要：如果点到了 UI 按钮，就不要执行射线的逻辑，否则会冲突
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                string name = hit.collider.gameObject.name.ToLower();
                Debug.Log("【点击成功】目标: " + name);

                if (name.Contains("notebook")) targetAnchor = notebookAnchor;
                else if (name.Contains("workingboard")) targetAnchor = workingboardAnchor;
                else if (name.Contains("newspaper")) targetAnchor = newspaperAnchor;
            }
        }
    }

    // 2. 核心移动逻辑：LateUpdate 强制执行
    void LateUpdate()
    {
        if (targetAnchor != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetAnchor.position, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAnchor.rotation, Time.deltaTime * rotateSpeed);

            if (Vector3.Distance(transform.position, targetAnchor.position) < 0.001f)
            {
                transform.position = targetAnchor.position;
                transform.rotation = targetAnchor.rotation;
            }
        }
    }

    // 供按钮调用的方法：确保这个名字和你在 Button 里的设置一致
    public void BackToInitialView()
    {
        if (initialAnchor != null)
        {
            targetAnchor = initialAnchor;
            Debug.Log("【执行成功】相机正在退回初始点: " + initialAnchor.position);
        }
        else
        {
            Debug.LogError("你忘了在 Inspector 里拖入 Initial Anchor 啦！");
        }
    }
}