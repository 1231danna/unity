using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    [SerializeField]
    GameBoard board = default;

    [Header("Level Design")]
    [SerializeField]
    Vector2Int[] spawnPositions = default;
    [SerializeField]
    int[] playerTypes = default;

    void Awake()
    {
        board.InitLogicTiles();
        board.InitPlayers(spawnPositions, playerTypes);
    }

    void Update()
    {
        if(TurnManager.instance.currentState != BattleState.PlayerTurn) return;

        if(Input.GetMouseButtonDown(0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            Vector3Int worldPos = new Vector3Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y), 0);
            
            // starTile, movePower - 3
            var tile = board.GetTileByPos(worldPos);
            board.ClickOneTile(tile);
            //board.ShowUITile(worldPos, movePower, attackRange);
        }

    }

}
