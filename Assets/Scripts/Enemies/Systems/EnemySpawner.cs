using UnityEngine;

/// <summary>
/// Spawns enemies just outside the camera view at regular intervals.
/// Optimized for spawning many enemies without performance impact.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 0.25f; // Starting: 4 per second (doubled for slower-paced gameplay)
    [SerializeField] private float minSpawnInterval = 0.05f; // Cap at 20 per second
    [SerializeField] private float spawnBufferDistance = 3f; // How far outside camera view to spawn (in world units)
    
    [Header("References")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    
    [Header("Cleanup")]
    [SerializeField] private float cleanupInterval = 5f; // Check for distant enemies
    [SerializeField] private float maxEnemyDistance = 84f; // Deactivate if beyond this (scaled for 1.4x larger camera)
    
    private float spawnTimer;
    private float cleanupTimer;
    private float gameTime; // Track time for spawn rate scaling
    private float currentSpawnInterval;
    private bool skeletonUnlocked = false;
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
        // ONLY spawn enemies during gameplay phase
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        
        if (enemyPool == null || player == null || GameState.IsPaused) return;
        
        // Track game time and scale spawn rate (doubles every minute)
        gameTime += Time.deltaTime;
        
        // Enemy unlock progression: Goblins (0-30s) → Skeletons (30-60s) → Dragons (60s+)
        if (!skeletonUnlocked && gameTime >= 30f)
        {
            skeletonUnlocked = true;
            DebugLog.Info("[EnemySpawner] Skeletons are now spawning!", "EnemySpawner");
        }
        if (!dragonUnlocked && gameTime >= 60f)
        {
            dragonUnlocked = true;
            DebugLog.Info("[EnemySpawner] Dragons are now spawning!", "EnemySpawner");
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
            
            // Pass scaling multiplier to enemy instead of creating new ScriptableObject
            // This avoids GC allocation spam (was creating 60+ ScriptableObjects per minute)
            enemy.Activate(spawnPosition, stats, healthMultiplier);
            DebugLog.Verbose($"Spawned {stats.enemyName} at {spawnPosition}, active: {enemyPool.GetActiveCount()}", "EnemySpawner");
        }
        else
        {
            DebugLog.Warning("Failed to spawn enemy - null enemy or stats");
        }
    }
    
    private Vector3 GetSpawnPositionOutsideView()
    {
        // Calculate actual camera bounds based on orthographic size and aspect ratio
        float camHalfHeight = mainCamera != null ? mainCamera.orthographicSize : 7f;
        float camHalfWidth = mainCamera != null ? mainCamera.orthographicSize * mainCamera.aspect : 12f;

        // Spawn distance = camera edge + buffer
        float spawnDistanceY = camHalfHeight + spawnBufferDistance;
        float spawnDistanceX = camHalfWidth + spawnBufferDistance;

        int edge = Random.Range(0, 4);
        Vector3 spawnPos = player.position;

        switch (edge)
        {
            case 0: // Top
                spawnPos += new Vector3(
                    Random.Range(-spawnDistanceX, spawnDistanceX),
                    spawnDistanceY,
                    0f
                );
                break;
            case 1: // Bottom
                spawnPos += new Vector3(
                    Random.Range(-spawnDistanceX, spawnDistanceX),
                    -spawnDistanceY,
                    0f
                );
                break;
            case 2: // Left
                spawnPos += new Vector3(
                    -spawnDistanceX,
                    Random.Range(-spawnDistanceY, spawnDistanceY),
                    0f
                );
                break;
            case 3: // Right
                spawnPos += new Vector3(
                    spawnDistanceX,
                    Random.Range(-spawnDistanceY, spawnDistanceY),
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