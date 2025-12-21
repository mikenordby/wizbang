using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spatial hash grid for efficient 2D collision detection.
/// Divides world space into fixed-size cells and only checks entities in nearby cells.
/// Reduces O(n²) collision checks to O(n) by spatial partitioning.
/// </summary>
public class SpatialHashGrid
{
    private float cellSize;
    private Dictionary<Vector2Int, List<ICollidable>> grid;
    private Vector2 gridOrigin;
    
    // Pooled query result list (eliminates per-frame allocations)
    // IMPORTANT: Callers must NOT store this reference - use immediately then discard
    private static readonly List<ICollidable> sharedQueryResults = new List<ICollidable>(128);
    
    // Statistics for profiling
    public int TotalCells => grid.Count;
    public int OccupiedCells { get; private set; }
    
    /// <summary>
    /// Create a new spatial hash grid
    /// </summary>
    /// <param name="cellSize">Size of each grid cell (should be ~2x max collision radius)</param>
    /// <param name="origin">World origin of the grid</param>
    public SpatialHashGrid(float cellSize, Vector2 origin = default)
    {
        this.cellSize = cellSize;
        this.gridOrigin = origin;
        // Pre-allocate dictionary to avoid resizing during gameplay
        this.grid = new Dictionary<Vector2Int, List<ICollidable>>(1024);
    }
    
    /// <summary>
    /// Clear all entities from the grid (call at start of each frame)
    /// </summary>
    public void Clear()
    {
        // Reuse existing lists instead of clearing dictionary
        // This avoids allocating new List objects every frame
        foreach (var list in grid.Values)
        {
            list.Clear();
        }
        OccupiedCells = 0;
    }
    
    /// <summary>
    /// Convert world position to grid cell coordinates
    /// </summary>
    private Vector2Int GetCellCoords(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
            Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize)
        );
    }
    
    /// <summary>
    /// Insert an entity into the grid
    /// </summary>
    public void Insert(ICollidable entity)
    {
        if (entity == null || !entity.IsActive) return;
        
        Vector2Int cell = GetCellCoords(entity.Position);
        
        if (!grid.TryGetValue(cell, out var list))
        {
            // Create new list for this cell (typical 8-16 entities per cell)
            list = new List<ICollidable>(16);
            grid[cell] = list;
            OccupiedCells++;
        }
        
        list.Add(entity);
    }
    
    /// <summary>
    /// Query all entities near a position that match the layer mask.
    /// Checks the cell containing the position plus all adjacent cells (3x3 = 9 cells).
    /// 
    /// OPTIMIZATION: Returns a shared pooled list to eliminate GC allocations.
    /// IMPORTANT: Caller must use the results immediately and NOT store the returned list reference.
    /// The list contents will be overwritten on the next Query() call.
    /// </summary>
    /// <param name="position">World position to query around</param>
    /// <param name="radius">Query radius (determines how many cells to check)</param>
    /// <param name="mask">Layer mask to filter results</param>
    /// <returns>Shared pooled list of entities (DO NOT STORE - use immediately)</returns>
    public List<ICollidable> Query(Vector3 position, float radius, CollisionLayer mask)
    {
        // Clear shared results from previous query (zero allocation)
        sharedQueryResults.Clear();
        
        Vector2Int centerCell = GetCellCoords(position);
        
        // Calculate how many cells the radius spans
        // For radius 0.5 and cellSize 2.0: cellRadius = 1 (check 3x3 cells)
        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        
        // Check all cells within radius
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                
                if (grid.TryGetValue(cell, out var list))
                {
                    // Filter by layer mask and add to shared results
                    foreach (var entity in list)
                    {
                        if (entity != null && entity.IsActive && (entity.Layer & mask) != 0)
                        {
                            sharedQueryResults.Add(entity);
                        }
                    }
                }
            }
        }
        
        return sharedQueryResults;
    }
    
    /// <summary>
    /// Draw grid visualization in Scene view for debugging
    /// </summary>
    public void DrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
        
        foreach (var kvp in grid)
        {
            if (kvp.Value.Count == 0) continue; // Skip empty cells
            
            Vector2Int cell = kvp.Key;
            Vector3 worldPos = new Vector3(
                gridOrigin.x + cell.x * cellSize,
                gridOrigin.y + cell.y * cellSize,
                0
            );
            
            // Draw cell bounds
            Vector3 center = worldPos + Vector3.one * cellSize * 0.5f;
            Vector3 size = new Vector3(cellSize, cellSize, 0);
            Gizmos.DrawWireCube(center, size);
            
            // Tint based on entity count (darker = more entities)
            int count = kvp.Value.Count;
            float intensity = Mathf.Clamp01(count / 10f);
            Gizmos.color = Color.Lerp(Color.green, Color.red, intensity);
            Gizmos.DrawCube(center, size * 0.1f); // Small cube at center
        }
    }
    
    /// <summary>
    /// Get debug statistics
    /// </summary>
    public string GetDebugStats()
    {
        int totalEntities = 0;
        int maxEntitiesPerCell = 0;
        
        foreach (var list in grid.Values)
        {
            totalEntities += list.Count;
            if (list.Count > maxEntitiesPerCell)
                maxEntitiesPerCell = list.Count;
        }
        
        float avgEntitiesPerCell = OccupiedCells > 0 ? (float)totalEntities / OccupiedCells : 0;
        
        return $"Grid Stats: {OccupiedCells} occupied cells, {totalEntities} entities, " +
               $"avg {avgEntitiesPerCell:F1}/cell, max {maxEntitiesPerCell}/cell";
    }
}
