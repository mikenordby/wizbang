using UnityEngine;

/// <summary>
/// Auto-shoots projectiles at nearest enemy
/// </summary>
public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private float fireRate = 1f; // 1 per second
    
    private float timeSinceLastShot = 0f;
    
    private void Update()
    {
        if (GameState.IsPaused) return;
        
        timeSinceLastShot += Time.deltaTime;
        
        if (timeSinceLastShot >= fireRate)
        {
            ShootAtNearestEnemy();
            timeSinceLastShot = 0f;
        }
    }
    
    private void ShootAtNearestEnemy()
    {
        if (playerTransform == null || projectilePool == null || enemyPool == null)
            return;
        
        // Find nearest active enemy
        Enemy nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
            return; // No enemies
        
        // Get direction to enemy
        Vector3 directionToEnemy = (nearestEnemy.transform.position - playerTransform.position).normalized;
        
        // Spawn projectile
        Projectile projectile = projectilePool.GetProjectile();
        projectile.ActivateStraight(playerTransform.position, directionToEnemy);
    }
    
    private Enemy FindNearestEnemy()
    {
        Enemy nearest = null;
        float nearestDistance = float.MaxValue;
        
        // Check all enemies (pool will provide active ones through its internal list)
        Enemy[] allEnemies = enemyPool.GetComponentsInChildren<Enemy>();
        
        foreach (var enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy)
                continue;
            
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
}