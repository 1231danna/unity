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
    Tilemap defenseTilemap = default;
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

    public Player currentPlayer = null;
    public static GameBoard instance;
    [SerializeField]
    Player[] playerPrefabs;
    public List<Player> allMyPlayers = new List<Player>();
    public List<Player> GetPlayersByTeam(PlayerType type)
    {
        List<Player> result = new List<Player>();
        foreach(var p in allMyPlayers)
        {
            if(p != null && p.team == type)
            {
                result.Add(p);
            }
        } 
        return result;

    }

    public LogicTile currentPreviewTile = null;

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

        int index = 0;
        foreach(var pos in walkTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = walkTilemap.CellToWorld(pos);
            allLogicTileDict.Add(worldPos, logicTiles[index]);
            logicTiles[index].LocalPos = pos;
            if(cannotWalkTilemap.HasTile(pos))
            {
                logicTiles[index].isWalkable = false;
            }
            if (defenseTilemap != null && defenseTilemap.HasTile(pos))
            {
                logicTiles[index].terrainDefense = 2;
                logicTiles[index].moveCost = 2;
            }
            index++;
        }

    }

    public void ClearAllUITiles()
    {
        for(int i = 0; i < uiTileObjects.Count; i++)
        {
            Destroy(uiTileObjects[i]);
        }
        uiTileObjects.Clear();
    }
    
    public void ShowUITile(Player player, LogicTile tile, int movePower, int attackRange, bool showMove = true, bool showAttack = true)
    {
        ClearAllUITiles();
        // findway
        FindMovePaths(player, tile, movePower);
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

    void FindMovePaths(Player player, LogicTile startTile, int movePower)
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
                search.Enqueue(tile.GrowNorth(player));
                search.Enqueue(tile.GrowEast(player));
                search.Enqueue(tile.GrowSouth(player));
                search.Enqueue(tile.GrowWest(player));
            }
        }

        for (int i = moveTiles.Count - 1; i >= 0; i--)
        {
            LogicTile tile = moveTiles[i];
            if (tile.PlayerOnTile != null && tile.PlayerOnTile != player && !tile.PlayerOnTile.isCover)
            {
                moveTiles.RemoveAt(i);
            }
        }
    }

    void FindAttackPaths(int attackRange)
    {
        attackTiles.Clear();
        // moveTiles Edge
        search.Clear();
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

    IEnumerator ShowAOETemporary(Player attacker, LogicTile centerTile, List<Vector2Int> shape)
    {
        
        attacker.ForceFaceTarget(centerTile);
        
        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, centerTile);
        Debug.Log($"[DEBUG] 预览方向: {facing}");
        
        var area = GetAOEArea(centerTile, facing, shape);
        ShowOneUITile(area, UITileType.Attack);

        yield return new WaitForSeconds(1.0f);

        CombatManager.instance.ExecuteAOE(attacker, centerTile, shape);

        ClearAllUITiles();
    }
    
    public void ClickOneTile(LogicTile tile)
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if(tile == null)
        {
            return;
        }

        if(UIManager.instance.systemMenu.activeSelf)
        {
            UIManager.instance.HideAllMenus();
            return;
        }

        if (currentPlayer != null && currentPlayer.State == PlayState.Grey)
        {
            currentPlayer = null;
            return;
        }
        
        if(currentPlayer != null && currentPlayer.State == PlayState.Moving)
        {
            return;
        }

        if (UIManager.instance.actionMenu.activeSelf)
        {
            if (IsMoveRange(tile) || IsAttackRange(tile))
            {
                return;
            }
            else
            {
                CancelSelection();
                if (UIManager.instance != null)
                {
                    UIManager.instance.HideActionMenu();
                }
                return;
            }
        }

        if(currentPlayer == null)
        {
            var player = tile.PlayerOnTile;
            if(player != null)
            {
               if(player.CanBeSelected())
                {
                    currentPlayer = player;
                    if (UIManager.instance != null)
                    {
                        UIManager.instance.ShowActionMenu(currentPlayer);
                    }
                }
                else
                {
                    ShowUITile(player, player.Tile, player.MovePower, player.AttackRange);
                }
            }
            else
            {
                currentPlayer = null;
                UIManager.instance.ShowSystemMenu();
                ClearAllUITiles();
            }
        }
        else
        {
            if(IsMoveRange(tile))
            {
                if (currentPlayer.isTank)
                {
                    var path = MoveToDestination(currentPlayer.Tile, tile);
                    foreach (var pTile in path)
                    {
                        if (pTile.CoverOnTile != null)
                        {
                            Destroy(pTile.CoverOnTile.gameObject);
                            pTile.CoverOnTile = null;
                        }
                    }
                }
                
                currentPlayer.MoveTo(tile);
            }
            else if (IsAttackRange(tile))
            {
                if(currentPlayer != null)
                {
                    var shape = currentPlayer.GetMyShape();
                    Player targetEntity = tile.PlayerOnTile;
                    if (targetEntity == null)
                    {
                        targetEntity = tile.CoverOnTile;
                    }
                    if (currentPlayer.AOEType == AOEType.Single && targetEntity == null)
                    {
                        return;
                    }

                    if (currentPreviewTile != tile)
                    {
                        if (currentPreviewTile != null)
                        {
                            CombatManager.instance.CancelAOEPreview(currentPlayer, currentPreviewTile, shape);
                        }
                        currentPreviewTile = tile;
                        currentPlayer.ForceFaceTarget(tile);
                        ShowAOEPreview(currentPlayer, tile);
                        CombatManager.instance.PreviewAOEDamage(currentPlayer, tile, shape);
                    }
                    else
                    {
                        CombatManager.instance.CancelAOEPreview(currentPlayer, tile, shape);
                        currentPreviewTile = null;

                        if (currentPlayer.AOEType == AOEType.Single)
                        {
                            CombatManager.instance.StartCombat(currentPlayer, targetEntity);
                        }
                        else
                        {
                            CombatManager.instance.ExecuteAOE(currentPlayer, tile, shape);
                        }
                        currentPlayer = null;
                        ClearAllUITiles();
                    }
                }
                else
                {
                    CancelSelection();
                }
            }
            else
            {
                if (currentPreviewTile != null && currentPlayer != null)
                {
                    CombatManager.instance.CancelAOEPreview(currentPlayer, currentPreviewTile, currentPlayer.GetMyShape());
                    currentPreviewTile = null;
                }
                if(currentPlayer.State == PlayState.MoveEnd)
                {
                    StandBy();
                    UIManager.instance.HideActionMenu();
                }
                else
                {
                    CancelSelection();
                    if (UIManager.instance != null)
                    {
                        UIManager.instance.HideActionMenu();
                    }
                }
            }

        }
        
    }

    bool IsMoveRange(LogicTile tile)
    {
        return moveTiles.Contains(tile);
    }

    public void InitPlayers(Vector2Int[] spawnCoords, int[] playerTypes)
    {
        int count = Math.Min(spawnCoords.Length, playerTypes.Length);
        for(int i = 0; i < count; i++)
        {
            int index = spawnCoords[i].y * tileSize.x + spawnCoords[i].x;
            
            if(index >=0 && index < logicTiles.Length)
            {
                CreatePlayerAtTile(logicTiles[index], playerTypes[i]);
            }
        }

        Player[] existingPlayers = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach(var p in existingPlayers)
        {
            if(p.Tile == null)
            {
                Vector3Int cellPos = walkTilemap.WorldToCell(p.transform.position);
                LogicTile tile = GetTileByPos(cellPos);
                
                if(tile != null)
                {
                    p.transform.position = walkTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0f, 0f);
                    p.Tile = tile;
                    if (p.isCover) 
                    {
                        tile.CoverOnTile = p;
                    }
                    else
                    {
                        tile.PlayerOnTile = p;
                    }
                    if(!allMyPlayers.Contains(p))
                    {
                        allMyPlayers.Add(p);
                    }
                }
            }
        }
    }

    void CreatePlayerAtTile(LogicTile tile, int playerType)
    {
        Player prefabToSpawn = playerPrefabs[playerType];
        if (prefabToSpawn.isCover)
        {
            if (tile.CoverOnTile != null)
            {
                Debug.LogWarning("Tile already has a cover on it!");
                return;
            }
        }
        else
        {
            if (tile.PlayerOnTile != null)
            {
                Debug.LogWarning("Tile already has a player on it!");
                return;
            }
        }

        var player = Instantiate(playerPrefabs[playerType]);
        player.transform.position = GetTileWorldPos(tile) + new Vector3(0.5f, 0f, 0f);
        player.Tile = tile;
        
        if (player.isCover)
        {
            tile.CoverOnTile = player;
        }
        else
        {
            tile.PlayerOnTile = player;
        }
        
        allMyPlayers.Add(player);
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

    bool IsAttackRange(LogicTile tile)
    {
        return attackTiles.Contains(tile);
    }

    void CancelSelection()
    {
        if(currentPlayer != null)
        {
            currentPlayer.Recover();
            currentPlayer = null;
            ClearAllUITiles();
        }
    }

    public void RemovePlayerFromList(Player player)
    {
        if(allMyPlayers.Contains(player))
        {
            allMyPlayers.Remove(player);
        }
    }

    public LogicTile GetBestMoveTile(Player self, Player target)
    {
        foreach(var tile in logicTiles)
        {
            tile.Clear();
        }

        moveTiles.Clear();
        search.Clear();

        LogicTile startTile = self.Tile;
        search.Enqueue(startTile);
        startTile.MarkAsStart(self.MovePower);

        while(search.Count > 0)
        {
            var tile = search.Dequeue();
            if(tile != null)
            {
                moveTiles.Add(tile);
                search.Enqueue(tile.GrowNorth(self));
                search.Enqueue(tile.GrowEast(self));
                search.Enqueue(tile.GrowSouth(self));
                search.Enqueue(tile.GrowWest(self));
            }
        }

        LogicTile bestTile = self.Tile;
        int minDist = int.MaxValue;

        foreach(var tile in moveTiles)
        {
            if(tile.PlayerOnTile != null && tile.PlayerOnTile != self) continue;
            
            int d = Mathf.Abs(tile.X - target.Tile.X) + Mathf.Abs(tile.Y - target.Tile.Y);
            if(d < minDist)
            {
                minDist = d;
                bestTile = tile;
            }
        }

        ClearAllUITiles();
        return bestTile;
    }
    
    public static readonly List<Vector2Int> SingleShape = new List<Vector2Int> { new Vector2Int(0, 0) };
    public static readonly List<Vector2Int> PierceShape = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1) };
    public static readonly List<Vector2Int> CrossShape = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(1,0), new Vector2Int(-1,0) };
    public static readonly List<Vector2Int> SquareShape = new List<Vector2Int> { new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1,-1) };
    public static readonly List<Vector2Int> FanShape = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-2, 2), new Vector2Int(-1, 2), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) };

    public LogicTile GetTileByCoords(int x, int y)
    {
        if (x >= 0 && x < tileSize.x && y >= 0 && y < tileSize.y)
        {
            int index = y * tileSize.x + x;

            if (index >= 0 && index < logicTiles.Length)
            {
                return logicTiles[index];
            }
        }

        return null;
    }

    public List<LogicTile> GetAOEArea(LogicTile center, Vector2Int facing, List<Vector2Int> shape)
    {
        List<LogicTile> results = new List<LogicTile>();
        
        foreach (var offset in shape)
        {
            int rx = 0, ry = 0;
            if (facing.y > 0) { rx = offset.x; ry = offset.y; }
            else if (facing.y < 0) { rx = -offset.x; ry = -offset.y; }
            else if (facing.x < 0) { rx = -offset.y; ry = offset.x; }
            else if (facing.x > 0) { rx = offset.y; ry = -offset.x; }
            else { rx = offset.x; ry = offset.y; }
            
            LogicTile t = GetTileByCoords(center.X + rx, center.Y + ry);
            if (t != null) results.Add(t);
        }
        return results;
    }

    public void ShowAOEPreview(Player attacker, LogicTile centerTile)
    {
        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, centerTile);

        attacker.ForceFaceTarget(centerTile);

        var shape = attacker.GetMyShape();
        var area = GetAOEArea(centerTile, facing, shape);

        ShowOneUITile(area, UITileType.Attack);
    }

}
