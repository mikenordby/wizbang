using UnityEngine;

/// <summary>
/// Spawns enemies just outside the camera view at regular intervals.
/// Optimized for spawning many enemies without performance impact.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 0.5f; // Starting: 2 per second
    [SerializeField] private float minSpawnInterval = 0.1f; // Max: 10 per second
    [SerializeField] private float spawnRateIncreasePerMinute = 0.1f; // Linear scaling
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
        
        // Track game time and scale spawn rate (linear increase)
        gameTime += Time.deltaTime;
        float minutesElapsed = gameTime / 60f;
        currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (spawnRateIncreasePerMinute * minutesElapsed));
        
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
        
        EnemyStats stats = enemyPool.GetRandomEnemyType();
        Enemy enemy = enemyPool.GetEnemy(stats);
        if (enemy != null && stats != null)
        {
            enemy.Activate(spawnPosition, stats);
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