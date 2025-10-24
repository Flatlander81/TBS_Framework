using System;
using System.Collections.Generic;
using UnityEngine;
using TBS.Units;

namespace TBS.Core
{
    /// <summary>
    /// Manages game state serialization and deserialization for save/load functionality.
    /// Provides a foundation for implementing save/load features.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private TurnManager turnManager;
        private Grid.GridManager gridManager;

        private void Start()
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            gridManager = FindFirstObjectByType<Grid.GridManager>();
        }

        /// <summary>
        /// Captures the current game state.
        /// </summary>
        public GameState CaptureGameState()
        {
            GameState state = new GameState();

            // Capture turn info
            if (turnManager != null)
            {
                state.currentTurn = turnManager.CurrentTurn;
                state.currentPhase = turnManager.CurrentPhase;
            }

            // Capture unit states
            Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            state.unitStates = new List<UnitState>();

            foreach (Unit unit in allUnits)
            {
                if (unit.IsAlive)
                {
                    UnitState unitState = new UnitState
                    {
                        unitName = unit.UnitName,
                        faction = unit.Faction,
                        gridPosition = unit.GridPosition,
                        currentHealth = unit.CurrentHealth,
                        maxHealth = unit.MaxHealth,
                        currentActionPoints = unit.CurrentActionPoints,
                        equippedWeaponName = unit.EquippedWeapon != null ? unit.EquippedWeapon.WeaponName : ""
                    };
                    state.unitStates.Add(unitState);
                }
            }

            state.timestamp = DateTime.Now.ToString();

            return state;
        }

        /// <summary>
        /// Saves the game state to JSON file.
        /// </summary>
        public void SaveGame(string fileName = "savegame.json")
        {
            GameState state = CaptureGameState();
            string json = JsonUtility.ToJson(state, true);

            string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            System.IO.File.WriteAllText(path, json);

            Debug.Log($"Game saved to: {path}");
        }

        /// <summary>
        /// Loads game state from JSON file.
        /// Note: This is a foundation - full implementation would require
        /// reconstructing the entire game state from this data.
        /// </summary>
        public GameState LoadGame(string fileName = "savegame.json")
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);

            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"Save file not found: {path}");
                return null;
            }

            string json = System.IO.File.ReadAllText(path);
            GameState state = JsonUtility.FromJson<GameState>(json);

            Debug.Log($"Game loaded from: {path}");
            return state;
        }

        /// <summary>
        /// Restores the game state.
        /// This is a simplified example - full implementation would require
        /// scene management and proper object instantiation.
        /// </summary>
        public void RestoreGameState(GameState state)
        {
            // This is a foundation for implementation
            // You would need to:
            // 1. Clear current game state
            // 2. Restore turn manager state
            // 3. Instantiate units at correct positions
            // 4. Restore unit stats and equipment
            // 5. Restore grid state

            Debug.Log($"Restoring game state from {state.timestamp}");
            Debug.Log($"Turn: {state.currentTurn}, Phase: {state.currentPhase}");
            Debug.Log($"Units to restore: {state.unitStates.Count}");

            // Example: You would implement full restoration logic here
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public bool SaveFileExists(string fileName = "savegame.json")
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            return System.IO.File.Exists(path);
        }
    }

    /// <summary>
    /// Serializable game state container.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public int currentTurn;
        public TurnPhase currentPhase;
        public List<UnitState> unitStates;
        public string timestamp;
    }

    /// <summary>
    /// Serializable unit state.
    /// </summary>
    [Serializable]
    public class UnitState
    {
        public string unitName;
        public UnitFaction faction;
        public Vector2Int gridPosition;
        public int currentHealth;
        public int maxHealth;
        public int currentActionPoints;
        public string equippedWeaponName;
    }
}
