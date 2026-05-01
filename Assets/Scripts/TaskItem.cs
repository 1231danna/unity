using UnityEngine;

public class TaskItem : MonoBehaviour
{
    public TaskTracker taskTracker;

    void OnMouseDown()
    {
        if (taskTracker != null)
        {
            taskTracker.RecordItemClick(gameObject);
            Debug.Log(gameObject.name + " alredy interacted");
        }
    }
}