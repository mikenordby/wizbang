using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for enemies to avoid instantiate/destroy overhead.
/// Supports multiple enemy types via EnemyStats.
/// </summary>
public class EnemyPool : ObjectPool<Enemy>
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField] private EnemyStats[] enemyTypes; // Array of enemy types
    
    private Transform playerTransform;
    
    protected override void Awake()
    {
        if (poolParent == null)
            poolParent = transform;
        
        base.Awake();
    }
    
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    /// <summary>
    /// Get an inactive enemy and activate it with specified stats
    /// </summary>
    public Enemy GetEnemy(EnemyStats stats)
    {
        Enemy enemy = GetItem();
        
        if (enemy != null)
        {
            // Ensure player reference is set before activation
            if (playerTransform != null)
                enemy.SetPlayerTransform(playerTransform);
        }
        
        return enemy;
    }
    
    public void ReturnEnemy(Enemy enemy)
    {
        ReturnItem(enemy);
        // NOTE: Don't call enemy.Deactivate() here - it creates infinite recursion
        // Deactivate() already calls ReturnEnemy() as a failsafe
    }
    
    /// <summary>
    /// Get cached list of active enemies (no GC allocations)
    /// </summary>
    public List<Enemy> GetActiveEnemies() => activeItems;
    
    public void CleanupDistantEnemies(float maxDistance)
    {
        int cleanedCount = 0;
        // Iterate backwards to safely remove during iteration
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            Enemy enemy = activeItems[i];
            if (enemy.IsTooFarFromPlayer(maxDistance))
            {
                ReturnEnemy(enemy); // This removes from activeItems list
                cleanedCount++;
            }
        }
        if (cleanedCount > 0)
            DebugLog.Verbose($"EnemyPool.Cleanup: Deactivated {cleanedCount} distant enemies, active={activeItems.Count}", "EnemyPool");
    }
    
    /// <summary>
    /// Get a random enemy type from the configured types
    /// </summary>
    public EnemyStats GetRandomEnemyType()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            DebugLog.Error("No enemy types configured!");
            return null;
        }
        
        return enemyTypes[Random.Range(0, enemyTypes.Length)];
    }
    
    /// <summary>
    /// Get a random enemy type based on elapsed game time (unlocks stronger enemies over time)
    /// </summary>
    public EnemyStats GetRandomEnemyType(float gameTime)
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            DebugLog.Error("No enemy types configured!");
            return null;
        }
        
        // Build list of available enemy types based on game time
        List<EnemyStats> availableTypes = new List<EnemyStats>();

        foreach (EnemyStats stats in enemyTypes)
        {
            // Enemy progression: Goblins (0-30s) → Skeletons (30-60s) → Dragons (60s+)
            // Ogre: DISABLED (sprites not available)
            // Use case-insensitive comparison for robustness
            string enemyName = stats.enemyName?.Trim() ?? "";

            // DIAGNOSTIC: Log each enemy check (first 300 frames only)
            if (Time.frameCount < 300)
            {
                DebugLog.Info($"[EnemyPool] Checking enemy: '{enemyName}' at gameTime={gameTime:F2}s", "EnemyPool");
            }

            if (enemyName.Equals("Ogre", System.StringComparison.OrdinalIgnoreCase))
            {
                if (Time.frameCount < 300)
                    DebugLog.Info($"[EnemyPool] → Skipping Ogre (disabled)", "EnemyPool");
                continue; // Completely disabled - no sprites available
            }
            if (enemyName.Equals("Skeleton", System.StringComparison.OrdinalIgnoreCase) && gameTime < 30f)
            {
                if (Time.frameCount < 300)
                    DebugLog.Info($"[EnemyPool] → Skipping Skeleton (locked until 30s, current={gameTime:F2}s)", "EnemyPool");
                continue; // Unlock at 30 seconds
            }
            if (enemyName.Equals("Dragon", System.StringComparison.OrdinalIgnoreCase) && gameTime < 60f)
            {
                if (Time.frameCount < 300)
                    DebugLog.Info($"[EnemyPool] → Skipping Dragon (locked until 60s, current={gameTime:F2}s)", "EnemyPool");
                continue; // Unlock at 60 seconds
            }

            if (Time.frameCount < 300)
                DebugLog.Info($"[EnemyPool] → ADDING '{enemyName}' to available list", "EnemyPool");
            availableTypes.Add(stats);
        }

        // Debug: Log what we're selecting (first few spawns only)
        if (Time.frameCount < 300)
        {
            string availableNames = availableTypes.Count > 0
                ? string.Join(", ", System.Linq.Enumerable.Select(availableTypes, e => e.enemyName))
                : "NONE";
            DebugLog.Info($"EnemyPool.GetRandomEnemyType: gameTime={gameTime:F1}s, available=[{availableNames}]", "EnemyPool");
        }

        if (availableTypes.Count == 0)
        {
            DebugLog.Warning($"[EnemyPool] availableTypes is EMPTY at gameTime={gameTime:F2}s! Using fallback...", "EnemyPool");
            // Fallback: RESPECT time locks! Return only unlocked enemies
            foreach (EnemyStats stats in enemyTypes)
            {
                string enemyName = stats.enemyName?.Trim() ?? "";
                DebugLog.Info($"[EnemyPool] Fallback checking: '{enemyName}'", "EnemyPool");

                if (enemyName.Equals("Ogre", System.StringComparison.OrdinalIgnoreCase))
                {
                    DebugLog.Info($"[EnemyPool] Fallback: Skipping Ogre (disabled)", "EnemyPool");
                    continue; // Never use Ogre
                }
                if (enemyName.Equals("Skeleton", System.StringComparison.OrdinalIgnoreCase) && gameTime < 30f)
                {
                    DebugLog.Info($"[EnemyPool] Fallback: Skipping Skeleton (locked until 30s)", "EnemyPool");
                    continue; // Locked until 30s
                }
                if (enemyName.Equals("Dragon", System.StringComparison.OrdinalIgnoreCase) && gameTime < 60f)
                {
                    DebugLog.Info($"[EnemyPool] Fallback: Skipping Dragon (locked until 60s)", "EnemyPool");
                    continue; // Locked until 60s
                }
                DebugLog.Info($"[EnemyPool] Fallback: Returning '{enemyName}'", "EnemyPool");
                return stats; // Return first unlocked enemy (should be Goblin)
            }
            DebugLog.Error($"EnemyPool.GetRandomEnemyType: No valid enemy types available at gameTime={gameTime:F1}s! Check enemy configuration.", "EnemyPool");
            return null;
        }
        
        return availableTypes[Random.Range(0, availableTypes.Count)];
    }
    
    protected override Enemy CreateNewItem()
    {
        GameObject enemyObj = Instantiate(enemyPrefab, poolParent);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy == null)
        {
            DebugLog.Error("Enemy prefab must have Enemy component!");
            return null;
        }
        
        // Don't initialize here - player reference may not be set yet
        // Will initialize on first activation
        
        enemy.Deactivate();
        pool.Add(enemy);
        
        // Add to active list if item is being created during GetItem()
        if (!activeItems.Contains(enemy))
            activeItems.Add(enemy);
        
        return enemy;
    }
    
    protected override bool IsActive(Enemy item)
    {
        return item.IsActive;
    }
}