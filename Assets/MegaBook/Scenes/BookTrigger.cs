using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection; // 召唤最强反射魔法必须加这句

public class BookTrigger : MonoBehaviour
{
    // 这里留了一个空位，用来手动瞄准书本
    public GameObject bookObject;
    public float targetPage = 3f;
    public string nextScene = "Demo";

    void Update()
    {
        // 如果没有瞄准书本，就什么都不做
        if (bookObject == null) return;

        // 强行搜查这个物体上的所有脚本
        Component[] allScripts = bookObject.GetComponents<MonoBehaviour>();
        foreach (Component script in allScripts)
        {
            if (script == null) continue;
            var type = script.GetType();

            // 绝地三尺：不管大小写，先找普通变量 (Field)
            var pageField = type.GetField("page", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pageField != null)
            {
                float currentPage = System.Convert.ToSingle(pageField.GetValue(script));
                if (currentPage >= targetPage) SceneManager.LoadScene(nextScene);
                return;
            }

            // 如果没找到，再找高级属性 (Property)
            var pageProperty = type.GetProperty("page", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pageProperty != null)
            {
                float currentPage = System.Convert.ToSingle(pageProperty.GetValue(script, null));
                if (currentPage >= targetPage) SceneManager.LoadScene(nextScene);
                return;
            }
        }
    }
}