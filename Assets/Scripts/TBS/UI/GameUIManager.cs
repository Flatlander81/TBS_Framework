using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TBS.Events;
using TBS.Core;
using TBS.Units;

namespace TBS.UI
{
    /// <summary>
    /// Manages the game's user interface.
    /// Displays turn information, unit stats, and game messages.
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        [Header("Turn Display")]
        [SerializeField] private TextMeshProUGUI turnPhaseText;
        [SerializeField] private TextMeshProUGUI turnNumberText;
        [SerializeField] private Button endTurnButton;

        [Header("Unit Info Panel")]
        [SerializeField] private GameObject unitInfoPanel;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitHealthText;
        [SerializeField] private TextMeshProUGUI unitActionPointsText;
        [SerializeField] private TextMeshProUGUI unitWeaponText;
        [SerializeField] private Slider healthBar;

        [Header("Combat Log")]
        [SerializeField] private TextMeshProUGUI combatLogText;
        [SerializeField] private int maxLogLines = 5;

        private TurnManager turnManager;
        private System.Collections.Generic.Queue<string> logMessages = new System.Collections.Generic.Queue<string>();

        private void Start()
        {
            turnManager = FindFirstObjectByType<TurnManager>();

            // Subscribe to events
            GameEvents.OnTurnPhaseChanged += UpdateTurnPhase;
            GameEvents.OnTurnNumberChanged += UpdateTurnNumber;
            GameEvents.OnUnitSelected += DisplayUnitInfo;
            GameEvents.OnUnitDeselected += HideUnitInfo;
            GameEvents.OnUnitActionPointsChanged += UpdateUnitActionPoints;
            GameEvents.OnGameMessage += AddLogMessage;
            GameEvents.OnAttackExecuted += OnAttackExecuted;

            // Setup end turn button
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            // Hide unit info initially
            if (unitInfoPanel != null)
            {
                unitInfoPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnTurnPhaseChanged -= UpdateTurnPhase;
            GameEvents.OnTurnNumberChanged -= UpdateTurnNumber;
            GameEvents.OnUnitSelected -= DisplayUnitInfo;
            GameEvents.OnUnitDeselected -= HideUnitInfo;
            GameEvents.OnUnitActionPointsChanged -= UpdateUnitActionPoints;
            GameEvents.OnGameMessage -= AddLogMessage;
            GameEvents.OnAttackExecuted -= OnAttackExecuted;

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            }
        }

        private void UpdateTurnPhase(TurnPhase phase)
        {
            if (turnPhaseText != null)
            {
                string phaseText = phase == TurnPhase.PlayerTurn ? "PLAYER TURN" : "ENEMY TURN";
                Color phaseColor = phase == TurnPhase.PlayerTurn ? Color.green : Color.red;
                turnPhaseText.text = phaseText;
                turnPhaseText.color = phaseColor;
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable = phase == TurnPhase.PlayerTurn;
            }
        }

        private void UpdateTurnNumber(int turnNumber)
        {
            if (turnNumberText != null)
            {
                turnNumberText.text = $"Turn {turnNumber}";
            }
        }

        private void DisplayUnitInfo(Unit unit)
        {
            if (unitInfoPanel != null)
            {
                unitInfoPanel.SetActive(true);
            }

            UpdateUnitDisplay(unit);
        }

        private void HideUnitInfo(Unit unit)
        {
            if (unitInfoPanel != null)
            {
                unitInfoPanel.SetActive(false);
            }
        }

        private void UpdateUnitDisplay(Unit unit)
        {
            if (unit == null) return;

            if (unitNameText != null)
            {
                unitNameText.text = unit.UnitName;
            }

            if (unitHealthText != null)
            {
                unitHealthText.text = $"HP: {unit.CurrentHealth}/{unit.MaxHealth}";
            }

            if (healthBar != null)
            {
                healthBar.maxValue = unit.MaxHealth;
                healthBar.value = unit.CurrentHealth;
            }

            if (unitActionPointsText != null)
            {
                unitActionPointsText.text = $"AP: {unit.CurrentActionPoints}";
            }

            if (unitWeaponText != null)
            {
                if (unit.EquippedWeapon != null)
                {
                    unitWeaponText.text = $"Weapon: {unit.EquippedWeapon.WeaponName}";
                }
                else
                {
                    unitWeaponText.text = "Weapon: None";
                }
            }
        }

        private void UpdateUnitActionPoints(Unit unit, int actionPoints)
        {
            if (unit.IsSelected)
            {
                UpdateUnitDisplay(unit);
            }
        }

        private void OnEndTurnClicked()
        {
            if (turnManager != null)
            {
                turnManager.EndCurrentPhase();
            }
        }

        private void OnAttackExecuted(Unit attacker, Unit target, Combat.AttackResult result)
        {
            // Update displays if these units are selected
            if (attacker.IsSelected)
            {
                UpdateUnitDisplay(attacker);
            }
            if (target.IsSelected)
            {
                UpdateUnitDisplay(target);
            }
        }

        private void AddLogMessage(string message)
        {
            logMessages.Enqueue(message);

            while (logMessages.Count > maxLogLines)
            {
                logMessages.Dequeue();
            }

            UpdateCombatLog();
        }

        private void UpdateCombatLog()
        {
            if (combatLogText != null)
            {
                combatLogText.text = string.Join("\n", logMessages);
            }
        }
    }
}
