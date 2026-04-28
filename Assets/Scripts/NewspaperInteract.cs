using UnityEngine;

public class NewspaperInteract : MonoBehaviour
{
    public NewspaperManager newspaperManager;
    public Transform newspaperAnchor;

    private SmoothInteractionCamera camScript;

    private void Start()
    {
        camScript = Camera.main.GetComponent<SmoothInteractionCamera>();

        if (newspaperAnchor == null)
        {
            GameObject anchorObject = GameObject.Find("AnchorNewspaper");
            if (anchorObject != null)
            {
                newspaperAnchor = anchorObject.transform;
            }
        }
    }

    private void OnMouseDown()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (camScript != null && newspaperAnchor != null && camScript.targetAnchor != newspaperAnchor)
        {
            camScript.targetAnchor = newspaperAnchor;
            camScript.SetCameraFrozen(false);
            camScript.UpdateUIButtonVisibility();
            return;
        }

        if (newspaperManager != null)
        {
            newspaperManager.OnOpenNewspaper();
        }
    }
}
