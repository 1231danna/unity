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

    public float GetAffinityMultiplier(FactionType attackerFaction, FactionType defenderFaction)
    {
        if (attackerFaction == FactionType.German && defenderFaction == FactionType.Allied)
        {
            return 1.15f;
        }
        return 1.0f;
    }

    void Awake()
    {
        instance = this;
    }

    public int CalculateDamage(Player attacker, Player defender)
    {
        float multiplier = GetAffinityMultiplier(attacker.FactionType, defender.FactionType);
        int finalATK = Mathf.RoundToInt(attacker.Attack * multiplier);
        int finalDefense = defender.Defense;
        if (defender.Tile != null)
        {
            finalDefense += defender.Tile.terrainDefense;
            if (defender.Tile.CoverOnTile != null && defender.Tile.CoverOnTile.coverType == 2)
            {
                if (!defender.isCover)
                {
                    finalDefense += 20;
                }
            }
        }
        return Mathf.Max(0, finalATK - finalDefense);
    }

   public void StartCombat(Player attacker, Player defender)
    {
        StartCoroutine(CombatSequence(attacker, defender));
    }
    private IEnumerator CombatSequence(Player attacker, Player defender)
    {
    
        attacker.PlayAttackAnimation(defender.Tile);
    
        yield return new WaitForSeconds(0.5f); 

        ExecuteAttack(attacker, defender);

        yield return new WaitForSeconds(0.3f);

        if (!defender.IsDead && !defender.isCover)
        {
            int distance = Mathf.Abs(attacker.EndTile.X - defender.Tile.X) + Mathf.Abs(attacker.EndTile.Y - defender.Tile.Y);
            if (distance <= defender.AttackRange)
            {

                defender.PlayAttackAnimation(attacker.Tile);
                
                yield return new WaitForSeconds(0.5f);
                
                ExecuteAttack(defender, attacker);
                
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                Debug.Log($"{defender.name} 无法反击，因为 {attacker.name} 在攻击范围外");
            }
        }

        yield return new WaitForSeconds(0.2f);
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
  
        StartCoroutine(AOESequence(attacker, targetTile, shape));
    }

    private IEnumerator AOESequence(Player attacker, LogicTile targetTile, List<Vector2Int> shape)
    {
        string aoeName = attacker.GetMyShape().ToString();
        Debug.Log($" {attacker.name} 发动了 {aoeName} ");
        
        attacker.ForceFaceTarget(targetTile);
        Vector2Int facing = Player.GetDirectionTo(attacker.Tile, targetTile);

        attacker.PlayAttackAnimation(targetTile);

        // wait for Animation
        yield return new WaitForSeconds(0.5f);

        List<LogicTile> area = GameBoard.instance.GetAOEArea(targetTile, facing, shape);
        
        Player mainTarget = targetTile.PlayerOnTile;
        if (mainTarget == null) mainTarget = targetTile.CoverOnTile;
        
        if (attacker.AOEType == AOEType.Single && mainTarget == null)
        {
            Debug.Log("单体攻击无法打空地");
            attacker.StandBy();
            yield break; 
        }
        
        foreach (var tile in area)
        {
            if (tile == null) continue;

            if (tile.PlayerOnTile != null && tile.PlayerOnTile != attacker)
            {
                Player target = tile.PlayerOnTile;
                int damage = CalculateDamage(attacker, tile.PlayerOnTile);
                Debug.Log($"AOE命中了 {tile.PlayerOnTile.name}造成 {damage} 点伤害");
                tile.PlayerOnTile.TakeDamage(damage);
            }

            if (tile.CoverOnTile != null && tile.CoverOnTile != attacker)
            {
                Player targetCover = tile.CoverOnTile;
                int damage = CalculateDamage(attacker, targetCover);
                Debug.Log($"AOE击中了掩体 {targetCover.name} 造成 {damage} 点伤害");
                targetCover.TakeDamage(damage);
            }

        }

        yield return new WaitForSeconds(0.3f);

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

            if (tile.PlayerOnTile != null && tile.PlayerOnTile != attacker)
            {
                Player targetPlayer = tile.PlayerOnTile;
                int predictedDamage = CalculateDamage(attacker, targetPlayer);
                
                if (targetPlayer.healthBar != null)
                {
                    targetPlayer.healthBar.ShowPreview(targetPlayer.currentHP, predictedDamage, targetPlayer.maxHP);
                }

                if (attacker.AOEType == AOEType.Single && !targetPlayer.isCover)
                {
                    if (targetPlayer.currentHP - predictedDamage > 0)
                    {
                        int distance = Mathf.Abs(attacker.Tile.X - targetPlayer.Tile.X) + Mathf.Abs(attacker.Tile.Y - targetPlayer.Tile.Y);
                        if (distance <= targetPlayer.AttackRange)
                        {
                            totalCounterDamage += CalculateDamage(targetPlayer, attacker);
                        }
                    }
                }
            }
                
            if (tile.CoverOnTile != null && tile.CoverOnTile != attacker)
            {
                Player targetCover = tile.CoverOnTile;
                int predictedDamage = CalculateDamage(attacker, targetCover);

                if (targetCover.healthBar != null)
                {
                        targetCover.healthBar.ShowPreview(targetCover.currentHP, predictedDamage, targetCover.maxHP);
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
            if (tile.PlayerOnTile != null && tile.PlayerOnTile != attacker && tile.PlayerOnTile.healthBar != null)
            {
                tile.PlayerOnTile.healthBar.CancelPreview(tile.PlayerOnTile.currentHP, tile.PlayerOnTile.maxHP);
            }

            if (tile.CoverOnTile != null && tile.CoverOnTile != attacker && tile.CoverOnTile.healthBar != null)
            {
                tile.CoverOnTile.healthBar.CancelPreview(tile.CoverOnTile.currentHP, tile.CoverOnTile.maxHP);
            }
        }

        if (attacker.healthBar != null)
        {
            attacker.healthBar.CancelPreview(attacker.currentHP, attacker.maxHP);
        }
    }

 



}