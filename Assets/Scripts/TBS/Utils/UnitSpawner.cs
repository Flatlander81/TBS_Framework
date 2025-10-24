using UnityEngine;
using TBS.Units;
using TBS.Combat;
using TBS.Grid;

namespace TBS.Utils
{
    /// <summary>
    /// Utility for spawning units at specified grid positions.
    /// Can be used in editor or at runtime.
    /// </summary>
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private GridManager gridManager;

        [System.Serializable]
        public class UnitSpawnData
        {
            public string unitName = "Soldier";
            public UnitFaction faction = UnitFaction.Player;
            public Vector2Int spawnPosition;
            public int maxHealth = 100;
            public int movementRange = 4;
            public int baseAccuracy = 75;
            public WeaponData startingWeapon;
            public Color unitColor = Color.blue;
        }

        [SerializeField] private UnitSpawnData[] unitsToSpawn;

        private void Start()
        {
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();

            SpawnAllUnits();
        }

        /// <summary>
        /// Spawns all configured units.
        /// </summary>
        [ContextMenu("Spawn All Units")]
        public void SpawnAllUnits()
        {
            if (unitsToSpawn == null || unitsToSpawn.Length == 0)
            {
                Debug.LogWarning("No units configured to spawn.");
                return;
            }

            foreach (UnitSpawnData spawnData in unitsToSpawn)
            {
                SpawnUnit(spawnData);
            }
        }

        /// <summary>
        /// Spawns a single unit from spawn data.
        /// </summary>
        public Unit SpawnUnit(UnitSpawnData spawnData)
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found!");
                return null;
            }

            GridTile spawnTile = gridManager.GetTile(spawnData.spawnPosition);
            if (spawnTile == null)
            {
                Debug.LogError($"Invalid spawn position: {spawnData.spawnPosition}");
                return null;
            }

            if (spawnTile.IsOccupied)
            {
                Debug.LogWarning($"Tile {spawnData.spawnPosition} is already occupied!");
                return null;
            }

            // Instantiate unit
            GameObject unitObj = Instantiate(unitPrefab, spawnTile.GetWorldPosition(), Quaternion.identity);
            unitObj.name = $"{spawnData.unitName} ({spawnData.faction})";

            Unit unit = unitObj.GetComponent<Unit>();
            if (unit == null)
            {
                Debug.LogError("Unit prefab doesn't have Unit component!");
                Destroy(unitObj);
                return null;
            }

            // Initialize unit
            unit.Initialize(spawnData.spawnPosition, gridManager);

            // Equip weapon if specified
            if (spawnData.startingWeapon != null)
            {
                unit.EquipWeapon(spawnData.startingWeapon);
            }

            // Apply visual color
            Renderer renderer = unitObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = spawnData.unitColor;
            }

            Debug.Log($"Spawned {spawnData.unitName} at {spawnData.spawnPosition}");
            return unit;
        }

        /// <summary>
        /// Quick spawn method for runtime use.
        /// </summary>
        public static Unit QuickSpawn(GameObject prefab, Vector2Int position, UnitFaction faction,
            WeaponData weapon, GridManager grid)
        {
            UnitSpawnData data = new UnitSpawnData
            {
                unitName = faction == UnitFaction.Player ? "Player Unit" : "Enemy Unit",
                faction = faction,
                spawnPosition = position,
                startingWeapon = weapon,
                unitColor = faction == UnitFaction.Player ? Color.blue : Color.red
            };

            UnitSpawner spawner = FindObjectOfType<UnitSpawner>();
            if (spawner == null)
            {
                GameObject spawnerObj = new GameObject("UnitSpawner");
                spawner = spawnerObj.AddComponent<UnitSpawner>();
                spawner.unitPrefab = prefab;
                spawner.gridManager = grid;
            }

            return spawner.SpawnUnit(data);
        }
    }
}
