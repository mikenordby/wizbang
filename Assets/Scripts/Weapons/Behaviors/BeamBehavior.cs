using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Behavior for beam weapons (Laser).
/// Creates continuous beam that damages all enemies in its path.
/// </summary>
public class BeamBehavior : WeaponBehaviorBase
{
    private LineRenderer beamRenderer;
    private float currentAngle = 0f;
    private float timeSinceLastTick = 0f;
    private bool isActive = false;
    private HashSet<int> hitEnemiesThisTick = new HashSet<int>();

    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        CreateBeamVisual(context);
        isActive = true;
    }

    public override void Execute(WeaponContext context)
    {
        // Beams are continuous - Execute just ensures we're active
        isActive = true;
        if (beamRenderer != null)
        {
            beamRenderer.enabled = true;
        }
    }

    public override void OnUpdate(WeaponContext context)
    {
        if (!isActive || beamRenderer == null || context.PlayerTransform == null)
            return;

        var settings = context.Definition.beamSettings;
        float deltaTime = Time.deltaTime;

        // Rotation
        if (settings.autoRotate && settings.rotationSpeed > 0)
        {
            currentAngle += settings.rotationSpeed * deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;
        }
        else
        {
            // Aim at nearest enemy within beam length
            Enemy nearest = FindNearestEnemy(context.PlayerTransform.position, settings.beamLength);
            if (nearest != null)
            {
                Vector3 direction = nearest.transform.position - context.PlayerTransform.position;
                currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
        }

        // Update beam visual
        UpdateBeamVisual(context, settings);

        // Damage tick
        timeSinceLastTick += deltaTime;
        if (timeSinceLastTick >= 1f / settings.tickRate)
        {
            ApplyBeamDamage(context, settings);
            timeSinceLastTick = 0f;
        }
    }

    private void CreateBeamVisual(WeaponContext context)
    {
        if (context.WeaponMonoBehaviour == null) return;

        GameObject beamObj = new GameObject("BeamVisual");
        beamObj.transform.SetParent(context.WeaponMonoBehaviour.transform);

        beamRenderer = beamObj.AddComponent<LineRenderer>();
        beamRenderer.startWidth = context.Definition.beamSettings.beamWidth;
        beamRenderer.endWidth = context.Definition.beamSettings.beamWidth * 0.5f;
        beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
        beamRenderer.positionCount = 2;
        beamRenderer.sortingOrder = 5;

        Color beamColor = DamageTypeColors.GetColor(context.Definition.damageType);
        beamRenderer.startColor = beamColor;
        beamRenderer.endColor = DamageTypeColors.GetFadedColor(context.Definition.damageType);
    }

    private void UpdateBeamVisual(WeaponContext context, BeamSettings settings)
    {
        if (beamRenderer == null) return;

        Vector3 start = context.PlayerTransform.position;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 end = start + direction * settings.beamLength * context.CurrentRange;

        beamRenderer.SetPosition(0, start);
        beamRenderer.SetPosition(1, end);

        // Update width based on projectile size multiplier
        float sizeMultiplier = context.Player != null ? context.Player.ProjectileSizeMultiplier : 1f;
        beamRenderer.startWidth = settings.beamWidth * context.CurrentProjectileSize * sizeMultiplier;
        beamRenderer.endWidth = beamRenderer.startWidth * 0.5f;
    }

    private void ApplyBeamDamage(WeaponContext context, BeamSettings settings)
    {
        hitEnemiesThisTick.Clear();

        Vector3 start = context.PlayerTransform.position;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        float beamLength = settings.beamLength * context.CurrentRange;
        float beamWidth = settings.beamWidth * context.CurrentProjectileSize;

        // Use spatial grid query instead of copying full enemy list (zero allocation)
        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr == null)
        {
            DebugLog.Error("[BeamBehavior] CollisionManager is NULL! Beam damage disabled.");
            return;
        }

        // Query enemies along the beam path - use beam length as radius from midpoint
        Vector3 beamMidpoint = start + direction * (beamLength * 0.5f);
        var nearbyEntities = collisionMgr.QueryNearbyEnemies(beamMidpoint, beamLength * 0.5f + beamWidth);

        foreach (var entity in nearbyEntities)
        {
            if (!(entity is Enemy enemy) || !enemy.IsActive) continue;

            // Check if enemy is within beam
            if (IsPointOnBeam(enemy.transform.position, start, direction, beamLength, beamWidth))
            {
                int id = enemy.GetInstanceID();
                if (hitEnemiesThisTick.Contains(id)) continue;
                hitEnemiesThisTick.Add(id);

                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    var damageContext = new DamageContext
                    {
                        baseDamage = context.CurrentDamage,
                        player = context.Player,
                        enemy = enemy,
                        damageType = context.Definition.damageType
                    };

                    var result = DamageCalculator.Instance.CalculateDamage(damageContext);
                    health.TakeDamage(result.finalDamage);

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
    }

    private bool IsPointOnBeam(Vector3 point, Vector3 beamStart, Vector3 beamDirection, float beamLength, float beamWidth)
    {
        // Project point onto beam line
        Vector3 toPoint = point - beamStart;
        float projection = Vector3.Dot(toPoint, beamDirection);

        // Check if within beam length
        if (projection < 0 || projection > beamLength)
            return false;

        // Check perpendicular distance
        Vector3 closestPointOnBeam = beamStart + beamDirection * projection;
        float distance = Vector3.Distance(point, closestPointOnBeam);

        return distance <= beamWidth / 2f;
    }

    public override void OnStatsChanged(WeaponContext context)
    {
        base.OnStatsChanged(context);
        // Update beam color/width when stats change
        if (beamRenderer != null)
        {
            Color beamColor = DamageTypeColors.GetColor(context.Definition.damageType);
            beamRenderer.startColor = beamColor;
            beamRenderer.endColor = DamageTypeColors.GetFadedColor(context.Definition.damageType);
        }
    }

    public override void Cleanup()
    {
        if (beamRenderer != null)
        {
            Object.Destroy(beamRenderer.gameObject);
            beamRenderer = null;
        }
        isActive = false;
        base.Cleanup();
    }
}
