using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum UITileType
{
    Move,
    Attack,
}

public class GameBoard : MonoBehaviour
{
    Vector2Int tileSize;
    [SerializeField]
    Tilemap walkTilemap = default;
    [SerializeField]
    Tilemap cannotWalkTilemap = default;
    [SerializeField]
    Transform uiTiles = default;
    [SerializeField]
    GameObject[] uiTilePrefabs;

    TileBase[] tiles;
    LogicTile[] logicTiles;

    Queue<LogicTile> search = new Queue<LogicTile>();
    List<LogicTile> moveTiles = new List<LogicTile>();
    List<LogicTile> attackTiles = new List<LogicTile>();
    List<GameObject> uiTileObjects = new List<GameObject>();

    Dictionary<Vector3, LogicTile> allLogicTileDict = new Dictionary<Vector3, LogicTile>();

    Player currentPlayer = null;
    public static GameBoard instance;
    [SerializeField]
    Player[] playerPrefabs;
    List<Player> allMyPlayers = new List<Player>();

    public void InitLogicTiles()
    {
        instance = this;
        var bounds = walkTilemap.cellBounds;
        tiles = walkTilemap.GetTilesBlock(bounds);
        tileSize = (Vector2Int)walkTilemap.size;

        logicTiles = new LogicTile[tileSize.x * tileSize.y];
        for(int y = 0, i = 0; y < tileSize.y; y++)
        {
            for(int x = 0; x < tileSize.x; x++, i++)
            {
                var tile = logicTiles[i] = new LogicTile(x,y, tiles[i]);
                if(x > 0)
                {
                    LogicTile.SetEWNeighbors(tile, logicTiles[i - 1]);
                }
                if(y > 0)
                {
                    LogicTile.SetNSNeighbors(tile, logicTiles[i - tileSize.x]);
                }
            }
        }

        //FindMovePaths();

        int index = 0;
        foreach(var pos in walkTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = walkTilemap.CellToWorld(pos);
            allLogicTileDict.Add(worldPos, logicTiles[index]);
            logicTiles[index].LocalPos = pos;
            index++;
        }

        //var path = MoveToDestination(logicTiles[0], logicTiles[3]);
        //for (int i = 0; i < path.Count; i++)
        //{
        //    Debug.Log("路径格子(" + path[i].X + "," + path[i].Y + ") ");
        //}
    }

    public void ClearAllUITiles()
    {
        for(int i = 0; i < uiTileObjects.Count; i++)
        {
            Destroy(uiTileObjects[i]);
        }
        uiTileObjects.Clear();
    }
    
    public void ShowUITile(LogicTile tile, int movePower, int attackRange, bool showMove = true, bool showAttack = true)
    {
        ClearAllUITiles();
        // findway
        FindMovePaths(tile, movePower);
        FindAttackPaths(attackRange);
        if(showMove)
        {
            ShowOneUITile(moveTiles, UITileType.Move);
        }
        if(showAttack)
        {
            ShowOneUITile(attackTiles, UITileType.Attack);  
        }
    }

    void ShowOneUITile(List<LogicTile> tiles, UITileType type)
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            Vector3Int localPos = tile.LocalPos;
            var worldP = walkTilemap.CellToWorld(localPos);
            var prefab = Instantiate(uiTilePrefabs[(int)type]);
            prefab.transform.SetParent(uiTiles);
            prefab.transform.position = worldP;
            uiTileObjects.Add(prefab);
        }
    }

    void FindMovePaths(LogicTile startTile, int movePower)
    {
        foreach(var tile in logicTiles)
        {
            tile.Clear();
        }
        
        moveTiles.Clear();
        search.Enqueue(startTile);
        startTile.MarkAsStart(movePower);
        while(search.Count > 0)
        {
            var tile = search.Dequeue();
            if(tile != null)
            {
                moveTiles.Add(tile);
                search.Enqueue(tile.GrowNorth());
                search.Enqueue(tile.GrowEast());
                search.Enqueue(tile.GrowSouth());
                search.Enqueue(tile.GrowWest());
            }
        }
    }

    void FindAttackPaths(int attackRange)
    {
        attackTiles.Clear();
        // moveTiles Edge
        var boundTiles = LogicTile.GetBoundTiles(moveTiles);

        for (int i = 0; i < boundTiles.Count; i++)
        {
            var tile = boundTiles[i];
            search.Enqueue(tile);
            tile.MarkAsAttackRange(attackRange);
        }
        
        while(search.Count > 0)
        {
            var tile = search.Dequeue();
            if(tile != null)
            {
                if ( !moveTiles.Contains(tile))
                {
                    attackTiles.Add(tile);
                }
                search.Enqueue(tile.GrowAttackNorth());
                search.Enqueue(tile.GrowAttackEast());
                search.Enqueue(tile.GrowAttackSouth());
                search.Enqueue(tile.GrowAttackWest());
            }
        }
    }

    public List<LogicTile> MoveToDestination(LogicTile start, LogicTile end)
    {
        List<LogicTile> result = new List<LogicTile>(); 
        LogicTile current = end;
        while(current != null)
        {
            result.Add(current);
            current = current.NextOnPath;
        }
        result.Add(start);
        result.Reverse();
        return result;
    }

    public LogicTile GetTileByPos(Vector3Int worldPos)
    {
        if(allLogicTileDict.TryGetValue(worldPos, out LogicTile tile))
        {
            return tile;
        }
        return null;
    }

    public void ClickOneTile(LogicTile tile)
    {
        if(tile == null)
        {
            return;
        }

        if(currentPlayer == null)
        {
            var player = tile.PlayerOnTile;
            if(player != null && player.CanBeSelected())
            {
                currentPlayer = player;
            }
        }
        else
        {
            if(IsMoveRange(tile))
            {
                currentPlayer.MoveTo(tile);
            }
            else
            {
                currentPlayer.Recover();
                currentPlayer = null;
            }
        }
        
    }

    bool IsMoveRange(LogicTile tile)
    {
        return moveTiles.Contains(tile);
    }

    public void InitPlayers(int[] indexes)
    {
        for(int i = 0; i < indexes.Length; i++)
        {
            var tile = logicTiles[indexes[i]];
            var worldPos = GetTileWorldPos(tile);
            var player = Instantiate(playerPrefabs[0]);
            allMyPlayers.Add(player);
            player.transform.position = worldPos + new Vector3(0.5f, 0f, 0f);

            tile.PlayerOnTile = player;
            player.Tile = tile;
        }
    }

    public Vector3 GetTileWorldPos(LogicTile tile)
    {
        var pos = tile.LocalPos;
        return walkTilemap.CellToWorld(pos);
    }

    public void Cancel()
    {
        if(currentPlayer != null && currentPlayer.State == PlayState.MoveEnd)
        {
            currentPlayer.GoBack();
        }
    }

    public void StandBy()
    {
        if(currentPlayer != null && currentPlayer.State == PlayState.MoveEnd)
        {
            currentPlayer.StandBy();
            currentPlayer = null;
        }
    }

    public void NextTurn()
    {
        foreach(var player in allMyPlayers)
        {
            player.NextTurn();
        }
    }
}
