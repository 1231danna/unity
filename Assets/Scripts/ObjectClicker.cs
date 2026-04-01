using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    public PanelController panelController; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 获取照片上的数据脚本
                PhotoItem item = hit.collider.GetComponent<PhotoItem>();
                
                if (item != null)
                {
                    // 这里调用 panelController 的 Show 函数
                    panelController.Show(item.highResSprite);
                }
            }
        }
    }
}