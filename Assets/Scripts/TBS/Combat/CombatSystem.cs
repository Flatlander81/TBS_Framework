using System.Collections.Generic;
using UnityEngine;
using TBS.Units;
using TBS.Grid;
using TBS.Events;

namespace TBS.Combat
{
    /// <summary>
    /// Handles all combat calculations and attack execution.
    /// Manages hit chances, damage calculation, and different weapon types.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        private GridManager gridManager;

        private void Awake()
        {
            gridManager = FindFirstObjectByType<GridManager>();
        }

        /// <summary>
        /// Executes an attack from attacker to target.
        /// </summary>
        public AttackResult ExecuteAttack(Unit attacker, Unit target)
        {
            if (!CanAttack(attacker, target, out string reason))
            {
                Debug.LogWarning($"Cannot attack: {reason}");
                return new AttackResult { Success = false };
            }

            WeaponData weapon = attacker.EquippedWeapon;
            AttackResult result = new AttackResult();

            switch (weapon.Type)
            {
                case WeaponType.Ranged:
                    result = ExecuteRangedAttack(attacker, target);
                    break;
                case WeaponType.Melee:
                    result = ExecuteMeleeAttack(attacker, target);
                    break;
                case WeaponType.Spread:
                    result = ExecuteSpreadAttack(attacker, target);
                    break;
                case WeaponType.AOE:
                    result = ExecuteAOEAttack(attacker, target.GridPosition);
                    break;
            }

            if (result.Success)
            {
                attacker.SpendActionPoints(weapon.ActionPointCost);
                GameEvents.TriggerAttackExecuted(attacker, target, result);
            }

            return result;
        }

        /// <summary>
        /// Executes a ranged attack with distance-based accuracy falloff.
        /// </summary>
        private AttackResult ExecuteRangedAttack(Unit attacker, Unit target)
        {
            AttackResult result = new AttackResult();
            WeaponData weapon = attacker.EquippedWeapon;

            int distance = gridManager.GetDistance(attacker.GridPosition, target.GridPosition);
            GridTile targetTile = target.GetCurrentTile();
            CoverType cover = targetTile != null ? targetTile.Cover : CoverType.None;

            result.HitChance = weapon.CalculateHitChance(attacker.TotalAccuracy, distance, cover);
            result.Hit = RollHit(result.HitChance);
            result.Success = true;

            if (result.Hit)
            {
                result.Damage = weapon.CalculateDamage();
                target.TakeDamage(result.Damage);
            }

            return result;
        }

        /// <summary>
        /// Executes a melee attack (adjacent tiles, high accuracy).
        /// </summary>
        private AttackResult ExecuteMeleeAttack(Unit attacker, Unit target)
        {
            AttackResult result = new AttackResult();
            WeaponData weapon = attacker.EquippedWeapon;

            // Melee attacks ignore cover and have minimal distance penalty
            result.HitChance = weapon.BaseAccuracy + attacker.TotalAccuracy;
            result.HitChance = Mathf.Clamp(result.HitChance, 5f, 95f);
            result.Hit = RollHit(result.HitChance);
            result.Success = true;

            if (result.Hit)
            {
                result.Damage = weapon.CalculateDamage();
                target.TakeDamage(result.Damage);
            }

            return result;
        }

        /// <summary>
        /// Executes a spread attack affecting a cone of targets.
        /// </summary>
        private AttackResult ExecuteSpreadAttack(Unit attacker, Unit target)
        {
            AttackResult result = new AttackResult();
            WeaponData weapon = attacker.EquippedWeapon;

            // Get tiles in the spread cone
            List<GridTile> affectedTiles = GetSpreadTiles(attacker.GridPosition, target.GridPosition, weapon.SpreadAngle);

            int totalDamage = 0;
            int hits = 0;

            foreach (GridTile tile in affectedTiles)
            {
                if (tile.IsOccupied)
                {
                    Unit tileUnit = tile.OccupyingUnit;
                    if (tileUnit.Faction != attacker.Faction)
                    {
                        int distance = gridManager.GetDistance(attacker.GridPosition, tile.GridPosition);
                        float hitChance = weapon.CalculateHitChance(attacker.TotalAccuracy, distance, tile.Cover);

                        if (RollHit(hitChance))
                        {
                            int damage = weapon.CalculateDamage();
                            tileUnit.TakeDamage(damage);
                            totalDamage += damage;
                            hits++;
                        }
                    }
                }
            }

            result.Success = true;
            result.Hit = hits > 0;
            result.Damage = totalDamage;
            result.HitChance = weapon.BaseAccuracy; // Average hit chance

            return result;
        }

