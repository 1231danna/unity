using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    [SerializeField]
    GameBoard board = default;

    void Awake()
    {
        board.InitLogicTiles();
        board.InitPlayers(new int[] { 0 });
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            Vector3Int worldPos = new Vector3Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y), 0);
            
            // starTile, movePower - 3
            var tile = board.GetTileByPos(worldPos);
            board.ClickOneTile(tile);
            //board.ShowUITile(worldPos, movePower, attackRange);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            board.StandBy();
        }
        else if(Input.GetKeyDown(KeyCode.Backspace))
        {
            board.Cancel();
        }
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            board.NextTurn();
        }
    }

}
