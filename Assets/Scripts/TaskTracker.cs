using UnityEngine;
using System.Collections.Generic;

public class TaskTracker : MonoBehaviour
{
    public NewspaperManager newspaperManager;
    public List<GameObject> requiredItems;

    private HashSet<GameObject> clickedItems = new HashSet<GameObject>();

    public void RecordItemClick(GameObject obj) // for taskitem
    {
        if (requiredItems.Contains(obj))
        {
            clickedItems.Add(obj);
        }
    }

    public bool IsEverythingDone() // for objectclicker
    {
        bool paperDone = (newspaperManager != null && newspaperManager.isGameCompleted);
        bool othersDone = (clickedItems.Count >= requiredItems.Count);

        return paperDone && othersDone;
    }
}