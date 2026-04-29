using UnityEngine;
using TMPro;

public class TitleOption : MonoBehaviour
{
    [Header("选项设置")]
    [TextArea(2, 4)]
    public string titleText; // 报纸上的标题

    public bool isCorrect;   // 是否正确

    [Header("点评内容")]
    [TextArea(2, 4)]
    public string feedbackText; // 玩家点击发布后 NPC 说的话

    [Header("UI 引用")]
    public TMP_Text buttonText; // 按钮自己的文字

    private void OnValidate()
    {
        if (buttonText != null) buttonText.text = titleText;
    }

    private void Start()
    {
        if (buttonText != null) buttonText.text = titleText;
    }
}