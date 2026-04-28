using UnityEngine;
using System.Collections.Generic;

public class TaskTracker : MonoBehaviour
{
    [Header("引用报纸管理器")]
    public NewspaperManager newspaperManager;

    [Header("清单：这一关必须点的物体（不包括报纸）")]
    public List<GameObject> requiredItems;

    // 存已经点过的物体
    private HashSet<GameObject> clickedItems = new HashSet<GameObject>();

    // 给 TaskItem 调用
    public void RecordItemClick(GameObject obj)
    {
        if (requiredItems.Contains(obj))
        {
            clickedItems.Add(obj);
        }
    }

    // 给 ObjectClicker 调用：判断总任务是否完成
    public bool IsEverythingDone()
    {
        // 1. 报纸拼对了没？
        bool paperDone = (newspaperManager != null && newspaperManager.isGameCompleted);

        // 2. 列表里的其他档案点完了没？
        bool othersDone = (clickedItems.Count >= requiredItems.Count);

        return paperDone && othersDone;
    }
}