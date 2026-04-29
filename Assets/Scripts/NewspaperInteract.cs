using UnityEngine;

public class NewspaperInteract : MonoBehaviour
{
    public NewspaperManager newspaperManager;

    void OnMouseDown()
    {
        // 防误触 1：防止穿透 UI
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
        {
            return;
        }

        // 防误触 2：询问 TutorialManager 是否允许点击
        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null && !tm.CanInteractWith(gameObject))
        {
            return; // 被拦截，直接退出
        }

        // 执行正常的打开报纸逻辑
        if (newspaperManager != null)
        {
            newspaperManager.OnOpenNewspaper();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}