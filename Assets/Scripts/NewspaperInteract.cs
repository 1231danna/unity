using UnityEngine;

public class NewspaperInteract : MonoBehaviour
{
    public NewspaperManager newspaperManager;

    void OnMouseDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
        {
            return;
        }

        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null && !tm.CanInteractWith(gameObject))
        {
            return; 
        }

        if (newspaperManager != null)
        {
            newspaperManager.OnOpenNewspaper();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}