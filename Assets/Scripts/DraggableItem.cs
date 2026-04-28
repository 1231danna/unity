using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    public Transform dragSurface;   // 报纸
    public float surfaceOffset = 0.01f; // 让方块稍微浮在表面上，避免穿进去

    private bool canDrag = false;
    private bool isDragging = false;
    private bool isLocked = false;

    void OnMouseDown()
    {
        if (!canDrag || isLocked) return;
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!canDrag || !isDragging || isLocked) return;
        if (dragSurface == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 用报纸自己的朝向作为拖拽平面
        Plane plane = new Plane(dragSurface.forward, dragSurface.position);

        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 point = ray.GetPoint(distance);

            // 稍微抬离报纸表面一点，防止z-fighting/穿模
            Vector3 offset = dragSurface.forward * surfaceOffset;

            transform.position = point + offset;

            // 让方块角度跟报纸一致
            transform.rotation = dragSurface.rotation;
        }
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public void LockItem()
    {
        isLocked = true;
        isDragging = false;
    }
}