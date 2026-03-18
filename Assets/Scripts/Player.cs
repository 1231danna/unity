using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum PlayState
{
    Idle,
    ReadyMove,
    Moving,
    MoveEnd,
    Grey,
}

public class Player : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    int movePower;
    [SerializeField]
    int attackRange;

    public LogicTile Tile;
    GameBoard board;
    public PlayState State { get; private set; } = PlayState.Idle;
    [SerializeField]
    float moveSpeed;
    LogicTile endTile;

    void Start()
    {
        board = GameBoard.instance;
    }

    void SetAnimation(int x, int y, bool isActive = true)
    {
        animator.SetBool("isActive", isActive);
        animator.SetInteger("x", x);
        animator.SetInteger("y", y);
    }

    public bool CanBeSelected()
    {
        if(State == PlayState.Idle)
        {
            State = PlayState.ReadyMove;
            //Show move range
            board.ShowUITile(Tile, movePower, attackRange);
            SetAnimation(0, -1, true);
            return true;
        }
        return false;
    }

    public void MoveTo(LogicTile endTile)
    {
        var path = board.MoveToDestination(Tile, endTile);
        
        StartCoroutine(Move(path));
    }

    IEnumerator Move(List<LogicTile> path)
    {
        if(path.Count >= 2)
        {
            State = PlayState.Moving;
            Vector2Int previousVector = Vector2Int.zero;
            for (int i = 1; i < path.Count; i++)
            {
                var tile = path[i - 1];
                var nextTile = path[i];
                Vector3 nextPos = board.GetTileWorldPos(nextTile) + new Vector3(0.5f, 0f, 0f);

                Vector2Int currentVector = new Vector2Int(nextTile.X - tile.X, nextTile.Y - tile.Y);
                if (currentVector != previousVector)
                {
                    SetAnimation(currentVector.x, currentVector.y);
                }

                while (Vector3.Distance(transform.position, nextPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }
        State = PlayState.MoveEnd;
        endTile = path[path.Count - 1];
        board.ShowUITile(endTile, 0, attackRange, false);

    }

    public void GoBack()
    {
        State = PlayState.Idle;
        SetAnimation(0, -1);
        board.ShowUITile(Tile, movePower, attackRange);
        transform.position = board.GetTileWorldPos(Tile) + new Vector3(0.5f, 0f, 0f);
    }

    public void Recover()
    {
        State = PlayState.Idle;
        SetAnimation(0, 0);
        board.ClearAllUITiles();
    }

    public void StandBy()
    {
        State = PlayState.Grey;
        SetAnimation(0, 0, false);
        board.ClearAllUITiles();
        Tile.PlayerOnTile = null;
        Tile = endTile;
        Tile.PlayerOnTile = this;
    }

    public void NextTurn()
    {
        State = PlayState.Idle;
        SetAnimation(0, 0);
        board.ClearAllUITiles();
    }   
}
