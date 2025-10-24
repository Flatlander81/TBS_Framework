using System.Collections.Generic;
using UnityEngine;

namespace TBS.Grid
{
    /// <summary>
    /// A* pathfinding algorithm implementation for the grid system.
    /// Finds the shortest path between two points on the tactical grid.
    /// </summary>
    public class Pathfinding
    {
        private GridManager gridManager;

        public Pathfinding(GridManager grid)
        {
            gridManager = grid;
        }

        /// <summary>
        /// Finds a path from start to end using the A* algorithm.
        /// Returns null if no path exists.
        /// </summary>
        public List<GridTile> FindPath(Vector2Int start, Vector2Int end)
        {
            GridTile startTile = gridManager.GetTile(start);
            GridTile endTile = gridManager.GetTile(end);

            if (startTile == null || endTile == null)
                return null;

            // Check if end tile is walkable (unless it's occupied by our target)
            if (!endTile.IsWalkable && endTile.OccupyingUnit == null)
                return null;

            List<PathNode> openSet = new List<PathNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

            PathNode startNode = new PathNode(start, null, 0, GetHeuristic(start, end));
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Get node with lowest F cost
                PathNode currentNode = GetLowestFCostNode(openSet);
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.Position);

                // Check if we reached the goal
                if (currentNode.Position == end)
                {
                    return ReconstructPath(currentNode);
                }

                // Check all neighbors
                List<GridTile> neighbors = gridManager.GetNeighbors(currentNode.Position);
                foreach (GridTile neighborTile in neighbors)
                {
                    Vector2Int neighborPos = neighborTile.GridPosition;

                    // Skip if already evaluated
                    if (closedSet.Contains(neighborPos))
                        continue;

                    // Skip if not walkable (unless it's the end tile)
                    if (!neighborTile.IsWalkable && neighborPos != end)
                        continue;

                    float newGCost = currentNode.GCost + 1; // Each step costs 1
                    PathNode existingNode = openSet.Find(n => n.Position == neighborPos);

                    if (existingNode == null)
                    {
                        // Add new node to open set
                        PathNode newNode = new PathNode(
                            neighborPos,
                            currentNode,
                            newGCost,
                            GetHeuristic(neighborPos, end)
                        );
                        openSet.Add(newNode);
                    }
                    else if (newGCost < existingNode.GCost)
                    {
                        // Update existing node with better path
                        existingNode.GCost = newGCost;
                        existingNode.Parent = currentNode;
                    }
                }
            }

            // No path found
            return null;
        }

        /// <summary>
        /// Gets the node with the lowest F cost from the open set.
        /// </summary>
        private PathNode GetLowestFCostNode(List<PathNode> nodes)
        {
            PathNode lowest = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].FCost < lowest.FCost ||
                    (nodes[i].FCost == lowest.FCost && nodes[i].HCost < lowest.HCost))
                {
                    lowest = nodes[i];
                }
            }
            return lowest;
        }

        /// <summary>
        /// Calculates the heuristic (Manhattan distance) between two positions.
        /// </summary>
        private float GetHeuristic(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// Reconstructs the path by following parent nodes.
        /// </summary>
        private List<GridTile> ReconstructPath(PathNode endNode)
        {
            List<GridTile> path = new List<GridTile>();
            PathNode currentNode = endNode;

            while (currentNode != null)
            {
                GridTile tile = gridManager.GetTile(currentNode.Position);
                if (tile != null)
                    path.Add(tile);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Represents a node in the pathfinding algorithm.
        /// </summary>
        private class PathNode
        {
            public Vector2Int Position;
            public PathNode Parent;
            public float GCost; // Distance from start
            public float HCost; // Heuristic distance to end
            public float FCost => GCost + HCost; // Total cost

            public PathNode(Vector2Int position, PathNode parent, float gCost, float hCost)
            {
                Position = position;
                Parent = parent;
                GCost = gCost;
                HCost = hCost;
            }
        }
    }
}
