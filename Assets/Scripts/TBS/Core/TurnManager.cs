using System.Collections.Generic;
using UnityEngine;
using TBS.Events;
using TBS.Units;

namespace TBS.Core
{
    /// <summary>
    /// Manages the turn-based gameplay flow.
    /// Handles turn phases, action point distribution, and turn progression.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Turn Settings")]
        [SerializeField] private int actionPointsPerTurn = 2;

        private TurnPhase currentPhase;
        private int currentTurn = 1;
        private List<Unit> playerUnits = new List<Unit>();
        private List<Unit> enemyUnits = new List<Unit>();

        public TurnPhase CurrentPhase => currentPhase;
        public int CurrentTurn => currentTurn;
        public int ActionPointsPerTurn => actionPointsPerTurn;

        private void Start()
        {
            // Subscribe to unit death events
            GameEvents.OnUnitDied += HandleUnitDeath;

            // Start with player turn
            StartPlayerTurn();
        }

        private void OnDestroy()
        {
            GameEvents.OnUnitDied -= HandleUnitDeath;
        }

        /// <summary>
        /// Registers a unit as a player unit.
        /// </summary>
        public void RegisterPlayerUnit(Unit unit)
        {
            if (!playerUnits.Contains(unit))
            {
                playerUnits.Add(unit);
            }
        }

        /// <summary>
        /// Registers a unit as an enemy unit.
        /// </summary>
        public void RegisterEnemyUnit(Unit unit)
        {
            if (!enemyUnits.Contains(unit))
            {
                enemyUnits.Add(unit);
            }
        }

        /// <summary>
        /// Handles unit death by removing from active lists.
        /// </summary>
        private void HandleUnitDeath(Unit unit)
        {
            playerUnits.Remove(unit);
            enemyUnits.Remove(unit);

            CheckWinCondition();
        }

        /// <summary>
        /// Checks if either side has won.
        /// </summary>
        private void CheckWinCondition()
        {
            if (playerUnits.Count == 0)
            {
                GameEvents.TriggerGameMessage("DEFEAT! All player units eliminated.");
            }
            else if (enemyUnits.Count == 0)
            {
                GameEvents.TriggerGameMessage("VICTORY! All enemy units eliminated.");
            }
        }

        /// <summary>
        /// Starts the player turn phase.
        /// </summary>
        public void StartPlayerTurn()
        {
            currentPhase = TurnPhase.PlayerTurn;
            GameEvents.TriggerTurnPhaseChanged(currentPhase);
            GameEvents.TriggerTurnNumberChanged(currentTurn);

            // Reset action points for all player units
            foreach (Unit unit in playerUnits)
            {
                unit.ResetActionPoints(actionPointsPerTurn);
            }

            GameEvents.TriggerGameMessage($"Turn {currentTurn} - Player Phase");
        }

        /// <summary>
        /// Starts the enemy turn phase.
        /// </summary>
        public void StartEnemyTurn()
        {
            currentPhase = TurnPhase.EnemyTurn;
            GameEvents.TriggerTurnPhaseChanged(currentPhase);

            // Reset action points for all enemy units
            foreach (Unit unit in enemyUnits)
            {
                unit.ResetActionPoints(actionPointsPerTurn);
            }

            GameEvents.TriggerGameMessage("Enemy Phase");

            // Trigger AI to take actions
            // This will be handled by the AI controller
        }

        /// <summary>
        /// Ends the current turn and progresses to the next phase.
        /// </summary>
        public void EndCurrentPhase()
        {
            if (currentPhase == TurnPhase.PlayerTurn)
            {
                StartEnemyTurn();
            }
            else if (currentPhase == TurnPhase.EnemyTurn)
            {
                currentTurn++;
                StartPlayerTurn();
            }
        }

        /// <summary>
        /// Checks if all units of the current faction have used their actions.
        /// </summary>
        public bool AreAllUnitsExhausted()
        {
            List<Unit> activeUnits = currentPhase == TurnPhase.PlayerTurn ? playerUnits : enemyUnits;

            foreach (Unit unit in activeUnits)
            {
                if (unit.CurrentActionPoints > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all units for the current faction.
        /// </summary>
        public List<Unit> GetCurrentFactionUnits()
        {
            return currentPhase == TurnPhase.PlayerTurn ? playerUnits : enemyUnits;
        }

        /// <summary>
        /// Gets all units for the opposing faction.
        /// </summary>
        public List<Unit> GetOpposingFactionUnits()
        {
            return currentPhase == TurnPhase.PlayerTurn ? enemyUnits : playerUnits;
        }

        /// <summary>
        /// Gets all player units.
        /// </summary>
        public List<Unit> GetPlayerUnits()
        {
            return new List<Unit>(playerUnits);
        }

        /// <summary>
        /// Gets all enemy units.
        /// </summary>
        public List<Unit> GetEnemyUnits()
        {
            return new List<Unit>(enemyUnits);
        }

        /// <summary>
        /// Checks if it's currently the player's turn.
        /// </summary>
        public bool IsPlayerTurn()
        {
            return currentPhase == TurnPhase.PlayerTurn;
        }
    }

    /// <summary>
    /// Represents the different phases of a turn.
    /// </summary>
    public enum TurnPhase
    {
        PlayerTurn,
        EnemyTurn
    }
}
