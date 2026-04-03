using UnityEngine;

public class NewspaperInteract : MonoBehaviour
{
    public GameObject newspaperUIPanel; // 拖入层级中的 NewspaperUI Panel

    void OnMouseDown()
    {
        // 显示 UI
        newspaperUIPanel.SetActive(true);

        // 释放鼠标：如果你的游戏是第一人称控制，需解锁鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}