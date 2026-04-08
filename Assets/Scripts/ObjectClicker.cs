using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectClicker : MonoBehaviour
{
    public PanelController panelController;
    private SmoothInteractionCamera camScript;

    [Header("视角切换目标物体")]
    public GameObject notebookObject;     // 在 Inspector 面板把笔记本模型拖进来
    public GameObject workingboardObject; // 在 Inspector 面板把桌子模型拖进来

    void Start()
    {
        // 自动获取相机脚本
        camScript = Camera.main.GetComponent<SmoothInteractionCamera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 如果点在 UI 上（比如点按钮），就不触发场景物体的射线检测
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // 2. 发射射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObj = hit.collider.gameObject;

                // --- 逻辑 A: 点击物体切换镜头 ---

                // 如果点到了笔记本物体
                if (hitObj == notebookObject)
                {
                    camScript.targetAnchor = camScript.notebookAnchor;
                    camScript.UpdateUIButtonVisibility();
                    return; // 切换了镜头就结束本次 Update，不往下执行
                }

                // 如果点到了工作板（桌子）物体
                if (hitObj == workingboardObject)
                {
                    camScript.targetAnchor = camScript.workingboardAnchor;
                    camScript.UpdateUIButtonVisibility();
                    return;
                }

                // --- 逻辑 B: 点击档案弹出大图 (仅在已经处于工作板视角时生效) ---

                if (camScript != null && camScript.targetAnchor == camScript.workingboardAnchor)
                {
                    PhotoItem item = hitObj.GetComponent<PhotoItem>();
                    if (item != null && item.highResSprite != null)
                    {
                        panelController.Show(item.highResSprite);
                    }
                }
            }
        }
    }
}