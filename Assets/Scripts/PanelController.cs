using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject bigPhotoPanel; 
    public Image displayImage;       

    // 确保这里叫 Show，并且接收一个 Sprite 参数
    public void Show(Sprite photo) 
    {
        if (photo == null) return;
        displayImage.sprite = photo;
        bigPhotoPanel.SetActive(true);
    }

    public void Hide() 
    {
        bigPhotoPanel.SetActive(false);
    }
}