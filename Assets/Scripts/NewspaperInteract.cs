using UnityEngine;

public class NewspaperInteract : MonoBehaviour
{
    // 将类型从 GameObject 改为 NewspaperManager，这样可以直接调用它的方法
    public NewspaperManager newspaperManager;

    void OnMouseDown()
    {
        if (newspaperManager != null)
        {
            // 关键：调用这个方法，它内部已经包含了面板显示和相机锁定的逻辑
            newspaperManager.OnOpenNewspaper();
        }

        // 释放鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}