using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    GameBoard gameBoard = default;
    [SerializeField]
    //UIManager uIManager = default;

    public static CombatManager instance;

    float[,] affinityChart = new float[,]
    {
        {1.0f, 0.5f, 2.0f, 1.0f, 1.0f},//Sword
        {2.0f, 1.0f, 0.5f, 1.0f, 1.0f},//Lance
        {0.5f, 2.0f, 1.0f, 1.0f, 1.0f},//Axe
        {1.0f, 1.0f, 1.0f, 1.0f, 1.0f},//Bow
        {1.0f, 1.0f, 1.0f, 1.0f, 1.0f}//Magic
        //Sword, lance, Axe, Bow, Magic
    };

    public float GetAffinityMultiplier(UnitType Attacker, UnitType Defender)
    {
        return affinityChart[(int)Attacker, (int)Defender];
    }

    void Awake()
    {
        instance = this;
    }

    public int CalculateDamage(Player attacker, Player defender)
    {
        float multiplier = GetAffinityMultiplier(attacker.UnitType, defender.UnitType);
        int finalATK = Mathf.RoundToInt(attacker.Attack * multiplier);
        return Mathf.Max(0, finalATK - defender.Defense);
    }

    public void StartCombat(Player attacker, Player defender)
    {
        ExecuteAttack(attacker, defender);

        if(!defender.IsDead)
        {
            int distance = Mathf.Abs(attacker.EndTile.X - defender.Tile.X) + Mathf.Abs(attacker.EndTile.Y - defender.Tile.Y);
            if (distance <= defender.AttackRange)
            {
                ExecuteAttack(defender, attacker);
            }
            else
            {
                Debug.Log($"{defender.name} 无法反击，因为 {attacker.name} 在攻击范围外");
            }

        }

        attacker.StandBy();
    }

    public void ExecuteAttack(Player attacker, Player defender)
    {
        int damage = CalculateDamage(attacker, defender);
        
        Debug.Log($"{attacker.name} 对 {defender.name} 造成了 {damage} 点伤害！");
        defender.TakeDamage(damage);
    }

    public void ExecuteAOE(Player attacker, LogicTile targetTile, List<Vector2Int> shape)
    {
        string aoeName = attacker.GetMyShape().ToString();
        Debug.Log($" {attacker.name} 发动了 {aoeName} ");
        
        attacker.ForceFaceTarget(targetTile);

        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, targetTile);

        List<LogicTile> area = GameBoard.instance.GetAOEArea(targetTile, facing, shape);

        if (attacker.AOEType == AOEType.Single && targetTile.PlayerOnTile == null)
        {
            Debug.Log("单体攻击无法打空地");
            return;
        }
        
        foreach (var tile in area)
        {
            if (tile == null) continue;
            
            Player target = tile.PlayerOnTile;

            if (target != null && target != attacker)
            {
                int damage = CalculateDamage(attacker, target);

                target.TakeDamage(damage);
                Debug.Log($"AOE命中了 {target.name}造成 {damage} 点伤害");
            }
        }

        attacker.StandBy();
    }

    public void PreviewAOEDamage(Player attacker, LogicTile targetTile, List<Vector2Int> shape)
    {
        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, targetTile);
        List<LogicTile> area = GameBoard.instance.GetAOEArea(targetTile, facing, shape);

        int totalCounterDamage = 0;

        foreach (var tile in area)
        {
            if (tile == null) continue;
            Player target = tile.PlayerOnTile;

            if (target != null && target != attacker)
            {
                int predictedDamage = CalculateDamage(attacker, target);
                
                if (target.healthBar != null)
                {
                    target.healthBar.ShowPreview(target.currentHP, predictedDamage, target.maxHP);
                }

                if (attacker.AOEType == AOEType.Single)
                {
                    if (target.currentHP - predictedDamage > 0)
                    {
                        int distance = Mathf.Abs(attacker.Tile.X - target.Tile.X) + Mathf.Abs(attacker.Tile.Y - target.Tile.Y);
                        if (distance <= target.AttackRange)
                        {
                            totalCounterDamage += CalculateDamage(target, attacker);
                        }
                    }
                }
            }
        }

        if (totalCounterDamage > 0 && attacker.healthBar != null)
        {
            attacker.healthBar.ShowPreview(attacker.currentHP, totalCounterDamage, attacker.maxHP);
        }
    }

    public void CancelAOEPreview(Player attacker, LogicTile targetTile, List<Vector2Int> shape)
    {
        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, targetTile);
        List<LogicTile> area = GameBoard.instance.GetAOEArea(targetTile, facing, shape);

        foreach (var tile in area)
        {
            if (tile == null) continue;
            Player target = tile.PlayerOnTile;

            if (target != null && target != attacker)
            {
                if (target.healthBar != null)
                {
                    target.healthBar.CancelPreview(target.currentHP, target.maxHP);
                }
            }
        }

        if (attacker.healthBar != null)
        {
            attacker.healthBar.CancelPreview(attacker.currentHP, attacker.maxHP);
        }
    }

}
