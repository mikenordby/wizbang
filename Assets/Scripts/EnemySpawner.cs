using UnityEngine;

/// <summary>
/// Spawns enemies just outside the camera view at regular intervals.
/// Optimized for spawning many enemies without performance impact.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 0.5f; // Starting: 2 per second
    [SerializeField] private float minSpawnInterval = 0.05f; // Cap at 20 per second
    [SerializeField] private float spawnDistanceFromCamera = 12f; // Just outside view
    
    [Header("References")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    
    [Header("Cleanup")]
    [SerializeField] private float cleanupInterval = 5f; // Check for distant enemies
    [SerializeField] private float maxEnemyDistance = 60f; // Deactivate if beyond this (increased from 30)
    
    private float spawnTimer;
    private float cleanupTimer;
    private float gameTime; // Track time for spawn rate scaling
    private float currentSpawnInterval;
    private bool ogreUnlocked = false;
    private bool dragonUnlocked = false;
    
    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (player == null)
            player = GameObject.Find("Player")?.transform;
        
        if (enemyPool != null && player != null)
        {
            enemyPool.SetPlayer(player);
        }
        else
        {
            DebugLog.Error("EnemySpawner: Missing enemyPool or player reference!");
        }
        
        currentSpawnInterval = baseSpawnInterval;
        spawnTimer = currentSpawnInterval;
        cleanupTimer = cleanupInterval;
    }
    
    private void Update()
    {
        if (enemyPool == null || player == null || GameState.IsPaused) return;
        
        // Track game time and scale spawn rate (doubles every minute)
        gameTime += Time.deltaTime;
        
        // Check for enemy unlocks
        if (!ogreUnlocked && gameTime >= 30f)
        {
            ogreUnlocked = true;
            DebugLog.Info("[EnemySpawner] Ogres are now spawning!");
        }
        if (!dragonUnlocked && gameTime >= 60f)
        {
            dragonUnlocked = true;
            DebugLog.Info("[EnemySpawner] Dragons are now spawning!");
        }
        
        float minutesElapsed = gameTime / 60f;
        float spawnMultiplier = Mathf.Pow(2f, minutesElapsed); // 1x, 2x, 4x, 8x...
        currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval / spawnMultiplier);
        
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = currentSpawnInterval;
        }
        
        cleanupTimer -= Time.deltaTime;
        if (cleanupTimer <= 0f)
        {
            enemyPool.CleanupDistantEnemies(maxEnemyDistance);
            cleanupTimer = cleanupInterval;
        }
    }
    
    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawnPositionOutsideView();
        
        EnemyStats stats = enemyPool.GetRandomEnemyType(gameTime);
        Enemy enemy = enemyPool.GetEnemy(stats);
        if (enemy != null && stats != null)
        {
            // Scale enemy health based on game time (doubles every minute)
            float minutesElapsed = gameTime / 60f;
            float healthMultiplier = Mathf.Pow(2f, minutesElapsed);
            
            // Create scaled stats (copy to avoid modifying the original asset)
            EnemyStats scaledStats = ScriptableObject.CreateInstance<EnemyStats>();
            scaledStats.enemyName = stats.enemyName;
            scaledStats.maxHealth = stats.maxHealth * healthMultiplier;
            scaledStats.moveSpeed = stats.moveSpeed;
            scaledStats.contactDamage = stats.contactDamage;
            scaledStats.xpDrop = stats.xpDrop;
            scaledStats.color = stats.color;
            scaledStats.scale = stats.scale;
            
            enemy.Activate(spawnPosition, scaledStats);
            DebugLog.Info($"Spawned {stats.enemyName} at {spawnPosition}, active: {enemyPool.GetActiveCount()}");
        }
        else
        {
            DebugLog.Warning("Failed to spawn enemy - null enemy or stats");
        }
    }
    
    private Vector3 GetSpawnPositionOutsideView()
    {
        int edge = Random.Range(0, 4);
        Vector3 spawnPos = player.position;
        
        switch (edge)
        {
            case 0: // Top
                spawnPos += new Vector3(
                    Random.Range(-spawnDistanceFromCamera, spawnDistanceFromCamera),
                    spawnDistanceFromCamera,
                    0f
                );
                break;
            case 1: // Bottom
                spawnPos += new Vector3(
                    Random.Range(-spawnDistanceFromCamera, spawnDistanceFromCamera),
                    -spawnDistanceFromCamera,
                    0f
                );
                break;
            case 2: // Left
                spawnPos += new Vector3(
                    -spawnDistanceFromCamera,
                    Random.Range(-spawnDistanceFromCamera, spawnDistanceFromCamera),
                    0f
                );
                break;
            case 3: // Right
                spawnPos += new Vector3(
                    spawnDistanceFromCamera,
                    Random.Range(-spawnDistanceFromCamera, spawnDistanceFromCamera),
                    0f
                );
                break;
        }
        
        return spawnPos;
    }
    
    private void OnGUI()
    {
        if (enemyPool != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Active Enemies: {enemyPool.GetActiveCount()}");
        }
    }
}