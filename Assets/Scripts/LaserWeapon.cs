using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Laser weapon that fires a continuous piercing beam.
/// Deals damage to all enemies in a line.
/// </summary>
public class LaserWeapon : Weapon
{
    [Header("Laser Properties")]
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float laserWidth = 0.3f;
    [SerializeField] private float beamDuration = 0.3f;
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second when rotating
    [SerializeField] private bool autoRotate = false;
    
    private float currentAngle = 0f;
    private EnemyPool enemyPool;
    private int shotCounter = 0; // Unique ID per shot
    private Dictionary<int, HashSet<int>> shotHitTracking = new Dictionary<int, HashSet<int>>(); // shotID -> enemy IDs
    
    protected override void Start()
    {
        base.Start();
        weaponName = "Piercing Laser";
        baseDamage = 12f;
        baseFireRate = 2f;
        basePierce = 999; // Infinite pierce
        damageType = DamageType.Fire;
        
        enemyPool = FindFirstObjectByType<EnemyPool>();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Auto-rotate laser
        if (autoRotate)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;
        }
    }
    
    protected override void Fire()
    {
        if (enemyPool == null) return;
        
        // Determine laser direction
        Vector3 direction;
        if (autoRotate)
        {
            direction = Quaternion.Euler(0, 0, currentAngle) * Vector3.right;
        }
        else
        {
            // Aim at nearest enemy
            Enemy nearest = FindNearestEnemy();
            if (nearest != null)
                direction = (nearest.transform.position - playerTransform.position).normalized;
            else
                direction = Vector3.right;
        }
        
        FireLaser(playerTransform.position, direction);
    }
    
    private void FireLaser(Vector3 origin, Vector3 direction)
    {
        // Generate unique shot ID
        int shotID = shotCounter++;
        HashSet<int> hitEnemies = new HashSet<int>();
        shotHitTracking[shotID] = hitEnemies;
        
        // Create laser visual
        DrawLaserBeam(origin, direction, shotID);
        
        // Damage all enemies in line
        if (enemyPool == null) return;
        
        // Create snapshot to avoid collection modification during iteration
        var enemies = new System.Collections.Generic.List<Enemy>(enemyPool.GetActiveEnemies());
        
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsActive) continue;
            
            // Check if enemy intersects laser line
            Vector3 toEnemy = enemy.transform.position - origin;
            float projectionLength = Vector3.Dot(toEnemy, direction);
            
            // Enemy must be in front of origin
            if (projectionLength < 0 || projectionLength > laserLength)
                continue;
            
            // Calculate perpendicular distance from laser line
            Vector3 projection = origin + direction * projectionLength;
            float perpDistance = Vector3.Distance(enemy.transform.position, projection);
            
            if (perpDistance <= laserWidth + enemy.CollisionRadius)
            {
                int enemyID = enemy.GetInstanceID();
                if (hitEnemies.Contains(enemyID)) continue;
                
                // Apply damage
                var context = new DamageContext
                {
                    baseDamage = currentDamage,
                    player = player,
                    enemy = enemy,
                    damageType = damageType
                };
                var result = DamageCalculator.Instance.CalculateDamage(context);
                
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(result.finalDamage);
                    
                    if (result.isCritical)
                        DamageNumberPool.Instance?.ShowCriticalDamage(enemy.transform.position, result.finalDamage);
                    else
                        DamageNumberPool.Instance?.ShowDamage(enemy.transform.position, result.finalDamage);
                }
                
                hitEnemies.Add(enemyID);
            }
        }
    }
    
    private void DrawLaserBeam(Vector3 origin, Vector3 direction, int shotID)
    {
        GameObject beamObj = new GameObject($"LaserBeam_{shotID}");
        
        // Add cleanup component to remove hit tracking when beam is destroyed
        var cleanup = beamObj.AddComponent<LaserCleanup>();
        cleanup.Initialize(this, shotID, beamDuration);
        
        LineRenderer line = beamObj.AddComponent<LineRenderer>();
        
        line.startWidth = laserWidth;
        line.endWidth = laserWidth * 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1f, 0.2f, 0.2f, 1f); // Bright red
        line.endColor = new Color(1f, 0.5f, 0f, 0.5f); // Orange fade
        line.positionCount = 2;
        line.sortingOrder = 5;
        
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction * laserLength);
        
        // Add glow effect with second line
        GameObject glowObj = new GameObject("LaserGlow");
        glowObj.transform.SetParent(beamObj.transform);
        LineRenderer glow = glowObj.AddComponent<LineRenderer>();
        
        glow.startWidth = laserWidth * 3f;
        glow.endWidth = laserWidth * 1.5f;
        glow.material = new Material(Shader.Find("Sprites/Default"));
        glow.startColor = new Color(1f, 0.8f, 0.2f, 0.3f);
        glow.endColor = new Color(1f, 0.3f, 0f, 0.1f);
        glow.positionCount = 2;
        glow.sortingOrder = 4;
        
        glow.SetPosition(0, origin);
        glow.SetPosition(1, origin + direction * laserLength);
        
        Destroy(beamObj, beamDuration);
    }
    
    /// <summary>
    /// Called by LaserCleanup when beam is destroyed
    /// </summary>
    public void CleanupShot(int shotID)
    {
        shotHitTracking.Remove(shotID);
    }
    
    private Enemy FindNearestEnemy()
    {
        if (enemyPool == null) return null;
        
        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (var enemy in enemyPool.GetActiveEnemies())
        {
            if (enemy == null || !enemy.IsActive) continue;
            
            float dist = Vector3.Distance(playerTransform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearest = enemy;
                nearestDist = dist;
            }
        }
        
        return nearest;
    }
    
    protected override void ApplyLevelUpgrades()
    {
        base.ApplyLevelUpgrades();
        
        // Level scaling
        switch (weaponLevel)
        {
            case 2: laserLength = 12f; break;
            case 3: baseFireRate = 2.5f; break;
            case 4: laserWidth = 0.4f; break;
            case 5: laserLength = 15f; break;
            case 6: autoRotate = true; rotationSpeed = 60f; break;
            case 7: laserWidth = 0.5f; break;
            case 8: rotationSpeed = 90f; projectileCount = 2; break; // Dual lasers
        }
    }
}
