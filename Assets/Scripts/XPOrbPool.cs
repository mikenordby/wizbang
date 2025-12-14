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
