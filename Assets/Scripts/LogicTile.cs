using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LogicTile
{

    int leftMoveCost;
    int distance;
    [SerializeField]
    int moveCost = 1;

    const int ATTACK_COST = 1;
    int leftAttackCost;

    public int X { get; private set; }
    public int Y { get; private set; }
    public TileBase Tile { get; private set; }
    public LogicTile NextOnPath { get; private set; }
    public Vector3Int LocalPos { get; set; }
    public Player PlayerOnTile { get; set; }
    LogicTile north, east, south, west;
    public bool HasPath => distance != int.MaxValue;

    public LogicTile(int x, int y, TileBase tile)
    {
        X = x;
        Y = y;
        Tile = tile;
    }

    public static void SetEWNeighbors(LogicTile east, LogicTile west)
    {
        east.west = west;
        west.east = east;
    }

    public static void SetNSNeighbors(LogicTile north, LogicTile south)
    {
        north.south = south;
        south.north = north;
    }

    public void Clear()
    {
        distance = int.MaxValue;
        leftMoveCost = 0;
        leftAttackCost = 0;
        NextOnPath = null;
    }

    public void MarkAsStart(int movement)
    {
        leftMoveCost = movement;
        distance = 0;
        NextOnPath = null;
    }

    public void MarkAsAttackRange(int range)
    {
        leftAttackCost = range;
    }

    LogicTile GrowPathTo(LogicTile neighbor)
    {
        if(!HasPath || neighbor == null || neighbor.HasPath || leftMoveCost < neighbor.moveCost) 
        {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.leftMoveCost = leftMoveCost - neighbor.moveCost;
        neighbor.NextOnPath = this;
        return neighbor;

    }

    public LogicTile GrowNorth() => GrowPathTo(north);
    public LogicTile GrowEast() => GrowPathTo(east);
    public LogicTile GrowSouth() => GrowPathTo(south);  
    public LogicTile GrowWest() => GrowPathTo(west);

    LogicTile GrowAttackPathTo(LogicTile neighbor)
    {
        if(!HasPath || neighbor == null || neighbor.HasPath || leftAttackCost < ATTACK_COST) 
        {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.leftAttackCost = leftAttackCost - ATTACK_COST;
        //neighbor.NextOnPath = this;
        return neighbor;

    }

    public LogicTile GrowAttackNorth() => GrowAttackPathTo(north);
    public LogicTile GrowAttackEast() => GrowAttackPathTo(east);
    public LogicTile GrowAttackSouth() => GrowAttackPathTo(south);  
    public LogicTile GrowAttackWest() => GrowAttackPathTo(west);


    static bool IsBoundTile(LogicTile tile, List<LogicTile> tiles)
    {
        if(tile.north != null && !tiles.Contains(tile.north))
        {
            return true;
        }
        if(tile.east != null && !tiles.Contains(tile.east))
        {
            return true;
        }
        if(tile.south != null && !tiles.Contains(tile.south))
        {
            return true;
        }
        if(tile.west != null && !tiles.Contains(tile.west))
        {
            return true;
        }

        return false;
    }
    
    public static List<LogicTile> GetBoundTiles(List<LogicTile> tiles)
    {
        List<LogicTile> result = new List<LogicTile>();
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if(IsBoundTile(tile, tiles))
            {
                result.Add(tile);
            }
        }
        return result;
    }


}
