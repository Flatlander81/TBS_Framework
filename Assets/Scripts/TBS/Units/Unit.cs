using UnityEngine;
using TBS.Grid;
using TBS.Combat;
using TBS.Events;
using System.Collections.Generic;

namespace TBS.Units
{
    /// <summary>
    /// Base class for all units in the game.
    /// Handles stats, equipment, movement, and action points.
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [Header("Unit Identity")]
        [SerializeField] private string unitName = "Soldier";
        [SerializeField] private UnitFaction faction = UnitFaction.Player;

        [Header("Base Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int baseMovementRange = 4;
        [SerializeField] private int baseAccuracy = 75;
        [SerializeField] private int baseDefense = 0;

        [Header("Equipment")]
        [SerializeField] private WeaponData equippedWeapon;

        [Header("Current State")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private int currentActionPoints = 2;

        private GridManager gridManager;
        private GridTile currentTile;
        private bool isSelected = false;

        // Properties
        public string UnitName => unitName;
        public UnitFaction Faction => faction;
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public int MovementRange => baseMovementRange;
        public int CurrentActionPoints => currentActionPoints;
        public Vector2Int GridPosition => gridPosition;
        public WeaponData EquippedWeapon => equippedWeapon;
        public bool IsAlive => currentHealth > 0;
        public bool IsSelected => isSelected;

        // Calculated Properties
        public int TotalAccuracy => baseAccuracy + (equippedWeapon != null ? equippedWeapon.AccuracyModifier : 0);
        public int TotalDefense => baseDefense;

        private void Start()
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                UpdateGridPosition(gridPosition);
            }

            // Register with turn manager
            Core.TurnManager turnManager = FindObjectOfType<Core.TurnManager>();
            if (turnManager != null)
            {
                if (faction == UnitFaction.Player)
                    turnManager.RegisterPlayerUnit(this);
                else
                    turnManager.RegisterEnemyUnit(this);
            }
        }

        /// <summary>
        /// Initializes the unit at a specific grid position.
        /// </summary>
        public void Initialize(Vector2Int position, GridManager grid)
        {
            gridManager = grid;
            UpdateGridPosition(position);
        }

        /// <summary>
        /// Updates the unit's grid position.
        /// </summary>
        private void UpdateGridPosition(Vector2Int newPosition)
        {
            // Clear old tile
            if (currentTile != null)
            {
                currentTile.ClearOccupyingUnit();
            }

            gridPosition = newPosition;
            currentTile = gridManager.GetTile(newPosition);

            if (currentTile != null)
            {
                currentTile.SetOccupyingUnit(this);
                transform.position = currentTile.GetWorldPosition() + Vector3.up * 0.5f;
            }
        }

        /// <summary>
        /// Resets action points at the start of a turn.
        /// </summary>
        public void ResetActionPoints(int points)
        {
            currentActionPoints = points;
            GameEvents.TriggerUnitActionPointsChanged(this, currentActionPoints);
        }

        /// <summary>
        /// Spends action points for an action.
        /// </summary>
        public bool SpendActionPoints(int cost)
        {
            if (currentActionPoints >= cost)
            {
                currentActionPoints -= cost;
                GameEvents.TriggerUnitActionPointsChanged(this, currentActionPoints);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the unit can afford an action.
        /// </summary>
        public bool CanAffordAction(int cost)
        {
            return currentActionPoints >= cost;
        }

        /// <summary>
        /// Moves the unit to a target tile.
        /// </summary>
        public bool MoveTo(Vector2Int targetPosition, int actionPointCost = 1)
        {
            if (!CanAffordAction(actionPointCost))
            {
                Debug.LogWarning($"{unitName} doesn't have enough action points to move.");
                return false;
            }

            GridTile targetTile = gridManager.GetTile(targetPosition);
            if (targetTile == null || !targetTile.IsWalkable)
            {
                Debug.LogWarning($"Cannot move to {targetPosition} - tile not walkable.");
                return false;
            }

            // Check if target is within movement range
            List<GridTile> reachableTiles = gridManager.GetReachableTiles(gridPosition, baseMovementRange);
            if (!reachableTiles.Contains(targetTile))
            {
                Debug.LogWarning($"Target position {targetPosition} is out of movement range.");
                return false;
            }

            Vector2Int oldPosition = gridPosition;
            UpdateGridPosition(targetPosition);
            SpendActionPoints(actionPointCost);

            GameEvents.TriggerUnitMoved(this, oldPosition, targetPosition);
            return true;
        }

        /// <summary>
        /// Equips a weapon to this unit.
        /// </summary>
        public void EquipWeapon(WeaponData weapon)
        {
            equippedWeapon = weapon;
            Debug.Log($"{unitName} equipped {weapon.WeaponName}");
        }

        /// <summary>
        /// Unequips the current weapon.
        /// </summary>
        public void UnequipWeapon()
        {
            equippedWeapon = null;
        }

        /// <summary>
        /// Takes damage and checks for death.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            int actualDamage = Mathf.Max(0, damage - TotalDefense);
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);

            GameEvents.TriggerUnitDamaged(this, actualDamage);

            if (currentHealth == 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heals the unit.
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive) return;

            int actualHealing = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += actualHealing;

            GameEvents.TriggerUnitHealed(this, actualHealing);
        }

        /// <summary>
        /// Handles unit death.
        /// </summary>
        private void Die()
        {
            if (currentTile != null)
            {
                currentTile.ClearOccupyingUnit();
            }

            GameEvents.TriggerUnitDied(this);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Selects this unit.
        /// </summary>
        public void Select()
        {
            isSelected = true;
            GameEvents.TriggerUnitSelected(this);

            // Highlight movement range
            if (currentActionPoints > 0)
            {
                gridManager.ClearAllHighlights();
                gridManager.HighlightMovementRange(gridPosition, baseMovementRange);
            }
        }

        /// <summary>
        /// Deselects this unit.
        /// </summary>
        public void Deselect()
        {
            isSelected = false;
            GameEvents.TriggerUnitDeselected(this);
            gridManager.ClearAllHighlights();
        }

        /// <summary>
        /// Gets a list of tiles within weapon range.
        /// </summary>
        public List<GridTile> GetAttackRange()
        {
            if (equippedWeapon == null)
                return new List<GridTile>();

            return gridManager.GetTilesInRange(gridPosition, equippedWeapon.Range);
        }

        /// <summary>
        /// Checks if a target is within attack range.
        /// </summary>
        public bool IsInAttackRange(Vector2Int targetPosition)
        {
            if (equippedWeapon == null)
                return false;

            int distance = gridManager.GetDistance(gridPosition, targetPosition);
            return distance <= equippedWeapon.Range;
        }

        /// <summary>
        /// Gets the current tile this unit is standing on.
        /// </summary>
        public GridTile GetCurrentTile()
        {
            return currentTile;
        }
    }

    /// <summary>
    /// Unit faction/team designation.
    /// </summary>
    public enum UnitFaction
    {
        Player,
        Enemy
    }
}
