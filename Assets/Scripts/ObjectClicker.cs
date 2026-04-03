using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectClicker : MonoBehaviour
{
    public PanelController panelController;
    private SmoothInteractionCamera camScript;

    void Start()
    {
        // 自动获取主相机上的相机交互脚本
        camScript = Camera.main.GetComponent<SmoothInteractionCamera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 如果点在 UI 上，不处理
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // 2. 【核心拦截】：如果相机当前不在看工作板，不允许点照片
            if (camScript != null)
            {
                if (camScript.targetAnchor != camScript.workingboardAnchor)
                {
                    // 还没靠近工作板，不执行后面的弹出逻辑
                    return; 
                }
            }

            // 3. 执行射线检测
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PhotoItem item = hit.collider.GetComponent<PhotoItem>();
                if (item != null && item.highResSprite != null)
                {
                    // 弹出大图面板
                    panelController.Show(item.highResSprite);
                    
                    // (可选) 如果你希望弹出照片时相机完全锁定，取消下面注释
                    // camScript.SetCameraFrozen(true);
                }
            }
        }
    }
}