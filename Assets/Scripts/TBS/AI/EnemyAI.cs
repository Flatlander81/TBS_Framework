using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// Implements XCOM-style tactical AI with cover seeking, flanking, threat assessment, and unit archetypes.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float actionDelay = 1f; // Delay between AI actions for visibility
        [SerializeField] private bool enableTacticalAI = true; // Toggle for advanced AI features
        [SerializeField] private bool enableDebugLogs = false; // Enable detailed AI decision logging

        private TurnManager turnManager;
        private GridManager gridManager;
        private CombatSystem combatSystem;
        private bool isProcessing = false;

        // Track damage dealt by player units for threat assessment
        private Dictionary<Unit, int> recentDamageByUnit = new Dictionary<Unit, int>();

        private void Start()
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            gridManager = FindFirstObjectByType<GridManager>();
            combatSystem = FindFirstObjectByType<CombatSystem>();

            // Subscribe to turn phase changes
            GameEvents.OnTurnPhaseChanged += OnTurnPhaseChanged;
            GameEvents.OnAttackExecuted += OnAttackExecuted;
            GameEvents.OnTurnPhaseChanged += OnTurnStart;
        }

        private void OnDestroy()
        {
            GameEvents.OnTurnPhaseChanged -= OnTurnPhaseChanged;
            GameEvents.OnAttackExecuted -= OnAttackExecuted;
            GameEvents.OnTurnPhaseChanged -= OnTurnStart;
        }

        /// <summary>
        /// Track damage dealt by units for threat assessment.
        /// </summary>
        private void OnAttackExecuted(Unit attacker, Unit target, AttackResult result)
        {
            if (result.Hit && attacker.Faction == UnitFaction.Player)
            {
                if (!recentDamageByUnit.ContainsKey(attacker))
                    recentDamageByUnit[attacker] = 0;
                recentDamageByUnit[attacker] += result.Damage;
            }
        }

        /// <summary>
        /// Clear threat tracking at the start of each turn.
        /// </summary>
        private void OnTurnStart(TurnPhase phase)
        {
            if (phase == TurnPhase.PlayerTurn)
            {
                recentDamageByUnit.Clear();
            }
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
            if (enableTacticalAI)
            {
                return ProcessTacticalTurn(enemy, playerUnits);
            }
            else
            {
                // Legacy simple AI behavior
                return ProcessSimpleTurn(enemy, playerUnits);
            }
        }

        /// <summary>
        /// XCOM-style tactical AI with cover, flanking, and threat assessment.
        /// </summary>
        private bool ProcessTacticalTurn(Unit enemy, List<Unit> playerUnits)
        {
            // Get AI archetype for this unit
            AIArchetype archetype = GetUnitArchetype(enemy);

            // Find best target using tactical scoring
            Unit bestTarget = FindBestTarget(enemy, playerUnits, archetype);

            if (bestTarget == null)
                return false;

            // Evaluate all possible actions and pick the best one
            List<AIAction> possibleActions = EvaluateAllActions(enemy, bestTarget, playerUnits, archetype);

            if (possibleActions.Count == 0)
                return false;

            // Execute the highest scoring action
            AIAction bestAction = possibleActions.OrderByDescending(a => a.Score).First();

            if (enableDebugLogs)
            {
                Debug.Log($"[AI] {enemy.UnitName} executing {bestAction.Type} (score: {bestAction.Score:F1}) targeting {bestTarget.UnitName}");
            }

            return ExecuteAction(enemy, bestTarget, bestAction);
        }

        /// <summary>
        /// Simple legacy AI behavior (old behavior preserved).
        /// </summary>
        private bool ProcessSimpleTurn(Unit enemy, List<Unit> playerUnits)
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

        #region Tactical AI Methods

        /// <summary>
        /// Determines the AI archetype for a unit based on its weapon type.
        /// </summary>
        private AIArchetype GetUnitArchetype(Unit unit)
        {
            if (unit.EquippedWeapon == null)
                return AIArchetype.Balanced;

            switch (unit.EquippedWeapon.Type)
            {
                case WeaponType.Melee:
                    return AIArchetype.Aggressive;
                case WeaponType.Ranged:
                    return unit.EquippedWeapon.Range > 5 ? AIArchetype.Defensive : AIArchetype.Balanced;
                case WeaponType.AOE:
                    return AIArchetype.Support;
                default:
                    return AIArchetype.Balanced;
            }
        }

        /// <summary>
        /// Find best target using multi-criteria scoring (XCOM-style).
        /// </summary>
        private Unit FindBestTarget(Unit enemy, List<Unit> potentialTargets, AIArchetype archetype)
        {
            Unit bestTarget = null;
            float bestScore = float.MinValue;

            foreach (Unit target in potentialTargets)
            {
                if (target == null || !target.IsAlive)
                    continue;

                float score = CalculateTargetPriority(enemy, target, archetype);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }

            if (enableDebugLogs && bestTarget != null)
            {
                Debug.Log($"[AI] {enemy.UnitName} selected target {bestTarget.UnitName} (score: {bestScore:F1})");
            }

            return bestTarget;
        }

        /// <summary>
        /// Calculate target priority score based on multiple factors.
        /// </summary>
        private float CalculateTargetPriority(Unit enemy, Unit target, AIArchetype archetype)
        {
            float score = 0f;

            // 1. Health-based scoring (prefer wounded targets - secure kills)
            float healthPercent = (float)target.CurrentHealth / target.MaxHealth;
            if (healthPercent < 0.3f)
                score += 25f; // Critical health - prioritize kill
            else if (healthPercent < 0.6f)
                score += 15f; // Wounded
            else
                score += 5f;  // Healthy

            // 2. Cover and exposure (prefer exposed targets)
            GridTile targetTile = target.GetCurrentTile();
            CoverType cover = targetTile != null ? targetTile.Cover : CoverType.None;

            switch (cover)
            {
                case CoverType.None:
                    score += 20f; // Exposed target
                    break;
                case CoverType.Half:
                    score += 10f; // Partial cover
                    break;
                case CoverType.Full:
                    score += 0f;  // In cover
                    break;
            }

            // 3. Flanking bonus
            if (IsFlankedFrom(enemy.GridPosition, target))
            {
                score += 15f;
            }

            // 4. Threat assessment (recent damage dealt)
            if (recentDamageByUnit.ContainsKey(target))
            {
                score += recentDamageByUnit[target] * 0.2f; // Prioritize dangerous targets
            }

            // 5. Distance consideration
            int distance = gridManager.GetDistance(enemy.GridPosition, target.GridPosition);
            int optimalRange = enemy.EquippedWeapon != null ? enemy.EquippedWeapon.Range / 2 : 3;

            // Archetype-based distance preference
            switch (archetype)
            {
                case AIArchetype.Aggressive:
                    score -= distance * 3f; // Strongly prefer close targets
                    break;
                case AIArchetype.Defensive:
                    score -= Mathf.Abs(distance - optimalRange) * 1.5f; // Prefer optimal range
                    break;
                case AIArchetype.Balanced:
                    score -= distance * 2f; // Moderate distance penalty
                    break;
                case AIArchetype.Support:
                    score -= Mathf.Abs(distance - optimalRange) * 1f; // Flexible range
                    break;
            }

            // 6. Line of sight validation
            if (enemy.EquippedWeapon != null && enemy.EquippedWeapon.Type != WeaponType.AOE)
            {
                if (!gridManager.HasLineOfSight(enemy.GridPosition, target.GridPosition))
                {
                    score -= 50f; // Heavy penalty for no LOS
                }
            }

            return score;
        }

        /// <summary>
        /// Check if target would be flanked from the given attacker position.
        /// </summary>
        private bool IsFlankedFrom(Vector2Int attackerPos, Unit target)
        {
            GridTile targetTile = target.GetCurrentTile();

            if (targetTile == null || targetTile.Cover == CoverType.None)
                return false; // No cover means no flanking bonus needed

            // Simplified flanking: check if attacker is on opposite side from cover
            // In a full implementation, you'd track cover direction
            // For now, we'll consider targets in half cover as flankable
            return targetTile.Cover == CoverType.Half;
        }

        /// <summary>
        /// Evaluate all possible actions for this unit.
        /// </summary>
        private List<AIAction> EvaluateAllActions(Unit enemy, Unit target, List<Unit> allTargets, AIArchetype archetype)
        {
            List<AIAction> actions = new List<AIAction>();

            // Get reachable tiles for movement
            List<GridTile> reachableTiles = gridManager.GetReachableTiles(enemy.GridPosition, enemy.MovementRange);

            // 1. Evaluate Attack Then Move (if can attack from current position)
            if (enemy.CurrentActionPoints >= 2 && enemy.EquippedWeapon != null)
            {
                AIAction attackThenMove = EvaluateAttackThenMove(enemy, target, reachableTiles, archetype);
                if (attackThenMove != null)
                    actions.Add(attackThenMove);
            }

            // 2. Evaluate Move Then Attack
            if (enemy.CurrentActionPoints >= 2)
            {
                AIAction moveThenAttack = EvaluateMoveThenAttack(enemy, target, reachableTiles, archetype);
                if (moveThenAttack != null)
                    actions.Add(moveThenAttack);
            }

            // 3. Evaluate Defensive Retreat (self-preservation)
            float healthPercent = (float)enemy.CurrentHealth / enemy.MaxHealth;
            if (healthPercent < 0.4f && enemy.CurrentActionPoints > 0)
            {
                AIAction retreat = EvaluateDefensiveRetreat(enemy, allTargets, reachableTiles, archetype);
                if (retreat != null)
                    actions.Add(retreat);
            }

            // 4. Evaluate Move to Flank
            if (enemy.CurrentActionPoints >= 1)
            {
                AIAction flankMove = EvaluateMoveToFlank(enemy, target, reachableTiles, archetype);
                if (flankMove != null)
                    actions.Add(flankMove);
            }

            // 5. Evaluate Direct Attack (if already in position)
            if (enemy.CurrentActionPoints >= 1 && enemy.EquippedWeapon != null &&
                enemy.IsInAttackRange(target.GridPosition))
            {
                if (combatSystem.CanAttack(enemy, target, out _))
                {
                    AIAction directAttack = new AIAction
                    {
                        Type = AIActionType.Attack,
                        TargetPosition = enemy.GridPosition,
                        Score = 30f // Base score for direct attack
                    };
                    actions.Add(directAttack);
                }
            }

            return actions;
        }

        /// <summary>
        /// Evaluate attacking then moving to cover.
        /// </summary>
        private AIAction EvaluateAttackThenMove(Unit enemy, Unit target, List<GridTile> reachableTiles, AIArchetype archetype)
        {
            // Can we attack from current position?
            if (!enemy.IsInAttackRange(target.GridPosition) ||
                !combatSystem.CanAttack(enemy, target, out _))
                return null;

            // Find best cover position after attacking
            GridTile bestCover = FindBestCoverPosition(enemy, target, reachableTiles);

            if (bestCover == null)
                return null;

            float score = 40f; // Good option - attack then get to safety

            // Bonus if moving to better cover
            GridTile currentTile = enemy.GetCurrentTile();
            if (bestCover.Cover > (currentTile?.Cover ?? CoverType.None))
                score += 15f;

            // Aggressive units prefer this less
            if (archetype == AIArchetype.Aggressive)
                score -= 10f;

            return new AIAction
            {
                Type = AIActionType.AttackThenMove,
                TargetPosition = bestCover.GridPosition,
                Score = score
            };
        }

        /// <summary>
        /// Evaluate moving to a better position then attacking.
        /// </summary>
        private AIAction EvaluateMoveThenAttack(Unit enemy, Unit target, List<GridTile> reachableTiles, AIArchetype archetype)
        {
            if (enemy.EquippedWeapon == null)
                return null;

            GridTile bestTile = null;
            float bestTileScore = float.MinValue;

            // Find best tile that allows attack
            foreach (GridTile tile in reachableTiles)
            {
                // Can we attack target from this tile?
                int distance = gridManager.GetDistance(tile.GridPosition, target.GridPosition);
                if (distance > enemy.EquippedWeapon.Range)
                    continue;

                // Check line of sight
                if (enemy.EquippedWeapon.Type != WeaponType.AOE &&
                    !gridManager.HasLineOfSight(tile.GridPosition, target.GridPosition))
                    continue;

                float tileScore = 0f;

                // Cover value
                switch (tile.Cover)
                {
                    case CoverType.Full:
                        tileScore += 20f;
                        break;
                    case CoverType.Half:
                        tileScore += 10f;
                        break;
                }

                // Distance to optimal range
                int optimalRange = enemy.EquippedWeapon.Range / 2;
                tileScore -= Mathf.Abs(distance - optimalRange) * 2f;

                // Flanking bonus
                if (IsFlankedFrom(tile.GridPosition, target))
                    tileScore += 15f;

                if (tileScore > bestTileScore)
                {
                    bestTileScore = tileScore;
                    bestTile = tile;
                }
            }

            if (bestTile == null)
                return null;

            float actionScore = 50f + bestTileScore; // Strong option

            // Defensive units love this
            if (archetype == AIArchetype.Defensive)
                actionScore += 15f;

            return new AIAction
            {
                Type = AIActionType.MoveThenAttack,
                TargetPosition = bestTile.GridPosition,
                Score = actionScore
            };
        }

        /// <summary>
        /// Evaluate retreating to cover (self-preservation).
        /// </summary>
        private AIAction EvaluateDefensiveRetreat(Unit enemy, List<Unit> threats, List<GridTile> reachableTiles, AIArchetype archetype)
        {
            GridTile bestRetreat = null;
            float bestScore = float.MinValue;

            foreach (GridTile tile in reachableTiles)
            {
                float score = 0f;

                // Prioritize best cover
                switch (tile.Cover)
                {
                    case CoverType.Full:
                        score += 30f;
                        break;
                    case CoverType.Half:
                        score += 15f;
                        break;
                }

                // Prefer distance from enemies
                foreach (Unit threat in threats)
                {
                    if (threat == null || !threat.IsAlive)
                        continue;

                    int distance = gridManager.GetDistance(tile.GridPosition, threat.GridPosition);
                    score += distance * 2f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestRetreat = tile;
                }
            }

            if (bestRetreat == null)
                return null;

            float actionScore = 60f; // High priority for wounded units

            // Aggressive units dislike retreating
            if (archetype == AIArchetype.Aggressive)
                actionScore -= 30f;

            return new AIAction
            {
                Type = AIActionType.Retreat,
                TargetPosition = bestRetreat.GridPosition,
                Score = actionScore
            };
        }

        /// <summary>
        /// Evaluate moving to flank a target.
        /// </summary>
        private AIAction EvaluateMoveToFlank(Unit enemy, Unit target, List<GridTile> reachableTiles, AIArchetype archetype)
        {
            GridTile targetTile = target.GetCurrentTile();

            // No point flanking if target has no cover
            if (targetTile == null || targetTile.Cover == CoverType.None)
                return null;

            GridTile bestFlank = null;
            float bestScore = float.MinValue;

            foreach (GridTile tile in reachableTiles)
            {
                // Check if this position would flank
                if (!IsFlankedFrom(tile.GridPosition, target))
                    continue;

                float score = 25f; // Base flanking bonus

                // Add cover value
                switch (tile.Cover)
                {
                    case CoverType.Full:
                        score += 15f;
                        break;
                    case CoverType.Half:
                        score += 8f;
                        break;
                }

                // Distance consideration
                int distance = gridManager.GetDistance(tile.GridPosition, target.GridPosition);
                if (enemy.EquippedWeapon != null)
                {
                    if (distance <= enemy.EquippedWeapon.Range)
                        score += 10f; // In attack range after move
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestFlank = tile;
                }
            }

            if (bestFlank == null)
                return null;

            float actionScore = 35f + bestScore;

            // Balanced and aggressive units like flanking
            if (archetype == AIArchetype.Aggressive || archetype == AIArchetype.Balanced)
                actionScore += 10f;

            return new AIAction
            {
                Type = AIActionType.MoveToFlank,
                TargetPosition = bestFlank.GridPosition,
                Score = actionScore
            };
        }

        /// <summary>
        /// Execute the chosen AI action.
        /// </summary>
        private bool ExecuteAction(Unit enemy, Unit target, AIAction action)
        {
            switch (action.Type)
            {
                case AIActionType.Attack:
                    return combatSystem.CanAttack(enemy, target, out _) &&
                           combatSystem.ExecuteAttack(enemy, target).Success;

                case AIActionType.AttackThenMove:
                    if (combatSystem.CanAttack(enemy, target, out _))
                    {
                        combatSystem.ExecuteAttack(enemy, target);
                        if (enemy.CurrentActionPoints > 0 && action.TargetPosition != enemy.GridPosition)
                        {
                            enemy.MoveTo(action.TargetPosition, 1);
                        }
                        return true;
                    }
                    return false;

                case AIActionType.MoveThenAttack:
                    if (action.TargetPosition != enemy.GridPosition)
                    {
                        enemy.MoveTo(action.TargetPosition, 1);
                    }
                    if (enemy.CurrentActionPoints > 0 && combatSystem.CanAttack(enemy, target, out _))
                    {
                        combatSystem.ExecuteAttack(enemy, target);
                    }
                    return true;

                case AIActionType.Retreat:
                case AIActionType.MoveToFlank:
                    if (action.TargetPosition != enemy.GridPosition)
                    {
                        return enemy.MoveTo(action.TargetPosition, 1);
                    }
                    return false;

                default:
                    return false;
            }
        }

        #endregion

        #region Legacy AI Methods

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

        #endregion
    }

    #region AI Data Structures

    /// <summary>
    /// AI behavior archetypes that determine tactical preferences.
    /// </summary>
    public enum AIArchetype
    {
        Aggressive,  // Melee units - prioritize damage over safety, rush forward
        Balanced,    // Standard tactical behavior - balance offense and defense
        Defensive,   // Ranged/Snipers - prioritize cover and optimal range
        Support      // AOE/Support units - flexible positioning, area control
    }

    /// <summary>
    /// Types of actions the AI can take.
    /// </summary>
    public enum AIActionType
    {
        Attack,          // Direct attack from current position
        AttackThenMove,  // Attack then move to cover
        MoveThenAttack,  // Move to better position then attack
        Retreat,         // Defensive retreat to cover (self-preservation)
        MoveToFlank      // Move to flanking position
    }

    /// <summary>
    /// Represents a possible AI action with its evaluated score.
    /// </summary>
    public class AIAction
    {
        public AIActionType Type;
        public Vector2Int TargetPosition;
        public float Score;
    }

    #endregion
}
