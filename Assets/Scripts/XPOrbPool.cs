using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for XP orbs
/// </summary>
public class XPOrbPool : MonoBehaviour
{
    [SerializeField] private int poolSize = 50;
    private List<XPOrb> orbPool = new List<XPOrb>();
    private Transform playerTransform;
    
    void Awake()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        
        // Create pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewOrb();
        }
        
        Debug.Log($"XPOrbPool.Awake: Created pool of {poolSize} orbs");
    }
    
    private void CreateNewOrb()
    {
        GameObject orbObj = new GameObject($"XPOrb_{orbPool.Count}");
        orbObj.transform.SetParent(transform);
        
        XPOrb orb = orbObj.AddComponent<XPOrb>();
        orb.Deactivate();
        
        orbPool.Add(orb);
    }
    
    /// <summary>
    /// Spawn an XP orb at position
    /// </summary>
    public void SpawnOrb(Vector3 position, int xpAmount)
    {
        Debug.Log($"XPOrbPool.SpawnOrb: Called with pos={position}, xp={xpAmount}, player={playerTransform != null}");
        
        // Find inactive orb
        XPOrb orb = null;
        foreach (var o in orbPool)
        {
            if (!o.IsActive())
            {
                orb = o;
                break;
            }
        }
        
        // If all orbs active, expand pool
        if (orb == null)
        {
            CreateNewOrb();
            orb = orbPool[orbPool.Count - 1];
            Debug.Log($"XPOrbPool.SpawnOrb: Expanded pool to {orbPool.Count}");
        }
        
        // Activate orb
        if (playerTransform != null && orb != null)
        {
            orb.Activate(position, xpAmount, playerTransform);
            Debug.Log($"XPOrbPool.SpawnOrb: Activated orb at {position}");
        }
        else
        {
            Debug.LogWarning($"XPOrbPool.SpawnOrb: Cannot activate - player={playerTransform != null}, orb={orb != null}");
        }
    }
    
    /// <summary>
    /// Get count of active orbs
    /// </summary>
    public int GetActiveCount()
    {
        int count = 0;
        foreach (var orb in orbPool)
        {
            if (orb.IsActive())
                count++;
        }
        return count;
    }
}
