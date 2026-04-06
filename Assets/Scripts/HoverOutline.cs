using EPOOutline; // 必须引入这个命名空间
using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    private Outlinable _outlinable;

    void Start()
    {
        _outlinable = GetComponent<Outlinable>();
        // 初始状态下关闭描边
        if (_outlinable != null) _outlinable.enabled = false;
    }

    // 鼠标移入显示描边
    void OnMouseEnter()
    {
        if (_outlinable != null) _outlinable.enabled = true;
    }

    // 鼠标移开消失描边
    void OnMouseExit()
    {
        if (_outlinable != null) _outlinable.enabled = false;
    }
}