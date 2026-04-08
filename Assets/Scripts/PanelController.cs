using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject bigPhotoPanel;
    public Image displayImage;
    public GameObject closePanelButton; // 这是面板自带的关闭按钮

    [Header("引用相机脚本")]
    public SmoothInteractionCamera camScript; // 在 Inspector 里把主相机拖进来

    public void Show(Sprite photo)
    {
        if (photo == null) return;

        displayImage.sprite = photo;
        bigPhotoPanel.SetActive(true);

        if (closePanelButton != null) closePanelButton.SetActive(true);

        // --- 核心联动：告诉相机现在正在看档案 ---
        if (camScript != null)
        {
            camScript.isShowingDocument = true;
            camScript.UpdateUIButtonVisibility(); // 立即刷新按钮隐藏
        }
    }

    public void Hide()
    {
        bigPhotoPanel.SetActive(false);

        if (closePanelButton != null) closePanelButton.SetActive(false);

        // --- 核心联动：告诉相机档案看完了 ---
        if (camScript != null)
        {
            camScript.isShowingDocument = false;
            camScript.UpdateUIButtonVisibility(); // 立即恢复相机的 Back 按钮
        }
    }
}