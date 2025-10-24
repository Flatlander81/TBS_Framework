using UnityEngine;
using TBS.Combat;
using TBS.Units;

namespace TBS.Data
{
    /// <summary>
    /// ScriptableObject that defines unit templates.
    /// Create instances via Assets/Create/TBS/Unit Data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Unit", menuName = "TBS/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("Unit Identity")]
        [SerializeField] private string unitName = "Soldier";
        [SerializeField] private UnitFaction faction = UnitFaction.Player;

        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int movementRange = 4;
        [SerializeField] private int baseAccuracy = 75;
        [SerializeField] private int baseDefense = 0;

        [Header("Starting Equipment")]
        [SerializeField] private WeaponData startingWeapon;

        [Header("Visual")]
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Color unitColor = Color.blue;

        // Properties
        public string UnitName => unitName;
        public UnitFaction Faction => faction;
        public int MaxHealth => maxHealth;
        public int MovementRange => movementRange;
        public int BaseAccuracy => baseAccuracy;
        public int BaseDefense => baseDefense;
        public WeaponData StartingWeapon => startingWeapon;
        public GameObject UnitPrefab => unitPrefab;
        public Color UnitColor => unitColor;

        /// <summary>
        /// Applies this unit data to a unit instance.
        /// </summary>
        public void ApplyToUnit(Unit unit)
        {
            // Use reflection or a setup method to apply stats
            // This is a simple example - you'd need to expose setters in Unit class
            if (startingWeapon != null)
            {
                unit.EquipWeapon(startingWeapon);
            }
        }
    }
}
