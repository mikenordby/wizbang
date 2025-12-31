using UnityEngine;

/// <summary>
/// Behavior for projectile-based weapons (Magic Missile, Rapid Fire, etc.)
/// Fires projectiles at targets with configurable aiming and spread.
/// </summary>
public class ProjectileBehavior : WeaponBehaviorBase
{
    private ProjectilePool projectilePool;

    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        projectilePool = GameServices.ProjectilePool;

        if (projectilePool == null)
        {
            DebugLog.Warning("[ProjectileBehavior] ProjectilePool not found");
        }
    }

    // Cached target to avoid duplicate FindNearestEnemy calls
    private Enemy cachedTarget;

    public override void Execute(WeaponContext context)
    {
        if (projectilePool == null || context.PlayerTransform == null)
            return;

        var settings = context.Definition.projectileSettings;

        // Get target direction and cache target for homing projectiles
        cachedTarget = null;
        Vector3? targetDirection = GetTargetDirection(context, settings);

        if (targetDirection == null && !settings.fireWithoutTarget)
        {
            return; // No target and weapon requires target
        }

        Vector3 baseDirection = targetDirection ?? GetRandomDirection();

        // Fire projectiles with spread (uses cachedTarget for homing)
        FireProjectiles(context, settings, baseDirection);
    }

    private Vector3? GetTargetDirection(WeaponContext context, ProjectileSettings settings)
    {
        switch (settings.aimType)
        {
            case AimType.NearestEnemy:
                cachedTarget = FindNearestEnemy(
                    context.PlayerTransform.position,
                    settings.targetingRange
                );
                if (cachedTarget != null)
                {
                    return (cachedTarget.transform.position - context.PlayerTransform.position).normalized;
                }
                return null;

            case AimType.RandomEnemy:
                EnemyPool pool = GameServices.EnemyPool;
                if (pool != null)
                {
                    var enemies = pool.GetActiveEnemies();
                    if (enemies.Count > 0)
                    {
                        cachedTarget = enemies[Random.Range(0, enemies.Count)];
                        return (cachedTarget.transform.position - context.PlayerTransform.position).normalized;
                    }
                }
                return null;

            case AimType.MouseDirection:
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = context.PlayerTransform.position.z;
                return (mouseWorld - context.PlayerTransform.position).normalized;

            case AimType.MovementDirection:
                // Use player's velocity or facing direction
                PlayerMovement movement = context.Player?.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    Vector2 vel = movement.GetCurrentVelocity();
                    if (vel.sqrMagnitude > 0.01f)
                    {
                        return new Vector3(vel.x, vel.y, 0f).normalized;
                    }
                }
                return Vector3.right; // Default to right

            case AimType.Fixed:
                return Vector3.up; // Fixed upward

            default:
                return null;
        }
    }

    private Vector3 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
    }

    private void FireProjectiles(WeaponContext context, ProjectileSettings settings, Vector3 baseDirection)
    {
        int count = context.CurrentProjectileCount;

        for (int i = 0; i < count; i++)
        {
            // Calculate spread angle for this projectile
            float angleOffset = 0f;
            if (count > 1)
            {
                float totalSpread = settings.spreadAngle * (count - 1);
                angleOffset = -totalSpread / 2f + (settings.spreadAngle * i);
            }

            // Rotate direction by angle offset
            Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
            Vector3 direction = rotation * baseDirection;

            // Get projectile from pool
            Projectile projectile = projectilePool.GetProjectile();
            if (projectile == null)
            {
                DebugLog.Warning("[ProjectileBehavior] Failed to get projectile from pool");
                continue;
            }

            // Apply visuals
            ApplyVisuals(projectile, context.Definition.visuals);

            // Set projectile stats
            float finalSize = context.CurrentProjectileSize *
                (context.Player != null ? context.Player.ProjectileSizeMultiplier : 1f);

            projectile.SetStats(
                context.CurrentDamage,
                context.CurrentPierce,
                context.Definition.damageType,
                finalSize
            );

            // Activate with appropriate movement type
            if (settings.homing)
            {
                // Use cached target from GetTargetDirection (avoids duplicate enemy search)
                projectile.ActivateHoming(
                    context.PlayerTransform.position,
                    direction,
                    cachedTarget?.transform,
                    settings.homingStrength
                );
            }
            else
            {
                projectile.ActivateStraight(context.PlayerTransform.position, direction);
            }
        }

        DebugLog.Verbose($"[ProjectileBehavior] Fired {count} projectile(s)");
    }

    private void ApplyVisuals(Projectile projectile, ProjectileVisuals visuals)
    {
        if (visuals == null) return;

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (visuals.sprite != null)
            {
                sr.sprite = visuals.sprite;
            }
            sr.color = visuals.color;
        }

        // Trail could be applied here if projectile has TrailRenderer
        if (visuals.hasTrail)
        {
            TrailRenderer trail = projectile.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.enabled = true;
                trail.startColor = visuals.trailColor;
                trail.endColor = new Color(visuals.trailColor.r, visuals.trailColor.g, visuals.trailColor.b, 0f);
                trail.time = visuals.trailLength;
            }
        }
    }
}
