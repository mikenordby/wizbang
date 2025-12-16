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
            DebugLog.Info($"EnemyPool.Cleanup: Deactivated {cleanedCount} distant enemies, active={activeItems.Count}");
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
            // Blob/Skeleton: Always available
            // Ogre: Available after 30 seconds
            // Dragon: Available after 60 seconds
            if (stats.enemyName == "Ogre" && gameTime < 30f)
                continue;
            if (stats.enemyName == "Dragon" && gameTime < 60f)
                continue;
                
            availableTypes.Add(stats);
        }
        
        if (availableTypes.Count == 0)
        {
            // Fallback to any enemy
            return enemyTypes[Random.Range(0, enemyTypes.Length)];
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