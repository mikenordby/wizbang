using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for damage number popups
/// </summary>
public class DamageNumberPool : MonoBehaviour
{
    [SerializeField] private int poolSize = 20;
    
    private List<DamageNumber> pool = new List<DamageNumber>();
    private int nextIndex = 0;
    
    private void Start()
    {
        // Create pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject($"DamageNumber_{i}");
            obj.transform.parent = transform;
            DamageNumber damageNumber = obj.AddComponent<DamageNumber>();
            obj.SetActive(false);
            pool.Add(damageNumber);
        }
        
        DebugLog.Info($"[DamageNumberPool] Created pool with {poolSize} damage numbers");
    }
    
    /// <summary>
    /// Get a damage number from the pool and show it
    /// </summary>
    public void ShowDamage(Vector3 position, float damage, bool isCrit = false)
    {
        // Get next available damage number (circular buffer)
        DamageNumber damageNumber = pool[nextIndex];
        nextIndex = (nextIndex + 1) % poolSize;
        
        // Show the damage number
        damageNumber.Show(position, damage, isCrit);
    }
    
    /// <summary>
    /// Show a critical hit damage number (gold color, 1.5x size)
    /// </summary>
    public void ShowCriticalDamage(Vector3 position, float damage)
    {
        ShowDamage(position, damage, isCrit: true);
    }
}
