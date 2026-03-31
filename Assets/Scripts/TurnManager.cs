using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public enum BattleState
{
    PlayerTurn,
    EnemyTurn,
    OtherTurn,
    Busy,
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public BattleState currentState { get; private set; } = BattleState.PlayerTurn;

    public int turnCount = 1;

    void Awake()
    {
        instance = this;
    }

    public void SetState(BattleState newState)
    {
        if(newState == BattleState.EnemyTurn)
        {
            if (GameBoard.instance.GetPlayersByTeam(PlayerType.Enemy).Count == 0)
            {
                Debug.Log("没有敌人，跳过敌人回合");
                SetState(BattleState.OtherTurn);
                return;
            }
        }
        else if (newState == BattleState.OtherTurn)
        {
            if (GameBoard.instance.GetPlayersByTeam(PlayerType.Other).Count == 0)
            {
                Debug.Log("没有other单位，跳过Other回合");
                SetState(BattleState.PlayerTurn);
                return;
            }
        }
        else if (newState == BattleState.PlayerTurn)
        {
            turnCount++;
            Debug.Log($"第 {turnCount} 回合 ");

            var players = GameBoard.instance.GetPlayersByTeam(PlayerType.Player);
            if(players.Count > 0)
            {
                CameraController.instance.FocusOn(players[0].transform.position);
            }
            
        }

        
        currentState = newState;
        Debug.Log($"回合切换到 {currentState}");

        switch (currentState)
        {
            case BattleState.PlayerTurn:
                GameBoard.instance.NextTurn();
                break;
            case BattleState.EnemyTurn:
                StartCoroutine(ExecuteComputerTurn(PlayerType.Enemy));
                break;
            case BattleState.OtherTurn:
                StartCoroutine(ExecuteComputerTurn(PlayerType.Other));
                break;
            case BattleState.Busy:
                break;
        }
    }

    IEnumerator ExecuteComputerTurn(PlayerType teamType)
    {
        GameBoard.instance.NextTurn();
        yield return new WaitForSeconds(1f);

        List<Player> units = GameBoard.instance.GetPlayersByTeam(teamType);

        foreach(var unit in units)
        {
            if(unit == null || unit.IsDead) continue;

            CameraController.instance.FocusOn(unit.transform.position);
            yield return new WaitForSeconds(0.8f);

            Player target = GetClosesEnemy(unit);

            if(target != null)
            {
                LogicTile bestTile = GameBoard.instance.GetBestMoveTile(unit, target);

                GameBoard.instance.ShowUITile(unit.Tile, unit.MovePower, unit.AttackRange);
                yield return new WaitForSeconds(0.5f);
                unit.MoveTo(bestTile);

                yield return new WaitUntil(() => unit.State == PlayState.MoveEnd);
                yield return new WaitForSeconds(0.5f);

                int dist = Mathf.Abs(unit.EndTile.X - target.Tile.X) + Mathf.Abs(unit.EndTile.Y - target.Tile.Y);
                if(dist <= unit.AttackRange)
                {
                    var shape = unit.GetMyShape();
                    GameBoard.instance.ShowAOEPreview(unit, target.Tile);
                    CombatManager.instance.PreviewAOEDamage(unit, target.Tile, shape);
                    yield return new WaitForSeconds(1.0f);
                    CombatManager.instance.CancelAOEPreview(unit, target.Tile, shape);
                    GameBoard.instance.ClearAllUITiles();
                    if (unit.AOEType == AOEType.Single)
                    {
                        CombatManager.instance.StartCombat(unit, target);
                    }
                    else
                    {
                        CombatManager.instance.ExecuteAOE(unit, target.Tile, shape);
                    }

                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    unit.StandBy();
                }
            }
            else
            {
                unit.StandBy();
            }
            yield return new WaitForSeconds(0.5f);
        }

        if(teamType == PlayerType.Enemy)
        {
            SetState(BattleState.OtherTurn);
        }
        else
        {
            SetState(BattleState.PlayerTurn);
        }
    }

    Player GetClosesEnemy(Player self)
    {
        Player closest = null;
        int minDist = int.MaxValue;

        foreach(var p in GameBoard.instance.allMyPlayers)
        {
            if(p != null && !p.IsDead && self.IsEnemy(p))
            {
                int d = Mathf.Abs(self.Tile.X - p.Tile.X) + Mathf.Abs(self.Tile.Y - p.Tile.Y);
                if(d < minDist)
                {
                    minDist = d;
                    closest = p;
                }
            }
        }
        return closest;
    }

}
