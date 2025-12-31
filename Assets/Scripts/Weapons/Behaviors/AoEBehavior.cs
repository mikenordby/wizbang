using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Behavior for AoE weapons (Circle of Fire, Poison Cloud, etc.)
/// Creates area damage effects around the player or target locations.
/// </summary>
public class AoEBehavior : WeaponBehaviorBase
{
    private List<AoEInstance> activeInstances = new List<AoEInstance>();

    // Cached circle sprite to avoid generating texture per AoE instance
    private static Sprite cachedCircleSprite;

    private class AoEInstance
    {
        public Vector3 Position;
        public float Radius;
        public float Damage;
        public float TickRate;
        public float Duration;
        public float TimeAlive;
        public float TimeSinceLastTick;
        public bool FollowPlayer;
        public bool ExpandOverTime;
        public float MaxRadius;
        public DamageType DamageType;
        public GameObject Visual;
    }

    public override void Execute(WeaponContext context)
    {
        var settings = context.Definition.aoeSettings;

        // For permanent, player-following AoE (like Circle of Fire), only create one instance
        if (settings.duration <= 0 && settings.followPlayer)
        {
            // Check if we already have an instance active
            if (activeInstances.Count > 0)
            {
                // Update existing instance with new damage values
                foreach (var existing in activeInstances)
                {
                    existing.Damage = context.CurrentDamage;
                    existing.Radius = settings.radius;
                }
                return; // Don't create new instances
            }
        }

        // Determine spawn position
        Vector3 spawnPos;
        if (settings.spawnAtPlayer)
        {
            spawnPos = context.PlayerTransform.position;
        }
        else
        {
            Enemy nearest = FindNearestEnemy(context.PlayerTransform.position, 15f);
            spawnPos = nearest != null ? nearest.transform.position : context.PlayerTransform.position;
        }

        // Create AoE instance for each projectile count
        for (int i = 0; i < context.CurrentProjectileCount; i++)
        {
            Vector3 offset = Vector3.zero;
            if (i > 0)
            {
                // Spread multiple instances around spawn point
                float angle = (360f / context.CurrentProjectileCount) * i * Mathf.Deg2Rad;
                offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 2f;
            }

            AoEInstance instance = new AoEInstance
            {
                Position = spawnPos + offset,
                Radius = settings.radius,
                Damage = context.CurrentDamage,
                TickRate = settings.tickRate,
                Duration = settings.duration,
                TimeAlive = 0f,
                TimeSinceLastTick = settings.tickRate, // Trigger immediately
                FollowPlayer = settings.followPlayer,
                ExpandOverTime = settings.expandOverTime,
                MaxRadius = settings.maxRadius,
                DamageType = context.Definition.damageType,
                Visual = CreateVisual(spawnPos + offset, settings.radius, context.Definition)
            };

            activeInstances.Add(instance);
        }

        DebugLog.Verbose($"[AoEBehavior] Created {context.CurrentProjectileCount} AoE instance(s)");
    }

    public override void OnUpdate(WeaponContext context)
    {
        float deltaTime = Time.deltaTime;

        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            AoEInstance instance = activeInstances[i];
            instance.TimeAlive += deltaTime;
            instance.TimeSinceLastTick += deltaTime;

            // Check if expired
            if (instance.Duration > 0 && instance.TimeAlive >= instance.Duration)
            {
                CleanupInstance(instance);
                activeInstances.RemoveAt(i);
                continue;
            }

            // Follow player if configured
            if (instance.FollowPlayer && context.PlayerTransform != null)
            {
                instance.Position = context.PlayerTransform.position;
                if (instance.Visual != null)
                {
                    instance.Visual.transform.position = instance.Position;
                }
            }

            // Expand over time if configured
            if (instance.ExpandOverTime)
            {
                float t = instance.TimeAlive / instance.Duration;
                instance.Radius = Mathf.Lerp(instance.Radius, instance.MaxRadius, t * 0.1f);
                UpdateVisualSize(instance);
            }

            // Apply damage tick
            if (instance.TimeSinceLastTick >= instance.TickRate)
            {
                ApplyDamage(instance, context);
                instance.TimeSinceLastTick = 0f;
            }
        }
    }

    private void ApplyDamage(AoEInstance instance, WeaponContext context)
    {
        // Use spatial grid query instead of copying full enemy list (zero allocation)
        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr == null)
        {
            DebugLog.Error("[AoEBehavior] CollisionManager is NULL! AoE damage disabled.");
            return;
        }

        var nearbyEntities = collisionMgr.QueryNearbyEnemies(instance.Position, instance.Radius);

        int hitCount = 0;
        foreach (var entity in nearbyEntities)
        {
            if (!(entity is Enemy enemy) || !enemy.IsActive) continue;

            float distance = Vector3.Distance(instance.Position, enemy.transform.position);
            if (distance <= instance.Radius)
            {
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    var damageContext = new DamageContext
                    {
                        baseDamage = instance.Damage,
                        player = context.Player,
                        enemy = enemy,
                        damageType = instance.DamageType
                    };

                    var result = DamageCalculator.Instance.CalculateDamage(damageContext);
                    health.TakeDamage(result.finalDamage);
                    hitCount++;

                    // Show damage number
                    var damagePool = GameServices.DamageNumberPool;
                    if (damagePool != null)
                    {
                        if (result.isCritical)
                            damagePool.ShowCriticalDamage(enemy.transform.position, result.finalDamage);
                        else
                            damagePool.ShowDamage(enemy.transform.position, result.finalDamage);
                    }
                }
            }
        }

        if (hitCount > 0)
        {
            DebugLog.Verbose($"[AoEBehavior] Hit {hitCount} enemies for {instance.Damage:F1} damage each");
        }
    }

    private GameObject CreateVisual(Vector3 position, float radius, WeaponDefinition definition)
    {
        // Create simple circle visual for AoE
        GameObject visual = new GameObject("AoE_Visual");
        visual.transform.position = position;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateCircleSprite();
        sr.color = DamageTypeColors.GetColor(definition.damageType, 0.8f); // Increased from 0.6 for better visibility
        sr.sortingOrder = -1;

        visual.transform.localScale = Vector3.one * radius * 2f;

        return visual;
    }

    /// <summary>
    /// Get cached circle sprite or create one if not exists.
    /// Eliminates texture generation per AoE instance (~4KB allocation each).
    /// </summary>
    private static Sprite GetOrCreateCircleSprite()
    {
        if (cachedCircleSprite != null)
            return cachedCircleSprite;

        // Create a simple circular texture (only done once)
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha * 0.7f); // Increased from 0.5 for better visibility
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return cachedCircleSprite;
    }

    private void UpdateVisualSize(AoEInstance instance)
    {
        if (instance.Visual != null)
        {
            instance.Visual.transform.localScale = Vector3.one * instance.Radius * 2f;
        }
    }

    private void CleanupInstance(AoEInstance instance)
    {
        if (instance.Visual != null)
        {
            Object.Destroy(instance.Visual);
        }
    }

    public override void Cleanup()
    {
        foreach (var instance in activeInstances)
        {
            CleanupInstance(instance);
        }
        activeInstances.Clear();
        base.Cleanup();
    }
}
