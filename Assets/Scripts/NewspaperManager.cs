using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewspaperManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleSlot;
    public GameObject dialogueBox; // 对话框物体
    public TMP_Text dialogueText;
    public Button closeButton;

    [Header("Camera Logic")]
    public SmoothInteractionCamera camScript;

    private bool isCurrentSelectionCorrect = false;
    private bool hasSelected = false;
    private bool isGameCompleted = false; // 标记任务是否彻底完成

    void Start()
    {
        if (closeButton != null) closeButton.interactable = false;
        if (dialogueBox != null) dialogueBox.SetActive(false); // 游戏开始时隐藏
    }

    public void SetCurrentSelection(TitleOption option)
    {
        // 如果已经彻底完成了，不允许再修改选项
        if (isGameCompleted || option == null) return;

        titleSlot.text = option.titleText;
        isCurrentSelectionCorrect = option.isCorrect;
        hasSelected = true;

        // 选择标题时，确保对话框是消失的，直到点发布
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    public void OnPublish()
    {
        if (isGameCompleted) return;
        if (!hasSelected) return;

        // 点击发布才显示对话框
        if (dialogueBox != null) dialogueBox.SetActive(true);

        if (isCurrentSelectionCorrect)
        {
            dialogueText.text = "Excellent! You solved it.";
            if (closeButton != null) closeButton.interactable = true;
            isGameCompleted = true; // 标记为永久完成
        }
        else
        {
            dialogueText.text = "Wrong! You are trapped here!";
            if (closeButton != null) closeButton.interactable = false;
        }
    }

    public void OnOpenNewspaper()
    {
        // 1. 相机逻辑处理
        if (camScript != null)
        {
            camScript.SetCameraFrozen(true); // 锁定镜头
            if (camScript.exitButton != null) camScript.exitButton.SetActive(false); // 隐藏退出键
        }

        // 2. UI 状态初始化
        gameObject.SetActive(true);

        // --- 核心修改：打开时根据是否完成来决定对话框显隐 ---
        if (isGameCompleted)
        {
            // 如果已经完成了，进来直接显示成功总结
            if (dialogueBox != null) dialogueBox.SetActive(true);
            dialogueText.text = "Excellent! You solved it.";
            if (closeButton != null) closeButton.interactable = true;
        }
        else
        {
            // 如果还没完成，进来时对话框必须是隐藏的
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (closeButton != null) closeButton.interactable = false;
        }
    }

    public void CloseNewspaper()
    {
        // 只有正确或者是已经完成的状态下才能关闭
        if (!isCurrentSelectionCorrect && !isGameCompleted) return;

        if (camScript != null)
        {
            camScript.SetCameraFrozen(false); // 恢复镜头摆动
            if (camScript.exitButton != null) camScript.exitButton.SetActive(true); // 恢复退出键
        }

        // 如果还没完成（点错了之后关闭），重置状态；如果已完成，则保留文字
        if (!isGameCompleted)
        {
            hasSelected = false;
            isCurrentSelectionCorrect = false;
            titleSlot.text = "";
            if (dialogueBox != null) dialogueBox.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}