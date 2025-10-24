using UnityEngine;
using TBS.Units;
using TBS.Grid;
using TBS.Combat;

namespace TBS.Core
{
    /// <summary>
    /// Handles player input for unit selection, movement, and combat.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask tileLayer;
        [SerializeField] private LayerMask unitLayer;

        private GridManager gridManager;
        private TurnManager turnManager;
        private CombatSystem combatSystem;
        private Unit selectedUnit;
        private PlayerState currentState = PlayerState.SelectingUnit;

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            gridManager = FindObjectOfType<GridManager>();
            turnManager = FindObjectOfType<TurnManager>();
            combatSystem = FindObjectOfType<CombatSystem>();
        }

        private void Update()
        {
            if (turnManager != null && !turnManager.IsPlayerTurn())
                return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                HandleLeftClick();
            }
            else if (Input.GetMouseButtonDown(1)) // Right click
            {
                HandleRightClick();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectUnit();
            }
        }

        private void HandleLeftClick()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            switch (currentState)
            {
                case PlayerState.SelectingUnit:
                    TrySelectUnit(ray);
                    break;

                case PlayerState.UnitSelected:
                    TrySelectAction(ray);
                    break;
            }
        }

        private void HandleRightClick()
        {
            if (currentState == PlayerState.UnitSelected)
            {
                DeselectUnit();
            }
        }

        private void TrySelectUnit(Ray ray)
        {
            // Try to select a unit
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null && unit.Faction == UnitFaction.Player && unit.IsAlive)
                {
                    SelectUnit(unit);
                }
            }
        }

        private void TrySelectAction(Ray ray)
        {
            // First check if clicking on another unit for attack
            if (Physics.Raycast(ray, out RaycastHit unitHit, 100f, unitLayer))
            {
                Unit targetUnit = unitHit.collider.GetComponent<Unit>();
                if (targetUnit != null && targetUnit != selectedUnit)
                {
                    if (targetUnit.Faction == UnitFaction.Player)
                    {
                        // Select different player unit
                        SelectUnit(targetUnit);
                    }
                    else if (targetUnit.Faction == UnitFaction.Enemy)
                    {
                        // Attack enemy unit
                        TryAttack(targetUnit);
                    }
                    return;
                }
            }

            // Check if clicking on a tile for movement
            if (Physics.Raycast(ray, out RaycastHit tileHit, 100f, tileLayer))
            {
                GridTile tile = tileHit.collider.GetComponent<GridTile>();
                if (tile != null)
                {
                    TryMove(tile);
                }
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (selectedUnit != null)
            {
                selectedUnit.Deselect();
            }

            selectedUnit = unit;
            selectedUnit.Select();
            currentState = PlayerState.UnitSelected;
        }

        private void DeselectUnit()
        {
            if (selectedUnit != null)
            {
                selectedUnit.Deselect();
                selectedUnit = null;
            }

            currentState = PlayerState.SelectingUnit;
        }

        private void TryMove(GridTile targetTile)
        {
            if (selectedUnit == null || !selectedUnit.CanAffordAction(1))
                return;

            if (selectedUnit.MoveTo(targetTile.GridPosition, 1))
            {
                // Movement successful
                gridManager.ClearAllHighlights();

                // Show new movement range if unit still has action points
                if (selectedUnit.CurrentActionPoints > 0)
                {
                    gridManager.HighlightMovementRange(selectedUnit.GridPosition, selectedUnit.MovementRange);
                }
            }
        }

        private void TryAttack(Unit target)
        {
            if (selectedUnit == null || selectedUnit.EquippedWeapon == null)
                return;

            if (combatSystem.CanAttack(selectedUnit, target, out string reason))
            {
                combatSystem.ExecuteAttack(selectedUnit, target);

                // Update highlights after attack
                gridManager.ClearAllHighlights();
                if (selectedUnit.CurrentActionPoints > 0)
                {
                    gridManager.HighlightMovementRange(selectedUnit.GridPosition, selectedUnit.MovementRange);
                }
                else
                {
                    // No more actions, deselect
                    DeselectUnit();
                }
            }
            else
            {
                Debug.LogWarning($"Cannot attack: {reason}");
            }
        }

        /// <summary>
        /// Gets the currently selected unit.
        /// </summary>
        public Unit GetSelectedUnit()
        {
            return selectedUnit;
        }
    }

    /// <summary>
    /// Player controller state machine.
    /// </summary>
    public enum PlayerState
    {
        SelectingUnit,
        UnitSelected
    }
}
