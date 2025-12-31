using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Behavior for chain weapons (Chain Lightning).
/// Hits initial target then chains to nearby enemies with reducing damage.
/// </summary>
public class ChainBehavior : WeaponBehaviorBase
{
    // Cached material to avoid Shader.Find per chain effect
    private static Material cachedLineMaterial;

    public override void Execute(WeaponContext context)
    {
        var settings = context.Definition.chainSettings;

        // Find initial target
        Enemy firstTarget = FindNearestEnemy(
            context.PlayerTransform.position,
            settings.targetingRange
        );

        if (firstTarget == null)
            return;

        // Start chain from player to first target
        ChainToTarget(
            context.PlayerTransform.position,
            firstTarget,
            context.CurrentDamage,
            0,
            settings.maxChains,
            settings,
            context
        );
    }

    private void ChainToTarget(
        Vector3 sourcePosition,
        Enemy target,
        float damage,
        int chainCount,
        int maxChains,
        ChainSettings settings,
        WeaponContext context)
    {
        if (target == null || !target.IsActive || chainCount >= maxChains)
            return;

        // Apply damage
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            var damageContext = new DamageContext
            {
                baseDamage = damage,
                player = context.Player,
                enemy = target,
                damageType = context.Definition.damageType
            };

            var result = DamageCalculator.Instance.CalculateDamage(damageContext);
            targetHealth.TakeDamage(result.finalDamage);

            // Show damage number
            var damagePool = GameServices.DamageNumberPool;
            if (damagePool != null)
            {
                if (result.isCritical)
                    damagePool.ShowCriticalDamage(target.transform.position, result.finalDamage);
                else
                    damagePool.ShowDamage(target.transform.position, result.finalDamage);
            }
        }

        // Visual effect
        DrawChainEffect(sourcePosition, target.transform.position, settings.effectDuration, context.Definition.damageType);

        // Find next target using spatial grid (more efficient than iterating all enemies)
        if (chainCount < maxChains - 1)
        {
            Enemy nextTarget = FindNearestEnemyWithSpatialGrid(
                target.transform.position,
                settings.chainRange,
                target // exclude current target
            );

            if (nextTarget != null)
            {
                float nextDamage = damage * settings.chainDamageMultiplier;
                ChainToTarget(
                    target.transform.position,
                    nextTarget,
                    nextDamage,
                    chainCount + 1,
                    maxChains,
                    settings,
                    context
                );
            }
        }

        DebugLog.Verbose($"[ChainBehavior] Chain {chainCount + 1}/{maxChains} hit {target.name} for {damage:F1}");
    }

    private void DrawChainEffect(Vector3 start, Vector3 end, float duration, DamageType damageType)
    {
        GameObject lineObj = new GameObject("ChainEffect");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        line.startWidth = 0.1f;
        line.endWidth = 0.05f;
        line.material = GetOrCreateLineMaterial();

        Color chainColor = DamageTypeColors.GetColor(damageType);
        line.startColor = chainColor;
        line.endColor = DamageTypeColors.GetFadedColor(damageType);
        line.sortingOrder = 10;

        // Create jagged lightning path
        int segments = 5;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // Add random offset for lightning effect (except endpoints)
            if (i > 0 && i < segments - 1)
            {
                Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.forward);
                point += perpendicular * Random.Range(-0.2f, 0.2f);
            }

            line.SetPosition(i, point);
        }

        Object.Destroy(lineObj, duration);
    }

    /// <summary>
    /// Get cached line material or create one.
    /// Avoids expensive Shader.Find per chain effect.
    /// </summary>
    private static Material GetOrCreateLineMaterial()
    {
        if (cachedLineMaterial == null)
        {
            cachedLineMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        return cachedLineMaterial;
    }

    /// <summary>
    /// Find nearest enemy using spatial grid query (zero allocation).
    /// </summary>
    private Enemy FindNearestEnemyWithSpatialGrid(Vector3 position, float maxRange, Enemy exclude = null)
    {
        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr == null)
        {
            DebugLog.Error("[ChainBehavior] CollisionManager is NULL! Cannot find chain targets.");
            return null;
        }

        var nearbyEntities = collisionMgr.QueryNearbyEnemies(position, maxRange);

        Enemy nearest = null;
        float nearestDistance = maxRange;

        foreach (var entity in nearbyEntities)
        {
            if (!(entity is Enemy enemy) || !enemy.IsActive) continue;
            if (exclude != null && enemy == exclude) continue;

            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }
}
