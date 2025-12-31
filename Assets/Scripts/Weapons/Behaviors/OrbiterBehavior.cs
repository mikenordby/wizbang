using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Behavior for orbiter weapons (Orbiting Blades).
/// Creates projectiles that orbit the player and damage enemies on contact.
/// Uses the unified Projectile system with orbital movement.
/// </summary>
public class OrbiterBehavior : WeaponBehaviorBase, IWeaponCollisionHandler
{
    private List<OrbiterInstance> activeOrbiters = new List<OrbiterInstance>();
    private int targetOrbiterCount = 0;
    private bool registeredWithCollisionManager = false;

    private class OrbiterInstance
    {
        public GameObject Visual;
        public float Angle;
        public float Damage;
        public float CollisionRadius;
        public Dictionary<int, float> HitCooldowns;
        public float HitCooldownDuration;

        public Vector3 Position => Visual != null ? Visual.transform.position : Vector3.zero;
        public bool IsActive => Visual != null && Visual.activeInHierarchy;

        public OrbiterInstance()
        {
            HitCooldowns = new Dictionary<int, float>();
        }

        public bool CanHitEnemy(int enemyId)
        {
            if (!HitCooldowns.ContainsKey(enemyId))
                return true;
            return Time.time >= HitCooldowns[enemyId];
        }

        public void RegisterHit(int enemyId)
        {
            HitCooldowns[enemyId] = Time.time + HitCooldownDuration;
        }

