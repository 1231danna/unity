using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewspaperManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleSlot;
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public Button closeButton; // 报纸界面右上角的 X 按钮

    [Header("Camera Logic")]
    public SmoothInteractionCamera camScript;

    private bool isCurrentSelectionCorrect = false;
    private bool hasSelected = false;

    void Start()
    {
        // 初始关闭按钮状态
        if (closeButton != null) closeButton.interactable = false;
    }

    public void SetCurrentSelection(TitleOption option)
    {
        if (option == null) return;
        titleSlot.text = option.titleText;
        isCurrentSelectionCorrect = option.isCorrect;
        hasSelected = true;
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    public void OnPublish()
    {
        if (!hasSelected) return;

        dialogueBox.SetActive(true);
        if (isCurrentSelectionCorrect)
        {
            dialogueText.text = "Excellent! You solved it.";
            if (closeButton != null) closeButton.interactable = true; // 仅正确时可点
        }
        else
        {
            dialogueText.text = "Wrong! You are trapped here!";
            if (closeButton != null) closeButton.interactable = false; // 错误时禁用
        }
    }

    // 当 3D 报纸被点击时触发
    public void OnOpenNewspaper()
    {
        if (camScript != null)
        {
            camScript.SetCameraFrozen(true); // 锁定镜头旋转
            if (camScript.exitButton != null) camScript.exitButton.SetActive(false); // 隐藏退出键
        }
        gameObject.SetActive(true);
    }

    public void CloseNewspaper()
    {
        // 双重保险：如果不正确，直接跳出不执行关闭
        if (!isCurrentSelectionCorrect) return;

        if (camScript != null)
        {
            camScript.SetCameraFrozen(false); // 恢复镜头旋转
            if (camScript.exitButton != null) camScript.exitButton.SetActive(true); // 恢复退出键
        }

        hasSelected = false;
        isCurrentSelectionCorrect = false;
        titleSlot.text = "";
        if (closeButton != null) closeButton.interactable = false; // 重置按钮状态
        gameObject.SetActive(false);
    }
}