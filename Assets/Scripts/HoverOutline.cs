using EPOOutline;
using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    private Outlinable _outlinable;
    private bool isTutorialTarget = false;

    void Awake()
    {
        _outlinable = GetComponent<Outlinable>();
        if (_outlinable != null) _outlinable.enabled = false;
    }

    void OnMouseEnter()
    {
        if (isTutorialTarget && _outlinable != null) _outlinable.enabled = true;
    }

    void OnMouseExit()
    {
        if (isTutorialTarget && _outlinable != null) _outlinable.enabled = false;
    }

    public void SetTutorialTarget(bool active)
    {
        isTutorialTarget = active;
        // 如果被关闭，立即隐藏高光
        if (!active && _outlinable != null) _outlinable.enabled = false;
    }
}