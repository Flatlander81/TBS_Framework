using UnityEngine;

namespace TBS.Core
{
    /// <summary>
    /// Main game manager that coordinates all game systems.
    /// Place this on a GameObject in your scene to bootstrap the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private Grid.GridManager gridManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Combat.CombatSystem combatSystem;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private AI.EnemyAI enemyAI;
        [SerializeField] private GameStateManager gameStateManager;

        [Header("UI")]
        [SerializeField] private UI.GameUIManager uiManager;

        [Header("Camera")]
        [SerializeField] private TBS.Camera.TacticalCamera tacticalCamera;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Auto-find components if not assigned
            if (gridManager == null) gridManager = FindObjectOfType<Grid.GridManager>();
            if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
            if (combatSystem == null) combatSystem = FindObjectOfType<Combat.CombatSystem>();
            if (playerController == null) playerController = FindObjectOfType<PlayerController>();
            if (enemyAI == null) enemyAI = FindObjectOfType<AI.EnemyAI>();
            if (gameStateManager == null) gameStateManager = FindObjectOfType<GameStateManager>();
            if (uiManager == null) uiManager = FindObjectOfType<UI.GameUIManager>();
            if (tacticalCamera == null) tacticalCamera = FindObjectOfType<TBS.Camera.TacticalCamera>();

            ValidateSystems();
        }

        private void ValidateSystems()
        {
            if (gridManager == null) Debug.LogError("GridManager not found! Please add GridManager to scene.");
            if (turnManager == null) Debug.LogError("TurnManager not found! Please add TurnManager to scene.");
            if (combatSystem == null) Debug.LogError("CombatSystem not found! Please add CombatSystem to scene.");
        }

        private void Update()
        {
            // Quick save/load shortcuts
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadGame();
            }
        }

        public void SaveGame()
        {
            if (gameStateManager != null)
            {
                gameStateManager.SaveGame();
                Events.GameEvents.TriggerGameMessage("Game Saved!");
            }
        }

        public void LoadGame()
        {
            if (gameStateManager != null)
            {
                GameState state = gameStateManager.LoadGame();
                if (state != null)
                {
                    gameStateManager.RestoreGameState(state);
                    Events.GameEvents.TriggerGameMessage("Game Loaded!");
                }
            }
        }

        public Grid.GridManager GetGridManager() => gridManager;
        public TurnManager GetTurnManager() => turnManager;
        public Combat.CombatSystem GetCombatSystem() => combatSystem;
    }
}
