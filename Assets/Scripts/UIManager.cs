using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    
    public static UIManager instance;
    [Header("ActionMenu")]
    [SerializeField]
    public GameObject actionMenu;
    [SerializeField]
    Button moveBtn;
    [SerializeField]
    Button attackBtn;
    [SerializeField]
    Button stayBtn;
    [SerializeField]
    Button undoBtn;

    [Header("SystemMenu")]
    public GameObject systemMenu;
    public GameObject exitConfirmMenu;
    [SerializeField]
    Button nextTurnBtn;
    [SerializeField]
    Button saveBtn;
    [SerializeField]
    Button loadBtn;
    [SerializeField]
    Button settingsBtn;
    [SerializeField]
    Button quitBtn;

    Player currentUnit;

    [Header("Game Over UI")]
    public GameObject victoryPanel;
    public GameObject failurePanel;

    void Awake()
    {
        instance = this;
        HideActionMenu();
    }

    void Update()
    {
        if (actionMenu != null && actionMenu.activeSelf && currentUnit != null)
        {
            UpdateMenuPosition();
        }
    }

    void UpdateMenuPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(currentUnit.transform.position);
        screenPos.z = 0; 
        actionMenu.transform.position = screenPos + new Vector3(60, 25, 0);
    }

    public void ShowActionMenu(Player unit)
    {
        if (unit.team != PlayerType.Player) 
        {
            HideActionMenu();
            return;
        }
        
        currentUnit = unit;
        UpdateMenuPosition();

        moveBtn.gameObject.SetActive(false);
        attackBtn.gameObject.SetActive(false);
        stayBtn.gameObject.SetActive(false);
        undoBtn.gameObject.SetActive(false);
        switch (unit.State)
        {
            case PlayState.ReadyMove:
            moveBtn.gameObject.SetActive(true);
            attackBtn.gameObject.SetActive(true);
            stayBtn.gameObject.SetActive(true);
            break;

            case PlayState.MoveEnd:
            attackBtn.gameObject.SetActive(true);
            stayBtn.gameObject.SetActive(true);
            undoBtn.gameObject.SetActive(true);
            break;

            case PlayState.Idle:
            moveBtn.gameObject.SetActive(true);
            attackBtn.gameObject.SetActive(true);
            stayBtn.gameObject.SetActive(true);
            break;
        }
        actionMenu.SetActive(true);
    }

    public void HideActionMenu()
    {
        actionMenu.SetActive(false);
    }

    public void OnMoveBtnClick()
    {
        HideActionMenu();
        if (currentUnit != null && GameBoard.instance != null)
        {
            GameBoard.instance.ShowUITile(currentUnit, currentUnit.Tile, currentUnit.MovePower, currentUnit.AttackRange, true, false);
        }
    }

    public void OnAttackBtnClick()
    {
        HideActionMenu();
        if (currentUnit != null && GameBoard.instance != null)
        {
            GameBoard.instance.ShowUITile(currentUnit, currentUnit.Tile, 0, currentUnit.AttackRange, false, true);
        }
    }

    public void OnStayBtnClick()
    {
        if (currentUnit != null)
        {
            currentUnit.StandBy();
            HideActionMenu();
            if (GameBoard.instance != null)
            {
                GameBoard.instance.ClearAllUITiles();
                GameBoard.instance.currentPlayer = null;
            }
            currentUnit = null;
        }
    }

    public void OnUndoBtnClick()
    {
        if (currentUnit != null)
        {
            currentUnit.GoBack();
        }
    }

    public void ShowSystemMenu()
    {
        Vector3 mousePos = Input.mousePosition;
        systemMenu.transform.position = mousePos + new Vector3(50, -25, 0);
        systemMenu.SetActive(true);

        if (exitConfirmMenu != null)
        {
            exitConfirmMenu.SetActive(false);
        }
    }

    public void HideAllMenus()
    {
        if (actionMenu != null) actionMenu.SetActive(false);
        if (systemMenu != null) systemMenu.SetActive(false);
    }

    public void OnNextTurnBtnClick()
    {
        HideAllMenus();
        TurnManager.instance.SetState(BattleState.EnemyTurn);
    }

    public void ClickExitMenu()
    {
        if (exitConfirmMenu != null)
        {
            exitConfirmMenu.SetActive(true);
        }
    }

    public void CancelExit()
    {
        if (exitConfirmMenu != null)
        {
            exitConfirmMenu.SetActive(false);
        }
    }
    
    public void OnExitBtnClick()
    {
        Application.Quit();
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public void GoToNextLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ShowFailurePanel()
    {
        if (failurePanel != null)
        {
            failurePanel.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
