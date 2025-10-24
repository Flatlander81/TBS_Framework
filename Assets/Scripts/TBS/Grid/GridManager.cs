using System.Collections.Generic;
using UnityEngine;

namespace TBS.Grid
{
    /// <summary>
    /// Manages the tactical grid system.
    /// Handles tile creation, pathfinding, range calculations, and line of sight.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private GameObject tilePrefab;

        [Header("Grid Origin")]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        private GridTile[,] tiles;
        private Pathfinding pathfinding;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public float TileSize => tileSize;

        private void Awake()
        {
            GenerateGrid();
            pathfinding = new Pathfinding(this);
        }

        /// <summary>
        /// Generates the grid based on specified dimensions.
        /// </summary>
        private void GenerateGrid()
        {
            tiles = new GridTile[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Vector3 worldPos = GridToWorldPosition(x, z);
                    GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    tileObj.name = $"Tile_{x}_{z}";

                    GridTile tile = tileObj.GetComponent<GridTile>();
                    if (tile == null)
                        tile = tileObj.AddComponent<GridTile>();

                    tile.GridPosition = new Vector2Int(x, z);
                    tiles[x, z] = tile;
                }
            }
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(int x, int z)
        {
            return gridOrigin + new Vector3(x * tileSize, 0, z * tileSize);
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return GridToWorldPosition(gridPos.x, gridPos.y);
        }

        /// <summary>
        /// Converts world position to grid coordinates.
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridOrigin;
            int x = Mathf.RoundToInt(localPos.x / tileSize);
            int z = Mathf.RoundToInt(localPos.z / tileSize);
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// Gets a tile at the specified grid position.
        /// </summary>
        public GridTile GetTile(Vector2Int gridPos)
        {
            return GetTile(gridPos.x, gridPos.y);
        }

        /// <summary>
        /// Gets a tile at the specified grid coordinates.
        /// </summary>
        public GridTile GetTile(int x, int z)
        {
            if (IsValidGridPosition(x, z))
                return tiles[x, z];
            return null;
        }

        /// <summary>
        /// Checks if grid coordinates are within bounds.
        /// </summary>
        public bool IsValidGridPosition(int x, int z)
        {
            return x >= 0 && x < gridWidth && z >= 0 && z < gridHeight;
        }

        /// <summary>
        /// Checks if grid position is within bounds.
        /// </summary>
        public bool IsValidGridPosition(Vector2Int gridPos)
        {
            return IsValidGridPosition(gridPos.x, gridPos.y);
        }

        /// <summary>
        /// Gets all tiles within a certain range from a position.
        /// </summary>
        public List<GridTile> GetTilesInRange(Vector2Int origin, int range, bool requireWalkable = false)
        {
            List<GridTile> tilesInRange = new List<GridTile>();

            for (int x = -range; x <= range; x++)
            {
                for (int z = -range; z <= range; z++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(z); // Manhattan distance
                    if (distance <= range)
                    {
                        Vector2Int checkPos = origin + new Vector2Int(x, z);
                        if (IsValidGridPosition(checkPos))
                        {
                            GridTile tile = GetTile(checkPos);
                            if (tile != null && (!requireWalkable || tile.IsWalkable))
                            {
                                tilesInRange.Add(tile);
                            }
                        }
                    }
                }
            }

            return tilesInRange;
        }

        /// <summary>
        /// Gets all neighboring tiles (4-directional).
        /// </summary>
        public List<GridTile> GetNeighbors(Vector2Int gridPos)
        {
            List<GridTile> neighbors = new List<GridTile>();

            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // North
                new Vector2Int(1, 0),   // East
                new Vector2Int(0, -1),  // South
                new Vector2Int(-1, 0)   // West
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = gridPos + dir;
                GridTile neighbor = GetTile(neighborPos);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Calculates the Manhattan distance between two grid positions.
        /// </summary>
        public int GetDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// Finds a path from start to end using A* pathfinding.
        /// </summary>
        public List<GridTile> FindPath(Vector2Int start, Vector2Int end)
        {
            return pathfinding.FindPath(start, end);
        }

        /// <summary>
        /// Gets all tiles reachable within a movement range.
        /// Uses flood-fill to respect walkability and movement costs.
        /// </summary>
        public List<GridTile> GetReachableTiles(Vector2Int origin, int movementRange)
        {
            List<GridTile> reachable = new List<GridTile>();
            Queue<(Vector2Int pos, int remainingMoves)> queue = new Queue<(Vector2Int, int)>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            queue.Enqueue((origin, movementRange));
            visited.Add(origin);

            while (queue.Count > 0)
            {
                var (currentPos, remainingMoves) = queue.Dequeue();
                GridTile currentTile = GetTile(currentPos);

                if (currentTile != null && currentPos != origin)
                {
                    reachable.Add(currentTile);
                }

                if (remainingMoves > 0)
                {
                    foreach (GridTile neighbor in GetNeighbors(currentPos))
                    {
                        Vector2Int neighborPos = neighbor.GridPosition;
                        if (!visited.Contains(neighborPos) && neighbor.IsWalkable)
                        {
                            visited.Add(neighborPos);
                            queue.Enqueue((neighborPos, remainingMoves - 1));
                        }
                    }
                }
            }

            return reachable;
        }

        /// <summary>
        /// Checks if there is line of sight between two positions.
        /// Uses a simple raycast approach.
        /// </summary>
        public bool HasLineOfSight(Vector2Int from, Vector2Int to)
        {
            Vector3 fromWorld = GridToWorldPosition(from) + Vector3.up;
            Vector3 toWorld = GridToWorldPosition(to) + Vector3.up;

            Vector3 direction = toWorld - fromWorld;
            float distance = direction.magnitude;

            // Raycast to check for obstacles
            if (Physics.Raycast(fromWorld, direction.normalized, out RaycastHit hit, distance))
            {
                // Check if we hit the target tile or an obstacle
                GridTile hitTile = hit.collider.GetComponent<GridTile>();
                if (hitTile != null && hitTile.GridPosition == to)
                {
                    return true;
                }
                return false;
            }

            return true; // No obstacles found
        }

        /// <summary>
        /// Clears all tile highlights.
        /// </summary>
        public void ClearAllHighlights()
        {
            foreach (GridTile tile in tiles)
            {
                if (tile != null)
                    tile.ResetHighlight();
            }
        }

        /// <summary>
        /// Highlights tiles for movement range.
        /// </summary>
        public void HighlightMovementRange(Vector2Int origin, int range)
        {
            List<GridTile> reachable = GetReachableTiles(origin, range);
            foreach (GridTile tile in reachable)
            {
                tile.Highlight(TileHighlightType.MovementRange);
            }
        }

        /// <summary>
        /// Highlights tiles for attack range.
        /// </summary>
        public void HighlightAttackRange(Vector2Int origin, int range)
        {
            List<GridTile> tilesInRange = GetTilesInRange(origin, range);
            foreach (GridTile tile in tilesInRange)
            {
                tile.Highlight(TileHighlightType.AttackRange);
            }
        }
    }
}
