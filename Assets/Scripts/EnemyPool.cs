using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for enemies to avoid instantiate/destroy overhead.
/// Supports multiple enemy types via EnemyStats.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private int maxPoolSize = 500;
    [SerializeField] private Transform poolParent;
    [SerializeField] private EnemyStats[] enemyTypes; // Array of enemy types
    
    private List<Enemy> pool;
    private Transform playerTransform;
    
    private void Awake()
    {
        if (poolParent == null)
            poolParent = transform;
        
        pool = new List<Enemy>(maxPoolSize);
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewEnemy();
        }
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
        foreach (Enemy enemy in pool)
        {
            if (!enemy.IsActive)
            {
                // Ensure player reference is set before activation
                if (playerTransform != null)
                    enemy.SetPlayerTransform(playerTransform);
                Debug.Log($"EnemyPool.GetEnemy: Reusing pooled enemy, active={GetActiveCount()}/{pool.Count}");
                return enemy;
            }
        }
        
        if (pool.Count < maxPoolSize)
        {
            Debug.Log($"EnemyPool.GetEnemy: Creating new enemy, pool={pool.Count}/{maxPoolSize}");
            return CreateNewEnemy();
        }
        
        Debug.LogWarning("Enemy pool exhausted, reusing enemy");
        return pool[0];
    }
    
    public void ReturnEnemy(Enemy enemy)
    {
        enemy.Deactivate();
    }
    
    public int GetActiveCount()
    {
        int count = 0;
        foreach (Enemy enemy in pool)
        {
            if (enemy.IsActive) count++;
        }
        return count;
    }
    
    public void CleanupDistantEnemies(float maxDistance)
    {
        int cleanedCount = 0;
        foreach (Enemy enemy in pool)
        {
            if (enemy.IsActive && enemy.IsTooFarFromPlayer(maxDistance))
            {
                enemy.Deactivate();
                cleanedCount++;
            }
        }
        if (cleanedCount > 0)
            Debug.Log($"EnemyPool.Cleanup: Deactivated {cleanedCount} distant enemies, active={GetActiveCount()}");
    }
    
    /// <summary>
    /// Get a random enemy type from the configured types
    /// </summary>
    public EnemyStats GetRandomEnemyType()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("No enemy types configured!");
            return null;
        }
        
        return enemyTypes[Random.Range(0, enemyTypes.Length)];
    }
    
    private Enemy CreateNewEnemy()
    {
        GameObject enemyObj = Instantiate(enemyPrefab, poolParent);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy == null)
        {
            Debug.LogError("Enemy prefab must have Enemy component!");
            return null;
        }
        
        // Don't initialize here - player reference may not be set yet
        // Will initialize on first activation
        
        enemy.Deactivate();
        pool.Add(enemy);
        
        return enemy;
    }
}