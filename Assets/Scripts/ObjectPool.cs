using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool base class to eliminate duplicate pooling logic.
/// Handles Get/Return/Expand/CleanupInactive operations with cached active lists.
/// </summary>
/// <typeparam name="T">The component type to pool (must derive from MonoBehaviour)</typeparam>
public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected int initialPoolSize = 50;
    [SerializeField] protected int maxPoolSize = 200;
    
    protected List<T> pool;
    protected List<T> activeItems; // Cached active list - eliminates GetComponentsInChildren
    
    protected virtual void Awake()
    {
        pool = new List<T>(maxPoolSize);
        activeItems = new List<T>(maxPoolSize);
        
        // Pre-allocate pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewItem();
        }
        
        DebugLog.Info($"{GetType().Name}: Initialized pool with {initialPoolSize} items");
    }
    
    /// <summary>
    /// Get an inactive item from the pool. If none available, creates new or reuses oldest.
    /// </summary>
    protected T GetItem()
    {
        // Find inactive item
        foreach (var item in pool)
        {
            if (!IsActive(item))
            {
                // Add to active list BEFORE activation
                if (!activeItems.Contains(item))
                    activeItems.Add(item);
                
                DebugLog.Verbose($"{GetType().Name}.GetItem: Reusing pooled item, active={activeItems.Count}/{pool.Count}");
                return item;
            }
        }
        
        // Create new if under max
        if (pool.Count < maxPoolSize)
        {
            DebugLog.Info($"{GetType().Name}.GetItem: Expanding pool to {pool.Count + 1}/{maxPoolSize}");
            return CreateNewItem();
        }
        
        // Pool exhausted, reuse oldest
        DebugLog.Warning($"{GetType().Name}: Pool exhausted at {maxPoolSize}, reusing oldest item");
        return pool[0];
    }
    
    /// <summary>
    /// Return an item to the pool (remove from active list)
    /// </summary>
    public void ReturnItem(T item)
    {
        if (activeItems.Remove(item))
        {
            DebugLog.Verbose($"{GetType().Name}.ReturnItem: Returned item, active={activeItems.Count}");
        }
    }
    
    /// <summary>
    /// Get the cached list of active items (no GC allocations)
    /// </summary>
    public List<T> GetActiveItems() => activeItems;
    
    /// <summary>
    /// Get count of currently active items
    /// </summary>
    public int GetActiveCount() => activeItems.Count;
    
    /// <summary>
    /// Cleanup inactive items from the active list (failsafe for desyncs)
    /// </summary>
    public void CleanupInactiveItems()
    {
        int cleanedCount = 0;
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            if (!IsActive(activeItems[i]))
            {
                activeItems.RemoveAt(i);
                cleanedCount++;
            }
        }
        
        if (cleanedCount > 0)
            DebugLog.Info($"{GetType().Name}.CleanupInactive: Removed {cleanedCount} stale items, active={activeItems.Count}");
    }
    
    /// <summary>
    /// Create a new pooled item. Must instantiate GameObject with component T.
    /// Override to customize creation (sprites, initialization, etc.)
    /// </summary>
    protected abstract T CreateNewItem();
    
    /// <summary>
    /// Check if an item is currently active. Override for custom active logic.
    /// </summary>
    protected abstract bool IsActive(T item);
}
