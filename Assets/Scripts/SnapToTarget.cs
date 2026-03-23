using UnityEngine;

public class SnapToTarget : MonoBehaviour
{
    public DraggableItem correctItem;
    public float snapDistance = 0.12f;
    public bool isFilled = false;

    void Update()
    {
        if (isFilled || correctItem == null) return;

        if (!correctItem.IsDragging() && !correctItem.IsLocked())
        {
            float distance = Vector3.Distance(correctItem.transform.position, transform.position);

            if (distance <= snapDistance)
            {
                correctItem.transform.position = transform.position;
                correctItem.transform.rotation = transform.rotation;
                correctItem.LockItem();
                isFilled = true;

                Renderer r = correctItem.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.green * 2f);
                    r.material.color = Color.white;
                }
            }
        }
    }
}