using System;
using UnityEngine;

namespace TBS.Events
{
    /// <summary>
    /// Central event system for game state changes.
    /// Allows decoupled communication between systems.
    /// </summary>
    public static class GameEvents
    {
        // Turn Events
        public static event Action<Core.TurnPhase> OnTurnPhaseChanged;
        public static event Action<int> OnTurnNumberChanged;

        // Unit Events
        public static event Action<Units.Unit> OnUnitSelected;
        public static event Action<Units.Unit> OnUnitDeselected;
        public static event Action<Units.Unit, Vector2Int, Vector2Int> OnUnitMoved;
        public static event Action<Units.Unit> OnUnitDied;
        public static event Action<Units.Unit, int> OnUnitActionPointsChanged;

        // Combat Events
        public static event Action<Units.Unit, Units.Unit, Combat.AttackResult> OnAttackExecuted;
        public static event Action<Units.Unit, int> OnUnitDamaged;
        public static event Action<Units.Unit, int> OnUnitHealed;

        // UI Events
        public static event Action<string> OnGameMessage;

        // Turn Event Triggers
        public static void TriggerTurnPhaseChanged(Core.TurnPhase newPhase)
        {
            OnTurnPhaseChanged?.Invoke(newPhase);
            Debug.Log($"Turn phase changed to: {newPhase}");
        }

        public static void TriggerTurnNumberChanged(int turnNumber)
        {
            OnTurnNumberChanged?.Invoke(turnNumber);
            Debug.Log($"Turn number: {turnNumber}");
        }

        // Unit Event Triggers
        public static void TriggerUnitSelected(Units.Unit unit)
        {
            OnUnitSelected?.Invoke(unit);
        }

        public static void TriggerUnitDeselected(Units.Unit unit)
        {
            OnUnitDeselected?.Invoke(unit);
        }

        public static void TriggerUnitMoved(Units.Unit unit, Vector2Int from, Vector2Int to)
        {
            OnUnitMoved?.Invoke(unit, from, to);
            Debug.Log($"{unit.UnitName} moved from {from} to {to}");
        }

        public static void TriggerUnitDied(Units.Unit unit)
        {
            OnUnitDied?.Invoke(unit);
            Debug.Log($"{unit.UnitName} has been eliminated!");
        }

        public static void TriggerUnitActionPointsChanged(Units.Unit unit, int actionPoints)
        {
            OnUnitActionPointsChanged?.Invoke(unit, actionPoints);
        }

        // Combat Event Triggers
        public static void TriggerAttackExecuted(Units.Unit attacker, Units.Unit target, Combat.AttackResult result)
        {
            OnAttackExecuted?.Invoke(attacker, target, result);

            if (result.Hit)
            {
                Debug.Log($"{attacker.UnitName} hit {target.UnitName} for {result.Damage} damage! ({result.HitChance:F0}% chance)");
            }
            else
            {
                Debug.Log($"{attacker.UnitName} missed {target.UnitName}! ({result.HitChance:F0}% chance)");
            }
        }

        public static void TriggerUnitDamaged(Units.Unit unit, int damage)
        {
            OnUnitDamaged?.Invoke(unit, damage);
        }

        public static void TriggerUnitHealed(Units.Unit unit, int healing)
        {
            OnUnitHealed?.Invoke(unit, healing);
        }

        // UI Event Triggers
        public static void TriggerGameMessage(string message)
        {
            OnGameMessage?.Invoke(message);
            Debug.Log($"[GAME] {message}");
        }

        /// <summary>
        /// Clears all event subscriptions. Call this when changing scenes.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnTurnPhaseChanged = null;
            OnTurnNumberChanged = null;
            OnUnitSelected = null;
            OnUnitDeselected = null;
            OnUnitMoved = null;
            OnUnitDied = null;
            OnUnitActionPointsChanged = null;
            OnAttackExecuted = null;
            OnUnitDamaged = null;
            OnUnitHealed = null;
            OnGameMessage = null;
        }
    }
}
