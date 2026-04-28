using UnityEngine;

public class TaskItem : MonoBehaviour
{
    [Header("拖入场景中的任务管家")]
    public TaskTracker taskTracker;

    void OnMouseDown()
    {
        if (taskTracker != null)
        {
            // 仅仅记录一下：这个物体被点过了
            taskTracker.RecordItemClick(gameObject);
            Debug.Log(gameObject.name + " 已被交互，报到成功！");
        }
    }
}