        public void CleanupExpiredCooldowns()
        {
            List<int> expired = new List<int>();
            foreach (var kvp in HitCooldowns)
            {
                if (Time.time >= kvp.Value)
                    expired.Add(kvp.Key);
            }
            foreach (int id in expired)
            {
                HitCooldowns.Remove(id);
            }
        }
    }

    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        targetOrbiterCount = context.CurrentProjectileCount;
        UpdateOrbiterCount(context);
        RegisterWithCollisionManager();
    }

    private void RegisterWithCollisionManager()
    {
        if (registeredWithCollisionManager) return;

        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr != null)
        {
            collisionMgr.RegisterWeapon(this);
            registeredWithCollisionManager = true;
            DebugLog.Info("[OrbiterBehavior] Registered with CollisionManager");
        }
    }

    public override void Execute(WeaponContext context)
    {
        // Orbiters don't fire - they're continuous
        // Just ensure count is correct
        if (targetOrbiterCount != context.CurrentProjectileCount)
        {
            targetOrbiterCount = context.CurrentProjectileCount;
            UpdateOrbiterCount(context);
        }
    }

    public override void OnUpdate(WeaponContext context)
    {
        if (context.PlayerTransform == null) return;

        // Retry registration if it failed during Initialize (GameServices might not have been ready)
        if (!registeredWithCollisionManager)
        {
            RegisterWithCollisionManager();
        }

        var settings = context.Definition.orbiterSettings;
        float deltaTime = Time.deltaTime;
        float radius = settings.orbitRadius * context.CurrentRange;
        float speed = settings.orbitSpeed + (context.CurrentFireRate * 0.5f);

        for (int i = 0; i < activeOrbiters.Count; i++)
        {
            var orbiter = activeOrbiters[i];
            if (!orbiter.IsActive) continue;

            // Update angle
            orbiter.Angle += speed * deltaTime;
            if (orbiter.Angle >= Mathf.PI * 2f)
                orbiter.Angle -= Mathf.PI * 2f;

            // Update position
            float x = context.PlayerTransform.position.x + Mathf.Cos(orbiter.Angle) * radius;
            float y = context.PlayerTransform.position.y + Mathf.Sin(orbiter.Angle) * radius;
            orbiter.Visual.transform.position = new Vector3(x, y, 0f);

            // Update stats
            orbiter.Damage = context.CurrentDamage;
            orbiter.HitCooldownDuration = settings.hitCooldown > 0 ? 1f / settings.hitCooldown : 1f;
            orbiter.CollisionRadius = settings.collisionRadius * context.CurrentProjectileSize;

            // Cleanup expired cooldowns periodically
            if (Time.frameCount % 60 == 0)
            {
                orbiter.CleanupExpiredCooldowns();
            }
        }
    }

    public override void OnStatsChanged(WeaponContext context)
    {
        base.OnStatsChanged(context);

        if (context.CurrentProjectileCount != targetOrbiterCount)
        {
            targetOrbiterCount = context.CurrentProjectileCount;
            UpdateOrbiterCount(context);
        }

        // Update visual sizes
        float sizeMultiplier = context.Player != null ? context.Player.ProjectileSizeMultiplier : 1f;
        float finalSize = context.CurrentProjectileSize * sizeMultiplier;

        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter.Visual != null)
            {
                orbiter.Visual.transform.localScale = Vector3.one * finalSize;
            }
        }
    }

    private void UpdateOrbiterCount(WeaponContext context)
    {
        // Remove excess orbiters
        while (activeOrbiters.Count > targetOrbiterCount)
        {
            int lastIndex = activeOrbiters.Count - 1;
            if (activeOrbiters[lastIndex].Visual != null)
            {
                Object.Destroy(activeOrbiters[lastIndex].Visual);
            }
            activeOrbiters.RemoveAt(lastIndex);
        }

        // Add new orbiters
        while (activeOrbiters.Count < targetOrbiterCount)
        {
            float startAngle = (Mathf.PI * 2f / targetOrbiterCount) * activeOrbiters.Count;
            OrbiterInstance orbiter = CreateOrbiter(context, startAngle);
            activeOrbiters.Add(orbiter);
        }

        // Redistribute angles evenly
        for (int i = 0; i < activeOrbiters.Count; i++)
        {
            activeOrbiters[i].Angle = (Mathf.PI * 2f / activeOrbiters.Count) * i;
        }

        DebugLog.Info($"[OrbiterBehavior] Updated orbiter count to {activeOrbiters.Count}");
    }

    private OrbiterInstance CreateOrbiter(WeaponContext context, float startAngle)
    {
        var settings = context.Definition.orbiterSettings;

        GameObject visual = new GameObject("Orbiter");
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = context.Definition.visuals.sprite ?? SpriteLoader.LoadProjectileSprite("blade");
        sr.color = context.Definition.visuals.color;
        sr.sortingOrder = 5;

        float sizeMultiplier = context.Player != null ? context.Player.ProjectileSizeMultiplier : 1f;
        visual.transform.localScale = Vector3.one * context.CurrentProjectileSize * sizeMultiplier;

        return new OrbiterInstance
        {
            Visual = visual,
            Angle = startAngle,
            Damage = context.CurrentDamage,
            CollisionRadius = settings.collisionRadius * context.CurrentProjectileSize,
            HitCooldownDuration = settings.hitCooldown > 0 ? 1f / settings.hitCooldown : 1f
        };
    }

    #region IWeaponCollisionHandler

    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        if (grid == null || enemyPool == null || cachedContext == null)
        {
            if (Time.frameCount % 60 == 0)
                DebugLog.Warning($"[OrbiterBehavior] CheckCollisions: grid={grid != null}, enemyPool={enemyPool != null}, cachedContext={cachedContext != null}");
            return;
        }

        int totalHits = 0;
        foreach (var orbiter in activeOrbiters)
        {
            if (!orbiter.IsActive) continue;

            var nearbyEntities = grid.Query(
                orbiter.Position,
                orbiter.CollisionRadius,
                CollisionLayer.Enemy
            );

            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    int enemyId = enemy.GetInstanceID();
                    if (!orbiter.CanHitEnemy(enemyId))
                        continue;

                    float distance = Vector3.Distance(orbiter.Position, enemy.Position);
                    float combinedRadius = orbiter.CollisionRadius + enemy.CollisionRadius;

                    if (distance < combinedRadius)
                    {
                        ApplyOrbiterDamage(orbiter, enemy);
                        orbiter.RegisterHit(enemyId);
                        totalHits++;
                    }
                }
            }
        }

        if (Time.frameCount % 60 == 0)
        {
            DebugLog.Info($"[OrbiterBehavior] CheckCollisions: {activeOrbiters.Count} orbiters, {totalHits} hits this frame");
        }
    }

    private void ApplyOrbiterDamage(OrbiterInstance orbiter, Enemy enemy)
    {
        Health health = enemy.GetComponent<Health>();
        if (health == null) return;

        var damageContext = new DamageContext
        {
            baseDamage = orbiter.Damage,
            player = cachedContext.Player,
            enemy = enemy,
            damageType = cachedContext.Definition?.damageType ?? DamageType.Physical
        };

        var result = DamageCalculator.Instance.CalculateDamage(damageContext);
        health.TakeDamage(result.finalDamage);

        // Lifesteal
        if (cachedContext.Player != null && cachedContext.Player.LifestealChance > 0f)
        {
            if (Random.value < cachedContext.Player.LifestealChance)
            {
                Health playerHealth = cachedContext.Player.GetComponent<Health>();
                if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                {
                    playerHealth.Heal(1f);
                }
            }
        }

        // Show damage number
        var damagePool = GameServices.DamageNumberPool;
        if (damagePool != null)
        {
            if (result.isCritical)
                damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
            else
                damagePool.ShowDamage(enemy.Position, result.finalDamage);
        }
    }

    bool IWeaponCollisionHandler.IsActive => activeOrbiters.Count > 0;

    #endregion

    public override void Cleanup()
    {
        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter.Visual != null)
            {
                Object.Destroy(orbiter.Visual);
            }
        }
        activeOrbiters.Clear();
        base.Cleanup();
    }
}
