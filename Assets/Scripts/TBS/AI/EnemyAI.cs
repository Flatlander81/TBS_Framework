using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TBS.Units;
using TBS.Core;
using TBS.Grid;
using TBS.Combat;
using TBS.Events;

namespace TBS.AI
{
    /// <summary>
    /// AI controller for enemy units.
    /// Uses a simple behavior: move towards nearest enemy, attack if in range.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float actionDelay = 1f; // Delay between AI actions for visibility

        private TurnManager turnManager;
        private GridManager gridManager;
        private CombatSystem combatSystem;
        private bool isProcessing = false;

        private void Start()
        {
            turnManager = FindObjectOfType<TurnManager>();
            gridManager = FindObjectOfType<GridManager>();
            combatSystem = FindObjectOfType<CombatSystem>();

            // Subscribe to turn phase changes
            GameEvents.OnTurnPhaseChanged += OnTurnPhaseChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnTurnPhaseChanged -= OnTurnPhaseChanged;
        }

        private void OnTurnPhaseChanged(TurnPhase phase)
        {
            if (phase == TurnPhase.EnemyTurn && !isProcessing)
            {
                StartCoroutine(ExecuteEnemyTurn());
            }
        }

        private IEnumerator ExecuteEnemyTurn()
        {
            isProcessing = true;

            yield return new WaitForSeconds(actionDelay);

            List<Unit> enemyUnits = turnManager.GetEnemyUnits();
            List<Unit> playerUnits = turnManager.GetPlayerUnits();

            // Process each enemy unit
            foreach (Unit enemy in enemyUnits)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                // Process all action points for this unit
                while (enemy.CurrentActionPoints > 0)
                {
                    bool actionTaken = ProcessUnitTurn(enemy, playerUnits);

                    if (!actionTaken)
                        break; // No valid actions, move to next unit

                    yield return new WaitForSeconds(actionDelay);
                }
            }

            // End enemy turn after all units have acted
            yield return new WaitForSeconds(actionDelay);
            turnManager.EndCurrentPhase();

            isProcessing = false;
        }

        private bool ProcessUnitTurn(Unit enemy, List<Unit> playerUnits)
        {
            // Find nearest player unit
            Unit nearestTarget = FindNearestTarget(enemy, playerUnits);

            if (nearestTarget == null)
                return false;

            // Check if we can attack
            if (enemy.EquippedWeapon != null && enemy.IsInAttackRange(nearestTarget.GridPosition))
            {
                // Attack if in range
                if (combatSystem.CanAttack(enemy, nearestTarget, out string reason))
                {
                    combatSystem.ExecuteAttack(enemy, nearestTarget);
                    return true;
                }
            }

            // Move towards target
            if (enemy.CurrentActionPoints > 0)
            {
                Vector2Int moveTarget = FindMoveTowardsTarget(enemy, nearestTarget);
                if (moveTarget != enemy.GridPosition)
                {
                    enemy.MoveTo(moveTarget, 1);
                    return true;
                }
            }

            return false;
        }

        private Unit FindNearestTarget(Unit enemy, List<Unit> potentialTargets)
        {
            Unit nearest = null;
            int shortestDistance = int.MaxValue;

            foreach (Unit target in potentialTargets)
            {
                if (target == null || !target.IsAlive)
                    continue;

                int distance = gridManager.GetDistance(enemy.GridPosition, target.GridPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearest = target;
                }
            }

            return nearest;
        }

        private Vector2Int FindMoveTowardsTarget(Unit enemy, Unit target)
        {
            // Get all reachable tiles
            List<GridTile> reachableTiles = gridManager.GetReachableTiles(enemy.GridPosition, enemy.MovementRange);

            if (reachableTiles.Count == 0)
                return enemy.GridPosition;

            // Find the reachable tile closest to the target
            GridTile bestTile = null;
            int shortestDistance = int.MaxValue;

            foreach (GridTile tile in reachableTiles)
            {
                int distance = gridManager.GetDistance(tile.GridPosition, target.GridPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestTile = tile;
                }
            }

            return bestTile != null ? bestTile.GridPosition : enemy.GridPosition;
        }

        /// <summary>
        /// Advanced: Find best cover position near target.
        /// Can be used for more sophisticated AI.
        /// </summary>
        private GridTile FindBestCoverPosition(Unit enemy, Unit target, List<GridTile> reachableTiles)
        {
            GridTile bestPosition = null;
            float bestScore = float.MinValue;

            foreach (GridTile tile in reachableTiles)
            {
                float score = 0f;

                // Prefer tiles with cover
                switch (tile.Cover)
                {
                    case CoverType.Half:
                        score += 10f;
                        break;
                    case CoverType.Full:
                        score += 20f;
                        break;
                }

                // Prefer closer to target (but not too close)
                int distance = gridManager.GetDistance(tile.GridPosition, target.GridPosition);
                int optimalRange = enemy.EquippedWeapon != null ? enemy.EquippedWeapon.Range / 2 : 3;
                score -= Mathf.Abs(distance - optimalRange) * 2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = tile;
                }
            }

            return bestPosition;
        }
    }
}
