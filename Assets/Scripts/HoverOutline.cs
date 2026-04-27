using EPOOutline;
using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    private Outlinable _outlinable;
    public bool isTutorialTarget = false; // 只有在 TutorialManager 中被设为 true，鼠标移入才会发光

    void Start()
    {
        _outlinable = GetComponent<Outlinable>();
        if (_outlinable != null) _outlinable.enabled = false;
    }

    void OnMouseEnter()
    {
        if (isTutorialTarget && _outlinable != null) 
            _outlinable.enabled = true;
    }

    void OnMouseExit()
    {
        if (isTutorialTarget && _outlinable != null) 
            _outlinable.enabled = false;
    }

    // 供 TutorialManager 调用的接口
    public void SetTutorialTarget(bool active)
    {
        isTutorialTarget = active;
        // 如果被设为 false，立即关掉高光
        if (!active && _outlinable != null) _outlinable.enabled = false;
    }
}