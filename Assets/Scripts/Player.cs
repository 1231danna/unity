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

public enum FactionType
{
    Allied,
    German,
    None
}

public enum PlayerType
{
    Player,
    Enemy,
    Other,
}

public enum AOEType
{
    Single,
    Pierce,
    Cross,
    Fan,
    Square,
}

public class Player : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    int movePower;

    public int MovePower => movePower;
    [SerializeField]
    int attackRange;

    public int AttackRange => attackRange;

    public LogicTile Tile;
    GameBoard board;
    public PlayState State { get; private set; } = PlayState.Idle;
    [SerializeField]
    float moveSpeed;
    LogicTile endTile;
    public LogicTile EndTile => endTile;

    [Header("CombatStatus")]
    [SerializeField] 
    FactionType factionType;
    public FactionType FactionType => factionType;

    [SerializeField] 
    public int maxHP = 50;
    public int currentHP { get; private set; }
    [SerializeField]
    int attack = 20;
    public int Attack => attack;
    [SerializeField]
    int defense = 10;
    public int Defense => defense;
    public bool IsDead => currentHP <= 0;
    public PlayerType team;
    LogicTile startTile;

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Class/Cover Settings")]
    public bool isTank = false;
    public bool isCover = false;
    public int coverType = 0;

    public bool IsEnemy(Player other)
    {
        if (other == null) return false;
        if(this.team == PlayerType.Enemy)
        {
            return other.team == PlayerType.Player || other.team == PlayerType.Other;
        }
        return other.team == PlayerType.Enemy;
            
    }

    
    void Start()
    {
        board = GameBoard.instance;
        currentHP = maxHP;
        endTile = Tile;

        if (healthBar != null)
        {
            healthBar.UpdateHP(currentHP, maxHP);
        }
    }

    void SetAnimation(int x, int y, bool isActive = true)
    {
        animator.SetBool("isActive", isActive);
        animator.SetInteger("x", x);
        animator.SetInteger("y", y);
    }

    public bool CanBeSelected()
    {
        if(State == PlayState.Idle && team == PlayerType.Player)
        {
            startTile = Tile;
            State = PlayState.ReadyMove;
            board.ShowUITile(this, Tile, movePower, attackRange);
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
            if (UIManager.instance != null) 
            {
                UIManager.instance.HideActionMenu();
            }
            
            State = PlayState.Moving;
            if (this.isCover) 
            {
                this.Tile.CoverOnTile = null;
            }
            else 
            {
                this.Tile.PlayerOnTile = null;
            }
            
            LogicTile destination = path[path.Count - 1];
            this.Tile = destination;
            this.endTile = destination;
            
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

            if (this.isCover) 
            {
                this.Tile.CoverOnTile = this;
            }
            else 
            {
                this.Tile.PlayerOnTile = this;
            }
        }
        State = PlayState.MoveEnd;
        board.ShowUITile(this, endTile, 0, attackRange, false);

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowActionMenu(this);
        }

    }

    public void GoBack()
    {
        if (this.isCover) 
        {
            Tile.CoverOnTile = null;
        }
        else 
        {
            Tile.PlayerOnTile = null;
        }
        State = PlayState.Idle;
        SetAnimation(0, -1);
        Tile = startTile;
        endTile = startTile;
        if (this.isCover) 
        {
            Tile.CoverOnTile = this;
        }
        else 
        {
            Tile.PlayerOnTile = this;
        }
        transform.position = board.GetTileWorldPos(Tile) + new Vector3(0.5f, 0f, 0f);
        board.ClearAllUITiles();
        board.ShowUITile(this, Tile, movePower, attackRange);
        UIManager.instance.ShowActionMenu(this);
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
    }

    public void NextTurn()
    {
        State = PlayState.Idle;
        SetAnimation(0, 0);
        board.ClearAllUITiles();
    }   

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if(currentHP < 0)
        {
            currentHP = 0;
        }

        if (healthBar != null)
        {
            healthBar.UpdateHP(currentHP, maxHP);
        }

        if(IsDead)
        {
            Die();
        }
    }

    void Die()
    {
        //Play die animation
        board.ClearAllUITiles();
        if (Tile != null)
        {
            if (this.isCover) Tile.CoverOnTile = null;
            else Tile.PlayerOnTile = null;
        }

        if (endTile != null)
        {
            if (this.isCover) endTile.CoverOnTile = null;
            else endTile.PlayerOnTile = null;
        }

        Debug.Log($"{gameObject.name} is dead.");
        GameBoard.instance.RemovePlayerFromList(this);
        
        if(TurnManager.instance != null)
        {
            TurnManager.instance.CheckGameOver();
            TurnManager.instance.CheckVictory();
        }
        
        Destroy(gameObject);
    }

    [Header("Attack Settings")]
    [SerializeField]
    public AOEType AOEType = AOEType.Single;

    public List<Vector2Int> GetMyShape()
    {
        switch(AOEType)
        {
            case AOEType.Single: return GameBoard.SingleShape;
            case AOEType.Pierce: return GameBoard.PierceShape;
            case AOEType.Cross: return GameBoard.CrossShape;
            case AOEType.Fan: return GameBoard.FanShape;
            case AOEType.Square: return GameBoard.SquareShape;
            default: return GameBoard.SingleShape;
        }
    }

    public static Vector2Int GetDirectionTo(LogicTile from, LogicTile to)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            return new Vector2Int(dx > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, dy > 0 ? 1 : -1);
    }

    public void ForceFaceTarget(LogicTile targetTile)
    {
        Vector2Int facing = GetDirectionTo(this.Tile, targetTile);

        string fullStateName = $"{team}_Move_{(facing.x == 1 ? "Right" : facing.x == -1 ? "Left" : facing.y == 1 ? "Up" : "Down")}";

        animator.Play(fullStateName, 0, 0f);

        animator.SetInteger("x", facing.x);
        animator.SetInteger("y", facing.y);
        animator.SetBool("isActive", true);
    }
}
