using UnityEngine;

namespace TBS.Grid
{
    /// <summary>
    /// Represents a single tile in the tactical grid.
    /// Contains information about position, cover, occupancy, and traversability.
    /// </summary>
    public class GridTile : MonoBehaviour
    {
        [Header("Tile Properties")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private bool isWalkable = true;
        [SerializeField] private CoverType coverType = CoverType.None;

        [Header("Visual Settings")]
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color attackRangeColor = Color.red;
        [SerializeField] private Color moveRangeColor = Color.blue;

        private Units.Unit occupyingUnit;
        private Material tileMaterial;

        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }

        public bool IsWalkable => isWalkable && occupyingUnit == null;
        public bool IsOccupied => occupyingUnit != null;
        public Units.Unit OccupyingUnit => occupyingUnit;
        public CoverType Cover => coverType;

        private void Awake()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponent<Renderer>();

            if (tileRenderer != null)
            {
                tileMaterial = tileRenderer.material;
            }
        }

        /// <summary>
        /// Sets the unit currently occupying this tile.
        /// </summary>
        public void SetOccupyingUnit(Units.Unit unit)
        {
            occupyingUnit = unit;
        }

        /// <summary>
        /// Clears the unit from this tile.
        /// </summary>
        public void ClearOccupyingUnit()
        {
            occupyingUnit = null;
        }

        /// <summary>
        /// Sets the cover level for this tile.
        /// </summary>
        public void SetCoverType(CoverType cover)
        {
            coverType = cover;
        }

        /// <summary>
        /// Sets whether this tile can be walked on.
        /// </summary>
        public void SetWalkable(bool walkable)
        {
            isWalkable = walkable;
        }

        /// <summary>
        /// Highlights the tile with a specific color.
        /// </summary>
        public void Highlight(TileHighlightType highlightType)
        {
            if (tileMaterial == null) return;

            switch (highlightType)
            {
                case TileHighlightType.None:
                    tileMaterial.color = defaultColor;
                    break;
                case TileHighlightType.Selected:
                    tileMaterial.color = highlightColor;
                    break;
                case TileHighlightType.MovementRange:
                    tileMaterial.color = moveRangeColor;
                    break;
                case TileHighlightType.AttackRange:
                    tileMaterial.color = attackRangeColor;
                    break;
            }
        }

        /// <summary>
        /// Resets the tile to its default visual state.
        /// </summary>
        public void ResetHighlight()
        {
            Highlight(TileHighlightType.None);
        }

        /// <summary>
        /// Gets the world position of the tile center.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }

    /// <summary>
    /// Types of cover available on tiles.
    /// </summary>
    public enum CoverType
    {
        None = 0,      // No cover, no defense bonus
        Half = 1,      // Half cover, moderate defense bonus
        Full = 2       // Full cover, high defense bonus
    }

    /// <summary>
    /// Types of visual highlights for tiles.
    /// </summary>
    public enum TileHighlightType
    {
        None,
        Selected,
        MovementRange,
        AttackRange
    }
}
