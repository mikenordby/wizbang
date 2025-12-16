using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns treasure chests using chunk-based generation like rocks.
/// Approximately 1 chest per 20 chunks (5% spawn chance per chunk).
/// Chests grant a level-up perk selection without leveling up.
/// </summary>
public class TreasureChestSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private Transform player;
    
    [Tooltip("Chance that a chunk will spawn a chest (0.0 to 1.0)")]
    [SerializeField] private float chestSpawnChance = 0.05f; // 5% = ~1 per 20 chunks
    
    [Tooltip("Size of each chunk in world units (should match RockSpawner)")]
    [SerializeField] private int chunkSize = 20;
    
    [Tooltip("How far from player before despawning")]
    [SerializeField] private float despawnDistance = 35f;
    
    private Dictionary<Vector2Int, GameObject> chunkChests = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chestPool = new Queue<GameObject>();
    
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                DebugLog.Warning("[TreasureChestSpawner] Player not found!");
            }
        }
        
        DebugLog.Info($"[TreasureChestSpawner] Initialized with {chestSpawnChance * 100f}% spawn chance per {chunkSize}x{chunkSize} chunk");
    }
    
    private void Update()
    {
        if (player == null) return;
        
        Vector2Int playerChunk = GetChunkCoord(player.position);
        
        // Check nearby chunks (3x3 grid around player)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int chunk = playerChunk + new Vector2Int(x, y);
                
                if (!chunkChests.ContainsKey(chunk))
                {
                    TrySpawnChestInChunk(chunk);
                }
            }
        }
        
        // Despawn distant chests
        CleanupDistantChunks();
    }
    
    /// <summary>
    /// Try to spawn a chest in a specific chunk with seeded randomness
    /// </summary>
    private void TrySpawnChestInChunk(Vector2Int chunk)
    {
        // Use chunk coordinates as seed for consistent generation
        Random.InitState(chunk.x * 73856 + chunk.y * 19349);
        
        // Roll for chest spawn
        bool shouldSpawn = Random.value < chestSpawnChance;
        
        if (shouldSpawn)
        {
            // Random position within chunk
            Vector3 position = new Vector3(
                chunk.x * chunkSize + Random.Range(chunkSize * 0.2f, chunkSize * 0.8f), // Stay away from edges
                chunk.y * chunkSize + Random.Range(chunkSize * 0.2f, chunkSize * 0.8f),
                0f
            );
            
            // Get or create chest
            GameObject chest = GetChestFromPool();
            chest.transform.position = position;
            chest.name = $"TreasureChest_{chunk.x}_{chunk.y}";
            
            TreasureChest chestComponent = chest.GetComponent<TreasureChest>();
            if (chestComponent == null)
            {
                chestComponent = chest.AddComponent<TreasureChest>();
            }
            
            chest.SetActive(true);
            chunkChests[chunk] = chest;
            
            DebugLog.Verbose($"[TreasureChestSpawner] Spawned chest in chunk {chunk} at {position}");
        }
        else
        {
            // Mark chunk as checked but no chest
            chunkChests[chunk] = null;
        }
        
        // Restore random state
        Random.InitState((int)(Time.time * 1000));
    }
    
    /// <summary>
    /// Remove chunks that are too far from player
    /// </summary>
    private void CleanupDistantChunks()
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        
        foreach (var kvp in chunkChests)
        {
            Vector2Int chunk = kvp.Key;
            Vector3 chunkCenter = new Vector3(
                chunk.x * chunkSize + chunkSize * 0.5f,
                chunk.y * chunkSize + chunkSize * 0.5f,
                0f
            );
            
            float dist = Vector3.Distance(player.position, chunkCenter);
            
            if (dist > despawnDistance)
            {
                // Return chest to pool if it exists
                if (kvp.Value != null)
                {
                    ReturnChestToPool(kvp.Value);
                }
                chunksToRemove.Add(chunk);
            }
        }
        
        // Remove chunks from dictionary
        foreach (Vector2Int chunk in chunksToRemove)
        {
            chunkChests.Remove(chunk);
            DebugLog.Verbose($"[TreasureChestSpawner] Despawned chunk {chunk}");
        }
    }
    
    /// <summary>
    /// Get chest from pool or create new one
    /// </summary>
    private GameObject GetChestFromPool()
    {
        if (chestPool.Count > 0)
        {
            GameObject chest = chestPool.Dequeue();
            return chest;
        }
        else
        {
            GameObject chest = new GameObject("TreasureChest");
            chest.transform.SetParent(transform);
            
            // Add TreasureChest component (handles visuals and pickup)
            chest.AddComponent<TreasureChest>();
            
            return chest;
        }
    }
    
    /// <summary>
    /// Return chest to pool for reuse
    /// </summary>
    private void ReturnChestToPool(GameObject chest)
    {
        chest.SetActive(false);
        chestPool.Enqueue(chest);
    }
    
    /// <summary>
    /// Get chunk coordinate from world position
    /// </summary>
    private Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }
    
    /// <summary>
    /// Draw gizmos for debugging chunk boundaries
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || player == null) return;
        
        // Draw chunks with chests
        Gizmos.color = Color.cyan;
        foreach (var kvp in chunkChests)
        {
            if (kvp.Value != null) // Only draw if chest actually spawned
            {
                Vector2Int chunk = kvp.Key;
                Vector3 center = new Vector3(
                    chunk.x * chunkSize + chunkSize * 0.5f,
                    chunk.y * chunkSize + chunkSize * 0.5f,
                    0f
                );
                Gizmos.DrawWireCube(center, new Vector3(chunkSize, chunkSize, 0f));
            }
        }
    }
}
