using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns rock obstacles procedurally around the player in chunks.
/// Rocks block player and enemy movement but not projectiles.
/// Uses chunk-based generation for infinite worlds with consistent placement.
/// </summary>
public class RockSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private Transform player;
    
    [Tooltip("Number of rocks per chunk (10x10 unit area)")]
    [SerializeField] private int rocksPerChunk = 1; // Very low density = sparse placement
    
    [Tooltip("Size of each chunk in world units")]
    [SerializeField] private int chunkSize = 20;
    
    [Tooltip("How far from player before despawning")]
    [SerializeField] private float despawnDistance = 35f;
    
    [Header("Rock Properties")]
    [Tooltip("Rock size in world units")]
    [SerializeField] private float rockSize = 0.6f; // Single size for consistent hitboxes
    
    private Dictionary<Vector2Int, List<GameObject>> chunkRocks = new Dictionary<Vector2Int, List<GameObject>>();
    private Queue<GameObject> rockPool = new Queue<GameObject>();
    
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                DebugLog.Warning("[RockSpawner] Player not found!");
            }
        }
        
        DebugLog.Info($"[RockSpawner] Initialized with {rocksPerChunk} rocks per {chunkSize}x{chunkSize} chunk");
    }
    
    private void Update()
    {
        if (player == null) return;
        
        Vector2Int playerChunk = GetChunkCoord(player.position);
        
        // Spawn rocks in nearby chunks (3x3 grid around player)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int chunk = playerChunk + new Vector2Int(x, y);
                
                if (!chunkRocks.ContainsKey(chunk))
                {
                    SpawnRocksInChunk(chunk);
                }
            }
        }
        
        // Despawn distant rocks
        CleanupDistantChunks();
    }
    
    /// <summary>
    /// Spawn rocks in a specific chunk with seeded randomness for consistency
    /// </summary>
    private void SpawnRocksInChunk(Vector2Int chunk)
    {
        // Use chunk coordinates as seed for consistent generation
        Random.InitState(chunk.x * 73856 + chunk.y * 19349);
        
        List<GameObject> rocks = new List<GameObject>();
        
        for (int i = 0; i < rocksPerChunk; i++)
        {
            // Random position within chunk
            Vector3 position = new Vector3(
                chunk.x * chunkSize + Random.Range(0f, chunkSize),
                chunk.y * chunkSize + Random.Range(0f, chunkSize),
                0f
            );
            
            // Get or create rock
            GameObject rock = GetRockFromPool();
            rock.transform.position = position;
            rock.name = $"Rock_{chunk.x}_{chunk.y}_{i}";
            
            RockObstacle rockComponent = rock.GetComponent<RockObstacle>();
            if (rockComponent != null)
            {
                rockComponent.Initialize(rockSize);
            }
            
            rock.SetActive(true);
            rocks.Add(rock);
        }
        
        chunkRocks[chunk] = rocks;
        DebugLog.Verbose($"[RockSpawner] Spawned {rocksPerChunk} rocks in chunk {chunk}");
        
        // Restore random state
        Random.InitState((int)(Time.time * 1000));
    }
    
    /// <summary>
    /// Remove chunks that are too far from player
    /// </summary>
    private void CleanupDistantChunks()
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        
        foreach (var kvp in chunkRocks)
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
                // Return rocks to pool
                foreach (GameObject rock in kvp.Value)
                {
                    if (rock != null)
                    {
                        ReturnRockToPool(rock);
                    }
                }
                chunksToRemove.Add(chunk);
            }
        }
        
        // Remove chunks from dictionary
        foreach (Vector2Int chunk in chunksToRemove)
        {
            chunkRocks.Remove(chunk);
            DebugLog.Verbose($"[RockSpawner] Despawned chunk {chunk}");
        }
    }
    
    /// <summary>
    /// Get rock from pool or create new one
    /// </summary>
    private GameObject GetRockFromPool()
    {
        if (rockPool.Count > 0)
        {
            GameObject rock = rockPool.Dequeue();
            return rock;
        }
        else
        {
            GameObject rock = new GameObject("Rock");
            rock.transform.SetParent(transform);
            rock.AddComponent<RockObstacle>();
            return rock;
        }
    }
    
    /// <summary>
    /// Return rock to pool for reuse
    /// </summary>
    private void ReturnRockToPool(GameObject rock)
    {
        rock.SetActive(false);
        rockPool.Enqueue(rock);
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
        
        // Draw active chunks
        Gizmos.color = Color.yellow;
        foreach (var kvp in chunkRocks)
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
