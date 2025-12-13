using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to setup projectile and collision systems
/// </summary>
public class ProjectileSystemSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Projectile System")]
    private static void SetupProjectileSystem()
    {
        // Create ProjectilePrefab if it doesn't exist
        GameObject projectilePrefab = GameObject.Find("ProjectilePrefab");
        if (projectilePrefab == null)
        {
            projectilePrefab = new GameObject("ProjectilePrefab");
            projectilePrefab.AddComponent<SpriteRenderer>().color = Color.yellow;
            projectilePrefab.AddComponent<Projectile>();
            projectilePrefab.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            projectilePrefab.SetActive(false); // Prefab should be inactive
            Debug.Log("Created ProjectilePrefab");
        }
        
        // Find or create GameManager
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj == null)
        {
            gameManagerObj = new GameObject("GameManager");
            Debug.Log("Created GameManager GameObject");
        }
        
        // Setup components on GameManager
        EnemySpawner enemySpawner = gameManagerObj.GetComponent<EnemySpawner>();
        EnemyPool enemyPool = gameManagerObj.GetComponent<EnemyPool>();
        ProjectileSpawner projectileSpawner = gameManagerObj.GetComponent<ProjectileSpawner>();
        if (projectileSpawner == null)
        {
            projectileSpawner = gameManagerObj.AddComponent<ProjectileSpawner>();
            Debug.Log("Added ProjectileSpawner to GameManager");
        }
        
        ProjectilePool projectilePool = gameManagerObj.GetComponent<ProjectilePool>();
        if (projectilePool == null)
        {
            projectilePool = gameManagerObj.AddComponent<ProjectilePool>();
            Debug.Log("Added ProjectilePool to GameManager");
        }
        
        CollisionManager collisionManager = gameManagerObj.GetComponent<CollisionManager>();
        if (collisionManager == null)
        {
            collisionManager = gameManagerObj.AddComponent<CollisionManager>();
            Debug.Log("Added CollisionManager to GameManager");
        }
        
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObj.AddComponent<GameManager>();
            Debug.Log("Added GameManager script to GameManager");
        }
        
        // Find references
        GameObject player = GameObject.Find("Player");
        
        // Use SerializedObject to set references
        SerializedObject soProjectilePool = new SerializedObject(projectilePool);
        soProjectilePool.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        soProjectilePool.ApplyModifiedProperties();
        
        SerializedObject soProjectileSpawner = new SerializedObject(projectileSpawner);
        soProjectileSpawner.FindProperty("playerTransform").objectReferenceValue = player?.transform;
        soProjectileSpawner.FindProperty("projectilePool").objectReferenceValue = projectilePool;
        soProjectileSpawner.FindProperty("enemyPool").objectReferenceValue = enemyPool;
        soProjectileSpawner.ApplyModifiedProperties();
        
        SerializedObject soCollisionManager = new SerializedObject(collisionManager);
        soCollisionManager.FindProperty("playerTransform").objectReferenceValue = player?.transform;
        soCollisionManager.FindProperty("projectilePool").objectReferenceValue = projectilePool;
        soCollisionManager.FindProperty("enemyPool").objectReferenceValue = enemyPool;
        soCollisionManager.ApplyModifiedProperties();
        
        Debug.Log("Projectile System Setup Complete!");
        Debug.Log("- Yellow projectiles shoot at nearest enemy (1/sec)");
        Debug.Log("- Projectiles despawn on hit or after 10 seconds");
        Debug.Log("- Game Over when player touches enemy");
        Debug.Log("- Press R to restart after Game Over");
    }
}