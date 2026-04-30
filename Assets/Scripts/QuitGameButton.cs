using UnityEngine;
using UnityEngine.SceneManagement;
public class QuitGameButton : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToNextLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
