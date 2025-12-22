using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Circle of fire weapon that creates a damaging ring around the player.
/// Deals tick damage to all enemies within the ring radius.
/// Tick rate scales with weapon fire rate (attack speed).
/// Implements IWeaponCollisionHandler for self-managed collision detection.
/// </summary>
public class FireRingWeapon : Weapon, IWeaponCollisionHandler
{
    [Header("Fire Ring Settings")]
    [SerializeField] private float ringRadius = 4f;
    [SerializeField] private GameObject ringVisual;
    
    private HashSet<Enemy> enemiesInRing = new HashSet<Enemy>();
    private float tickTimer = 0f;
    private CollisionManager collisionManager;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake()
        weaponName = "Circle of Fire";
        baseDamage = 8f; // Damage per tick
        baseFireRate = 1f; // Ticks per second
        projectileCount = 1; // Not used, but required
        basePierce = 0;
        baseRange = 1f; // Could affect radius
        projectileSize = 1f; // Could affect radius
        damageType = DamageType.Fire; // Applies burning

        // Set weapon tags for synergy system
        weaponTags = new List<WeaponTag> { WeaponTag.Area, WeaponTag.Fire };

        base.Awake();
        
        collisionManager = GameServices.CollisionManager;
        
        RegisterWithCollisionManager();
        
        CreateRingVisual();
        
        DebugLog.Info("[FireRingWeapon] Initialized - Circle of Fire!");
    }
    
    private void CreateRingVisual()
    {
        // Create visual ring GameObject
        ringVisual = new GameObject("FireRing");
        ringVisual.transform.SetParent(playerTransform);
        ringVisual.transform.localPosition = Vector3.zero;
        
        // Add sprite renderer for ring visual
        SpriteRenderer sr = ringVisual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -1; // Behind player
        
        // Create circular sprite for ring
        Texture2D texture = new Texture2D(256, 256);
        Color[] pixels = new Color[256 * 256];
        
        Vector2 center = new Vector2(128, 128);
        float radiusPixels = 120f;
        
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                // Fill entire circle with gradient
                if (distance <= radiusPixels)
                {
                    // Center is bright, fades toward edge
                    float intensity = 1f - (distance / radiusPixels) * 0.5f;
                    float alpha = intensity * 0.3f; // Semi-transparent
                    pixels[y * 256 + x] = new Color(1f, 0.5f, 0f, alpha); // Orange fire
                }
                else
                {
                    pixels[y * 256 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite ringSprite = Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 32);
        sr.sprite = ringSprite;
        
        // Scale to match ring radius (4 world units = 128 pixels at 32 PPU)
        float visualScale = (ringRadius * 2f) / (256f / 32f); // radius * 2 for diameter, divided by sprite world size
        ringVisual.transform.localScale = Vector3.one * visualScale;
        
        DebugLog.Info($"[FireRingWeapon] Created ring visual: radius={ringRadius}, scale={visualScale}");
    }
    
    protected override void Fire()
    {
        // Fire ring is always active, handled in Update via tick timer
    }
    
    protected override void Update()
    {
        base.Update(); // Handles fire rate timing
        
        // ONLY tick damage during gameplay phase
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;
        
        tickTimer += Time.deltaTime;
        float tickRate = currentFireRate; // Ticks per second based on fire rate
        
        if (tickTimer >= 1f / tickRate)
        {
            tickTimer = 0f;
            ApplyRingDamage();
        }
    }
    
    private void ApplyRingDamage()
    {
        if (collisionManager == null || playerTransform == null) return;
        
        // Ring radius scales with range AND projectile size upgrades
        float effectiveRadius = ringRadius * currentRange * currentProjectileSize;
        
        // Use spatial hash grid to efficiently query only nearby enemies
        var nearbyEntities = collisionManager.QueryNearbyEnemies(playerTransform.position, effectiveRadius);
        
        int enemiesHit = 0;
        foreach (var entity in nearbyEntities)
        {
            if (entity is Enemy enemy && enemy.IsActive)
            {
                // Spatial grid returns entities in nearby cells, so verify exact distance
                float distance = Vector2.Distance(playerTransform.position, enemy.transform.position);
                
                if (distance <= effectiveRadius)
                {
                    // Apply damage through DamageCalculator
                    DamageContext context = new DamageContext
                    {
                        baseDamage = currentDamage,
                        player = player,
                        enemy = enemy,
                        damageType = DamageType.Fire
                    };
                    
                    DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                    
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(result.finalDamage);
                        enemiesHit++;
                        
                        // Show damage number
                        DamageNumberPool damagePool = GameServices.DamageNumberPool;
                        if (damagePool != null)
                        {
                            if (result.isCritical)
                                damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                            else
                                damagePool.ShowDamage(enemy.Position, result.finalDamage);
                        }
                    }
                }
            }
        }
        
        if (enemiesHit > 0)
        {
            DebugLog.Verbose($"[FireRingWeapon] Tick hit {enemiesHit} enemies for {currentDamage:F1} damage (rate={currentFireRate:F1} ticks/sec)");
        }
    }
    
    public override void RecalculateStats()
    {
        base.RecalculateStats();

        // Ring radius scales with range AND projectile size upgrades
        float effectiveRadius = ringRadius * currentRange * currentProjectileSize;
        
        // Update visual scale
        if (ringVisual != null)
        {
            float visualScale = (effectiveRadius * 2f) / (256f / 32f);
            ringVisual.transform.localScale = Vector3.one * visualScale;
        }
        
        DebugLog.Info($"[FireRingWeapon] Stats: damage={currentDamage:F1}/tick, tickRate={currentFireRate:F1}/sec, radius={effectiveRadius:F1} (base={ringRadius}, range={currentRange:F2}, size={currentProjectileSize:F2})");
    }
    
    private void OnDestroy()
    {
        if (ringVisual != null)
        {
            Destroy(ringVisual);
        }
    }
    
    #region IWeaponCollisionHandler Implementation
    
    /// <summary>
    /// Fire ring handles its own damage application in Update() via tick system.
    /// No additional collision detection needed here.
    /// </summary>
    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        // No-op: Fire ring damage is tick-based, handled in Update()
    }
    
    /// <summary>
    /// Whether this weapon is active and should check collisions
    /// </summary>
    bool IWeaponCollisionHandler.IsActive => false; // Disable, uses tick-based damage in Update()
    
    #endregion
}
