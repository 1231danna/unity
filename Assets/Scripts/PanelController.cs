using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject bigPhotoPanel; 
    public Image displayImage;       
    public GameObject backButton; // 新增：把你的返回按钮拖到这里

    public void Show(Sprite photo) 
    {
        if (photo == null) return;
        displayImage.sprite = photo;
        bigPhotoPanel.SetActive(true);
        
        // 显示面板时，确保按钮也显示出来
        if(backButton != null) backButton.SetActive(true);
    }

    public void Hide() 
    {
        bigPhotoPanel.SetActive(false);
        
        // --- 核心修改：关闭面板时，隐藏按钮 ---
        if(backButton != null) backButton.SetActive(false);
    }
}