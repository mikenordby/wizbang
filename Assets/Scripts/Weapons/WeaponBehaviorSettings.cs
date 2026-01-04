using UnityEngine;

/// <summary>
/// Enum defining the type of weapon behavior.
/// Each type has corresponding settings class.
/// </summary>
public enum WeaponBehaviorType
{
    Projectile,  // Magic Missile, Rapid Fire - fires aimed projectiles
    AoE,         // Circle of Fire, Poison Cloud - area damage
    Chain,       // Chain Lightning - chains between targets
    Beam,        // Laser - continuous beam
    Orbiter      // Orbiting Blades - projectiles that orbit player
}

/// <summary>
/// Targeting behavior for projectile weapons.
/// </summary>
public enum AimType
{
    NearestEnemy,       // Auto-aim at closest enemy
    RandomEnemy,        // Pick random enemy in range
    MouseDirection,     // Fire toward mouse cursor
    MovementDirection,  // Fire in movement direction
    Fixed               // Fixed direction (e.g., always up)
}

/// <summary>
/// Settings for projectile-based weapons (Magic Missile, Rapid Fire, etc.)
/// </summary>
[System.Serializable]
public class ProjectileSettings
{
    [Tooltip("How the weapon targets enemies")]
    public AimType aimType = AimType.NearestEnemy;

    [Tooltip("Degrees of spread between multiple projectiles")]
    [Range(0f, 90f)]
    public float spreadAngle = 15f;

    [Tooltip("Projectile movement speed")]
    public float projectileSpeed = 10f;

    [Tooltip("How long projectile lives (seconds)")]
    public float lifetime = 2f;

    [Tooltip("Whether projectiles track enemies")]
    public bool homing = false;

    [Tooltip("Homing turn strength (higher = tighter tracking)")]
    [Range(0f, 10f)]
    public float homingStrength = 0f;

    [Tooltip("Maximum range for targeting")]
    public float targetingRange = 15f;

    [Tooltip("Fire even when no enemies present")]
    public bool fireWithoutTarget = false;
}

/// <summary>
/// Settings for AoE weapons (Circle of Fire, Poison Cloud, etc.)
/// </summary>
[System.Serializable]
public class AoESettings
{
    [Tooltip("Radius of area effect")]
    public float radius = 2.5f;

    [Tooltip("Seconds between damage ticks")]
    public float tickRate = 0.5f;

    [Tooltip("Duration of effect (seconds)")]
    public float duration = 5f;

    [Tooltip("Whether AoE follows player or stays in place")]
    public bool followPlayer = false;

    [Tooltip("Whether effect expands over time")]
    public bool expandOverTime = false;

    [Tooltip("Maximum radius if expanding")]
    public float maxRadius = 4f;

    [Tooltip("Spawn at player position vs nearest enemy")]
    public bool spawnAtPlayer = true;
}

/// <summary>
/// Settings for chain weapons (Chain Lightning)
/// </summary>
[System.Serializable]
public class ChainSettings
{
    [Tooltip("Maximum number of chain jumps")]
    public int maxChains = 5;

    [Tooltip("Range for finding next chain target")]
    public float chainRange = 4f;

    [Tooltip("Damage multiplier per chain (compounds)")]
    [Range(0.1f, 1f)]
    public float chainDamageMultiplier = 0.75f;

    [Tooltip("Range for finding initial target")]
    public float targetingRange = 9f;

    [Tooltip("Duration of chain visual effect")]
    public float effectDuration = 0.2f;
}

/// <summary>
/// Settings for beam weapons (Laser)
/// </summary>
[System.Serializable]
public class BeamSettings
{
    [Tooltip("Length of the beam")]
    public float beamLength = 13.5f;

    [Tooltip("Width of the beam")]
    public float beamWidth = 0.4f;

    [Tooltip("Rotation speed in degrees/sec (0 = stationary)")]
    public float rotationSpeed = 0f;

    [Tooltip("Whether beam auto-rotates")]
    public bool autoRotate = false;

    [Tooltip("Damage ticks per second")]
    public float tickRate = 10f;
}

/// <summary>
/// Settings for orbiter weapons (Orbiting Blades)
/// </summary>
[System.Serializable]
public class OrbiterSettings
{
    [Tooltip("Distance orbiters orbit from player")]
    public float orbitRadius = 2f;

    [Tooltip("Orbit speed in radians/second")]
    public float orbitSpeed = 8f;

    [Tooltip("Seconds before same enemy can be hit again")]
    public float hitCooldown = 1f;

    [Tooltip("Collision radius of each orbiter")]
    public float collisionRadius = 0.5f;
}

/// <summary>
/// Visual settings for projectiles.
/// </summary>
[System.Serializable]
public class ProjectileVisuals
{
    [Tooltip("Sprite to use (null = default)")]
    public Sprite sprite;

    [Tooltip("Tint color")]
    public Color color = Color.white;

    [Tooltip("Whether projectile has trail")]
    public bool hasTrail = false;

    [Tooltip("Trail color if enabled")]
    public Color trailColor = Color.white;

    [Tooltip("Trail length")]
    public float trailLength = 0.5f;
}
