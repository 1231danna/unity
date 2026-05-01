using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectClicker : MonoBehaviour
{
    public PanelController panelController;
    private SmoothInteractionCamera camScript;
    private const string VideoTriggerSpriteName = "moive";
    private const string TestVideoFileName = "test-video.mp4";

    public GameObject notebookObject;     
    public GameObject workingboardObject;
    public TaskTracker taskTracker;

    void Start()
    {
        camScript = Camera.main.GetComponent<SmoothInteractionCamera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj == notebookObject)
                {
                    if (taskTracker != null && !taskTracker.IsEverythingDone())
                    {
                        Debug.Log("mission not complete");
                        return; 
                    }
                        camScript.targetAnchor = camScript.notebookAnchor;
                    camScript.UpdateUIButtonVisibility();
                    return; 
                }

               
                if (hitObj == workingboardObject)
                {
                    camScript.targetAnchor = camScript.workingboardAnchor;
                    camScript.UpdateUIButtonVisibility();
                    return;
                }


                if (camScript != null && camScript.targetAnchor == camScript.workingboardAnchor)
                {
                    PhotoItem item = hitObj.GetComponent<PhotoItem>();
                    if (item != null && item.highResSprite != null)
                    {
                        if (item.highResSprite.name == VideoTriggerSpriteName)
                        {
                            VideoOverlayPlayer.Play(TestVideoFileName, camScript);
                            return;
                        }

                        panelController.Show(item.highResSprite);
                    }
                }
            }
        }
    }
}
