using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns treasure chests using chunk-based generation like RockSpawner.
/// ~25% chance per chunk. Chests grant item selection without leveling up.
/// </summary>
public class TreasureChestSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private Transform player;

    [Tooltip("Size of each chunk in world units (should match RockSpawner)")]
    [SerializeField] private int chunkSize = 20;

    [Tooltip("How far from player before despawning")]
    [SerializeField] private float despawnDistance = 49f;

    // 25% chance per chunk - hardcoded to prevent Inspector override
    private const float CHEST_SPAWN_CHANCE = 0.25f;

    // Dictionary stores: chunk -> chest GameObject (null if spawn roll failed)
    private Dictionary<Vector2Int, GameObject> chunkChests = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chestPool = new Queue<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null && GameServices.Player != null)
            {
                player = GameServices.Player.transform;
            }
        }

        DebugLog.Info($"[TreasureChestSpawner] Initialized with {CHEST_SPAWN_CHANCE * 100f}% spawn chance per {chunkSize}x{chunkSize} chunk");
    }

    private void Update()
    {
        // Only spawn during gameplay
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;

        // Try to find player if still null
        if (player == null)
        {
            if (GameServices.Player != null)
            {
                player = GameServices.Player.transform;
            }
            else
            {
                return;
            }
        }

        Vector2Int playerChunk = GetChunkCoord(player.position);

        // Spawn chests in nearby chunks (3x3 grid around player)
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
        // Different seed multiplier than rocks so chests don't overlap rocks
        Random.InitState(chunk.x * 91237 + chunk.y * 48271);

        // Roll for chest spawn
        bool shouldSpawn = Random.value < CHEST_SPAWN_CHANCE;

        if (shouldSpawn)
        {
            // Random position within chunk (stay away from edges)
            Vector3 position = new Vector3(
                chunk.x * chunkSize + Random.Range(chunkSize * 0.2f, chunkSize * 0.8f),
                chunk.y * chunkSize + Random.Range(chunkSize * 0.2f, chunkSize * 0.8f),
                0f
            );

            // Get or create chest
            GameObject chest = GetChestFromPool();
            chest.transform.position = position;
            chest.name = $"TreasureChest_{chunk.x}_{chunk.y}";
            chest.SetActive(true);

            chunkChests[chunk] = chest;
            DebugLog.Info($"[TreasureChestSpawner] Spawned chest in chunk {chunk} at {position}");
        }
        else
        {
            // Mark chunk as checked but no chest spawned
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
            // Make sure it has the component
            if (chest.GetComponent<TreasureChest>() == null)
            {
                chest.AddComponent<TreasureChest>();
            }
            return chest;
        }
        else
        {
            GameObject chest = new GameObject("TreasureChest");
            chest.transform.SetParent(transform);
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
}
