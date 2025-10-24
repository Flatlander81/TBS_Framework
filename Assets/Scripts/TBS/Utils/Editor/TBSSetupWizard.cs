using UnityEngine;
using UnityEditor;
using TBS.Combat;

namespace TBS.Utils.Editor
{
    /// <summary>
    /// Editor wizard to help set up the TBS Framework quickly.
    /// Access via Tools > TBS Framework > Setup Wizard
    /// </summary>
    public class TBSSetupWizard : EditorWindow
    {
        [MenuItem("Tools/TBS Framework/Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<TBSSetupWizard>("TBS Setup Wizard");
        }

        private void OnGUI()
        {
            GUILayout.Label("TBS Framework Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This wizard helps you create example weapons and units for the TBS Framework.",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Example Weapons", GUILayout.Height(30)))
            {
                CreateExampleWeapons();
            }

            if (GUILayout.Button("Create Example Units", GUILayout.Height(30)))
            {
                CreateExampleUnits();
            }

            if (GUILayout.Button("Create All Example Content", GUILayout.Height(30)))
            {
                CreateExampleWeapons();
                CreateExampleUnits();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "After creating content, check the Assets/Resources/ folders.",
                MessageType.Info);
        }

        private void CreateExampleWeapons()
        {
            string weaponsPath = "Assets/Resources/Weapons";
            if (!AssetDatabase.IsValidFolder(weaponsPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Weapons");
            }

            // Rifle (Ranged)
            CreateWeapon(weaponsPath, "Rifle", WeaponType.Ranged,
                "Standard ranged weapon with accuracy falloff", 30, 6, 85, 1, 5);

            // Sword (Melee)
            CreateWeapon(weaponsPath, "Sword", WeaponType.Melee,
                "Close-range melee weapon", 40, 1, 90, 1, 0);

            // Shotgun (Spread)
            CreateWeapon(weaponsPath, "Shotgun", WeaponType.Spread,
                "Spread weapon affecting cone area", 25, 3, 75, 1, 2);

            // Grenade (AOE)
            CreateWeapon(weaponsPath, "Grenade", WeaponType.AOE,
                "Explosive with area of effect", 35, 5, 100, 1, 0);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Example weapons created in Assets/Resources/Weapons/");
            EditorUtility.DisplayDialog("Success", "Example weapons created successfully!", "OK");
        }

        private void CreateWeapon(string path, string name, WeaponType type, string description,
            int damage, int range, int accuracy, int apCost, int falloff)
        {
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();

            // Use reflection to set private fields
            var nameField = typeof(WeaponData).GetField("weaponName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var typeField = typeof(WeaponData).GetField("weaponType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = typeof(WeaponData).GetField("description",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var damageField = typeof(WeaponData).GetField("baseDamage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rangeField = typeof(WeaponData).GetField("range",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var accuracyField = typeof(WeaponData).GetField("baseAccuracy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var apField = typeof(WeaponData).GetField("actionPointCost",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var falloffField = typeof(WeaponData).GetField("accuracyFalloffPerTile",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            nameField?.SetValue(weapon, name);
            typeField?.SetValue(weapon, type);
            descField?.SetValue(weapon, description);
            damageField?.SetValue(weapon, damage);
            rangeField?.SetValue(weapon, range);
            accuracyField?.SetValue(weapon, accuracy);
            apField?.SetValue(weapon, apCost);
            falloffField?.SetValue(weapon, falloff);

            string assetPath = $"{path}/{name}.asset";
            AssetDatabase.CreateAsset(weapon, assetPath);
        }

        private void CreateExampleUnits()
        {
            string unitsPath = "Assets/Resources/Units";
            if (!AssetDatabase.IsValidFolder(unitsPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Units");
            }

            // Load example weapons
            WeaponData rifle = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Resources/Weapons/Rifle.asset");

            CreateUnit(unitsPath, "Soldier", Units.UnitFaction.Player, 100, 4, 75, 0, rifle);
            CreateUnit(unitsPath, "Enemy Grunt", Units.UnitFaction.Enemy, 80, 4, 70, 0, rifle);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Example units created in Assets/Resources/Units/");
            EditorUtility.DisplayDialog("Success", "Example units created successfully!", "OK");
        }

        private void CreateUnit(string path, string name, Units.UnitFaction faction,
            int health, int movement, int accuracy, int defense, WeaponData weapon)
        {
            Data.UnitData unit = ScriptableObject.CreateInstance<Data.UnitData>();

            var nameField = typeof(Data.UnitData).GetField("unitName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var factionField = typeof(Data.UnitData).GetField("faction",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var healthField = typeof(Data.UnitData).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var movementField = typeof(Data.UnitData).GetField("movementRange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var accuracyField = typeof(Data.UnitData).GetField("baseAccuracy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defenseField = typeof(Data.UnitData).GetField("baseDefense",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weaponField = typeof(Data.UnitData).GetField("startingWeapon",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            nameField?.SetValue(unit, name);
            factionField?.SetValue(unit, faction);
            healthField?.SetValue(unit, health);
            movementField?.SetValue(unit, movement);
            accuracyField?.SetValue(unit, accuracy);
            defenseField?.SetValue(unit, defense);
            weaponField?.SetValue(unit, weapon);

            string assetPath = $"{path}/{name}.asset";
            AssetDatabase.CreateAsset(unit, assetPath);
        }
    }
}
