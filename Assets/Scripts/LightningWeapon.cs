using UnityEngine;

/// <summary>
/// Lightning weapon that chains between nearby enemies.
/// Each chain reduces damage and has a maximum chain count.
/// </summary>
public class LightningWeapon : Weapon
{
    [Header("Lightning Properties")]
    [SerializeField] private float chainRange = 3f;
    [SerializeField] private int maxChains = 3;
    [SerializeField] private int chainsPerLevel = 1; // Add 1 extra chain per level
    [SerializeField] private float chainDamageMultiplier = 0.7f; // Each chain does 70% of previous
    [SerializeField] private float lightningDuration = 0.2f;
    [SerializeField] private float targetingRange = 8f;
    
    private EnemyPool enemyPool;
    
    protected override void Start()
    {
        base.Start();
        weaponName = "Chain Lightning";
        baseDamage = 15f;
        baseFireRate = 0.8f; // Slower fire rate
        damageType = DamageType.Lightning;
        
        enemyPool = FindFirstObjectByType<EnemyPool>();
    }
    
    protected override void Fire()
    {
        if (enemyPool == null) return;
        
        // Find nearest enemy
        Enemy firstTarget = FindNearestEnemy(playerTransform.position, targetingRange);
        if (firstTarget == null) return;
        
        // Calculate current max chains based on level (use WeaponLevel property)
        int currentMaxChains = maxChains + (WeaponLevel - 1) * chainsPerLevel;
        
        // Start lightning chain
        ChainLightning(firstTarget, currentDamage, 0, currentMaxChains);
    }
    
    private void ChainLightning(Enemy target, float damage, int chainCount, int maxChainsForThisFire)
    {
        if (target == null || !target.IsActive || chainCount >= maxChainsForThisFire)
            return;
        
        // Apply damage
        var context = new DamageContext
        {
            baseDamage = damage,
            player = player,
            enemy = target,
            damageType = damageType
        };
        var result = DamageCalculator.Instance.CalculateDamage(context);
        
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(result.finalDamage);
            
            // Show damage number
            if (result.isCritical)
                DamageNumberPool.Instance?.ShowCriticalDamage(target.transform.position, result.finalDamage);
            else
                DamageNumberPool.Instance?.ShowDamage(target.transform.position, result.finalDamage);
        }
        
        // Visual effect (lightning bolt)
        DrawLightningBolt(chainCount == 0 ? playerTransform.position : target.transform.position, 
                          target.transform.position);
        
        // Find next target in chain
        if (chainCount < maxChainsForThisFire - 1)
        {
            Enemy nextTarget = FindNearestEnemy(target.transform.position, chainRange, target);
            if (nextTarget != null)
            {
                float nextDamage = damage * chainDamageMultiplier;
                ChainLightning(nextTarget, nextDamage, chainCount + 1, maxChainsForThisFire);
            }
        }
    }
    
    private Enemy FindNearestEnemy(Vector3 position, float range, Enemy exclude = null)
    {
        if (enemyPool == null) return null;
        
        Enemy nearest = null;
        float nearestDist = range;
        
        // Create snapshot to avoid collection modification during iteration
        var enemies = new System.Collections.Generic.List<Enemy>(enemyPool.GetActiveEnemies());
        
        foreach (var enemy in enemies)
        {
            if (enemy == exclude || !enemy.IsActive) continue;
            
            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearest = enemy;
                nearestDist = dist;
            }
        }
        
        return nearest;
    }
    
    private void DrawLightningBolt(Vector3 start, Vector3 end)
    {
        // Create temporary line renderer for visual effect
        GameObject lineObj = new GameObject("LightningBolt");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        line.startWidth = 0.1f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.5f, 0.8f, 1f, 1f); // Electric blue
        line.endColor = new Color(0.8f, 0.9f, 1f, 0.5f);
        line.positionCount = 2;
        line.sortingOrder = 10;
        
        // Add jagged path
        int segments = 5;
        line.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            
            // Add random offset for lightning effect
            if (i > 0 && i < segments - 1)
            {
                Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.forward);
                point += perpendicular * Random.Range(-0.2f, 0.2f);
            }
            
            line.SetPosition(i, point);
        }
        
        // Destroy after duration
        Destroy(lineObj, lightningDuration);
    }
    
    protected override void ApplyLevelUpgrades()
    {
        base.ApplyLevelUpgrades();
        
        // Level scaling
        switch (weaponLevel)
        {
            case 2: maxChains = 4; break;
            case 3: chainRange = 4f; break;
            case 4: maxChains = 5; break;
            case 5: chainDamageMultiplier = 0.8f; break;
            case 6: chainRange = 5f; break;
            case 7: maxChains = 6; break;
            case 8: chainDamageMultiplier = 0.9f; targetingRange = 10f; break;
        }
    }
}
