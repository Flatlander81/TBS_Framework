using UnityEngine;
using TBS.Grid;

namespace TBS.Combat
{
    /// <summary>
    /// ScriptableObject that defines weapon properties.
    /// Create instances via Assets/Create/TBS/Weapon Data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "TBS/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Weapon Identity")]
        [SerializeField] private string weaponName = "Rifle";
        [SerializeField] private WeaponType weaponType = WeaponType.Ranged;
        [SerializeField] private string description = "A standard ranged weapon.";

        [Header("Weapon Stats")]
        [SerializeField] private int baseDamage = 30;
        [SerializeField] private int range = 6;
        [SerializeField] private int baseAccuracy = 85;
        [SerializeField] private int actionPointCost = 1;

        [Header("Modifiers")]
        [SerializeField] private int accuracyModifier = 0;
        [SerializeField] private int defenseModifier = 0;

        [Header("Special Properties")]
        [SerializeField] private int accuracyFalloffPerTile = 5; // For ranged weapons
        [SerializeField] private int spreadAngle = 30; // For spread weapons (degrees)
        [SerializeField] private int aoeRadius = 2; // For AOE weapons

        // Properties
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public string Description => description;
        public int BaseDamage => baseDamage;
        public int Range => range;
        public int BaseAccuracy => baseAccuracy;
        public int ActionPointCost => actionPointCost;
        public int AccuracyModifier => accuracyModifier;
        public int DefenseModifier => defenseModifier;
        public int AccuracyFalloffPerTile => accuracyFalloffPerTile;
        public int SpreadAngle => spreadAngle;
        public int AOERadius => aoeRadius;

        /// <summary>
        /// Calculates damage with random variance.
        /// </summary>
        public int CalculateDamage()
        {
            // Add 10% variance to damage
            int variance = Mathf.RoundToInt(baseDamage * 0.1f);
            return baseDamage + Random.Range(-variance, variance + 1);
        }

        /// <summary>
        /// Calculates hit chance based on distance and modifiers.
        /// </summary>
        public float CalculateHitChance(int shooterAccuracy, int distance, CoverType targetCover)
        {
            float hitChance = baseAccuracy + shooterAccuracy;

            // Apply distance penalty for ranged weapons
            if (weaponType == WeaponType.Ranged)
            {
                hitChance -= distance * accuracyFalloffPerTile;
            }

            // Apply cover bonus to target
            switch (targetCover)
            {
                case CoverType.Half:
                    hitChance -= 20;
                    break;
                case CoverType.Full:
                    hitChance -= 40;
                    break;
            }

            // Clamp between 5% and 95%
            return Mathf.Clamp(hitChance, 5f, 95f);
        }
    }

    /// <summary>
    /// Types of weapons available in the game.
    /// </summary>
    public enum WeaponType
    {
        Ranged,  // Single-target, accuracy falls off with distance
        Melee,   // Adjacent tiles only, high accuracy
        Spread,  // Cone/spread pattern, multiple targets
        AOE      // Area of effect, affects multiple tiles
    }
}
