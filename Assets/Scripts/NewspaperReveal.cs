using UnityEngine;

public class NewspaperReveal : MonoBehaviour
{
    [Header("初始状态")]
    public Vector3 closedScale = new Vector3(0.45f, 0.02f, 0.30f);
    public Vector3 closedRotation = new Vector3(0f, 0f, 0f);

    [Header("展开后状态")]
    public Vector3 openedLocalOffset = new Vector3(0f, 0.12f, -0.18f);
    public Vector3 openedScale = new Vector3(0.95f, 0.02f, 0.65f);
    public Vector3 openedRotation = new Vector3(30f, 0f, 0f);

    [Header("动画速度")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 4f;
    public float scaleSpeed = 4f;

    [Header("展开后激活")]
    public DraggableItem[] draggableItems;

    private Vector3 startWorldPos;
    private Vector3 targetWorldPos;

    private bool isOpening = false;
    private bool isOpened = false;

    void Start()
    {
        startWorldPos = transform.position;

        transform.localScale = closedScale;
        transform.eulerAngles = closedRotation;

        foreach (DraggableItem item in draggableItems)
        {
            if (item != null)
            {
                item.SetCanDrag(false);
            }
        }
    }

    void OnMouseDown()
    {
        if (isOpened || isOpening) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 camForwardFlat = cam.transform.forward;
        camForwardFlat.y = 0f;
        camForwardFlat.Normalize();

        targetWorldPos = startWorldPos - camForwardFlat * Mathf.Abs(openedLocalOffset.z) + Vector3.up * openedLocalOffset.y;

        isOpening = true;
    }

    void Update()
    {
        if (!isOpening) return;

        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * moveSpeed);

        Quaternion targetRot = Quaternion.Euler(openedRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);

        transform.localScale = Vector3.Lerp(transform.localScale, openedScale, Time.deltaTime * scaleSpeed);

        bool posDone = Vector3.Distance(transform.position, targetWorldPos) < 0.01f;
        bool scaleDone = Vector3.Distance(transform.localScale, openedScale) < 0.01f;
        bool rotDone = Quaternion.Angle(transform.rotation, targetRot) < 1f;

        if (posDone && scaleDone && rotDone)
        {
            transform.position = targetWorldPos;
            transform.rotation = targetRot;
            transform.localScale = openedScale;

            isOpening = false;
            isOpened = true;

            foreach (DraggableItem item in draggableItems)
            {
                if (item != null)
                {
                    item.SetCanDrag(true);

                    Renderer r = item.GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.material.EnableKeyword("_EMISSION");
                        r.material.SetColor("_EmissionColor", Color.yellow * 2.5f);
                        r.material.color = Color.white;
                    }

                    item.transform.localScale *= 1.1f;
                }
            }
        }
    }

    public bool IsOpened()
    {
        return isOpened;
    }
}