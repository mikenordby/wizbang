using UnityEngine;

/// <summary>
/// Object pool for XP orbs
/// </summary>
public class XPOrbPool : ObjectPool<XPOrb>
{
    private Transform playerTransform;
    
    protected override void Awake()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        
        // Call base to create pool
        base.Awake();
    }
    
    /// <summary>
    /// Spawn an XP orb at position
    /// </summary>
    public void SpawnOrb(Vector3 position, int xpAmount)
    {
        DebugLog.Verbose($"XPOrbPool.SpawnOrb: Called with pos={position}, xp={xpAmount}, player={playerTransform != null}");

        // Check if pool is full (all items active)
        bool poolFull = GetActiveCount() >= maxPoolSize;

        if (poolFull)
        {
            // Pool is full - merge XP into nearby orb instead of spawning
            XPOrb nearbyOrb = FindNearbyOrb(position, 3f); // Search within 3 units
            if (nearbyOrb != null)
            {
                nearbyOrb.MergeXP(xpAmount);
                DebugLog.Info($"[XPOrbPool] Pool full ({GetActiveCount()}/{maxPoolSize}), merged {xpAmount} XP into nearby orb");
                return;
            }
            else
            {
                DebugLog.Warning($"[XPOrbPool] Pool full and no nearby orb found - XP will be lost!");
                // Could optionally still spawn and reuse oldest orb as fallback
            }
        }

        // Get inactive orb from pool
        XPOrb orb = GetItem();

        // Activate orb
        if (playerTransform != null && orb != null)
        {
            orb.Activate(position, xpAmount, playerTransform);
            DebugLog.Verbose($"XPOrbPool.SpawnOrb: Activated orb at {position}");
        }
        else
        {
            DebugLog.Warning($"XPOrbPool.SpawnOrb: Cannot activate - player={playerTransform != null}, orb={orb != null}");
        }
    }

    /// <summary>
    /// Find a nearby active XP orb within the given radius
    /// </summary>
    private XPOrb FindNearbyOrb(Vector3 position, float radius)
    {
        float closestDistance = float.MaxValue;
        XPOrb closestOrb = null;

        foreach (XPOrb orb in activeItems)
        {
            if (orb == null || !orb.IsActive()) continue;

            float distance = Vector3.Distance(position, orb.transform.position);
            if (distance <= radius && distance < closestDistance)
            {
                closestDistance = distance;
                closestOrb = orb;
            }
        }

        return closestOrb;
    }
    
    protected override XPOrb CreateNewItem()
    {
        GameObject orbObj = new GameObject($"XPOrb_{pool.Count}");
        orbObj.transform.SetParent(transform);
        
        XPOrb orb = orbObj.AddComponent<XPOrb>();
        orb.Deactivate();
        
        pool.Add(orb);
        return orb;
    }
    
    protected override bool IsActive(XPOrb item)
    {
        return item.IsActive();
    }
}