        /// <summary>
        /// Executes an AOE attack affecting multiple tiles.
        /// </summary>
        private AttackResult ExecuteAOEAttack(Unit attacker, Vector2Int targetPosition)
        {
            AttackResult result = new AttackResult();
            WeaponData weapon = attacker.EquippedWeapon;

            // Get all tiles in AOE radius
            List<GridTile> affectedTiles = gridManager.GetTilesInRange(targetPosition, weapon.AOERadius);

            int totalDamage = 0;
            int hits = 0;

            foreach (GridTile tile in affectedTiles)
            {
                if (tile.IsOccupied)
                {
                    Unit tileUnit = tile.OccupyingUnit;
                    // AOE can damage anyone, including friendlies
                    int damage = weapon.CalculateDamage();
                    tileUnit.TakeDamage(damage);
                    totalDamage += damage;
                    hits++;
                }
            }

            result.Success = true;
            result.Hit = hits > 0;
            result.Damage = totalDamage;
            result.HitChance = 100f; // AOE always hits the area

            return result;
        }

        /// <summary>
        /// Gets tiles in a spread/cone pattern.
        /// </summary>
        private List<GridTile> GetSpreadTiles(Vector2Int origin, Vector2Int target, int spreadAngle)
        {
            List<GridTile> tiles = new List<GridTile>();

            // Simple implementation: get tiles in a line and neighbors
            Vector2Int direction = target - origin;
            int distance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);

            for (int i = 1; i <= distance; i++)
            {
                Vector2Int checkPos = origin + new Vector2Int(
                    Mathf.RoundToInt((float)direction.x * i / distance),
                    Mathf.RoundToInt((float)direction.y * i / distance)
                );

                GridTile tile = gridManager.GetTile(checkPos);
                if (tile != null)
                {
                    tiles.Add(tile);

                    // Add adjacent tiles for spread effect
                    foreach (GridTile neighbor in gridManager.GetNeighbors(checkPos))
                    {
                        if (!tiles.Contains(neighbor))
                            tiles.Add(neighbor);
                    }
                }
            }

            return tiles;
        }

        /// <summary>
        /// Checks if attacker can attack target.
        /// </summary>
        public bool CanAttack(Unit attacker, Unit target, out string reason)
        {
            reason = "";

            if (attacker == null || target == null)
            {
                reason = "Invalid attacker or target";
                return false;
            }

            if (!attacker.IsAlive || !target.IsAlive)
            {
                reason = "Unit is not alive";
                return false;
            }

            if (attacker.EquippedWeapon == null)
            {
                reason = "No weapon equipped";
                return false;
            }

            if (!attacker.CanAffordAction(attacker.EquippedWeapon.ActionPointCost))
            {
                reason = "Not enough action points";
                return false;
            }

            if (!attacker.IsInAttackRange(target.GridPosition))
            {
                reason = "Target out of range";
                return false;
            }

            if (attacker.EquippedWeapon.Type != WeaponType.AOE)
            {
                if (!gridManager.HasLineOfSight(attacker.GridPosition, target.GridPosition))
                {
                    reason = "No line of sight";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Rolls to determine if an attack hits.
        /// </summary>
        private bool RollHit(float hitChance)
        {
            float roll = Random.Range(0f, 100f);
            return roll <= hitChance;
        }
    }

    /// <summary>
    /// Result of an attack action.
    /// </summary>
    public class AttackResult
    {
        public bool Success;    // Whether the attack action was executed
        public bool Hit;        // Whether the attack hit
        public int Damage;      // Total damage dealt
        public float HitChance; // The calculated hit chance percentage
    }
}
