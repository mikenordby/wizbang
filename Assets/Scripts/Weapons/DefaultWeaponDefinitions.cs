using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory class that creates default WeaponDefinition instances.
/// Used when no ScriptableObject assets exist in Resources/Weapons/.
/// These definitions mirror the stats from the legacy weapon classes.
/// </summary>
public static class DefaultWeaponDefinitions
{
    /// <summary>
    /// Create all default weapon definitions.
    /// </summary>
    public static List<WeaponDefinition> CreateAll()
    {
        return new List<WeaponDefinition>
        {
            // Tier 1 Basic Weapons (8 total)
            CreateMagicMissile(),
            CreateRapidFire(),
            CreateOrbitingBlades(),
            CreateChainLightning(),
            CreatePoisonCloud(),
            CreateFlameBolt(),
            CreateHeatAura(),
            CreateLightBeam(),
            // Tier 2 Combined Weapons (10 total)
            CreateCircleOfFire(),   // Flame Bolt + Heat Aura
            CreateLaserBeam(),      // Light Beam + Flame Bolt
            CreatePoisonedFlames(), // Poison Cloud + Flame Bolt
            CreateStormShield(),    // Chain Lightning + Orbiting Blades
            CreateArcaneGatling(),  // Magic Missile + Rapid Fire
            CreateToxicBarrage(),   // Poison Cloud + Rapid Fire
            CreateFlameChain(),     // Chain Lightning + Flame Bolt
            CreateFrozenStorm(),    // Chain Lightning + Poison Cloud
            CreateBladeBlaze(),     // Orbiting Blades + Flame Bolt
            CreateArcaneAura()      // Magic Missile + Heat Aura
        };
    }

    /// <summary>
    /// Magic Missile - Auto-aim arcane projectiles
    /// </summary>
    public static WeaponDefinition CreateMagicMissile()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "MagicMissile";
        def.weaponId = "magic_missile";
        def.displayName = "Magic Missile";
        def.description = "Fires arcane projectiles that home in on the nearest enemy.";

        // Stats from ProjectileWeapon.cs
        def.baseDamage = 10f;
        def.baseFireRate = 1f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1.2f;
        def.damageType = DamageType.Arcane;

        def.tags = new List<WeaponTag> { WeaponTag.Arcane };
        def.behaviorType = WeaponBehaviorType.Projectile;

        def.projectileSettings = new ProjectileSettings
        {
            aimType = AimType.NearestEnemy,
            spreadAngle = 15f,
            projectileSpeed = 10f,
            lifetime = 2f,
            homing = false,
            targetingRange = 15f,
            fireWithoutTarget = false
        };

        def.visuals = new ProjectileVisuals
        {
            color = new Color(0.8f, 0.4f, 0.9f), // Purple/arcane
            hasTrail = false
        };

        return def;
    }

    /// <summary>
    /// Rapid Fire - Fast-shooting gun weapon
    /// </summary>
    public static WeaponDefinition CreateRapidFire()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "RapidFire";
        def.weaponId = "rapid_fire";
        def.displayName = "Rapid Fire Pistol";
        def.description = "Shoots rapidly with low damage but high fire rate.";

        // Stats from RapidFireWeapon.cs
        def.baseDamage = 5f;
        def.baseFireRate = 4f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 0.8f;
        def.projectileSize = 0.7f;
        def.damageType = DamageType.Physical;

        def.tags = new List<WeaponTag> { WeaponTag.Gun };
        def.behaviorType = WeaponBehaviorType.Projectile;

        def.projectileSettings = new ProjectileSettings
        {
            aimType = AimType.NearestEnemy,
            spreadAngle = 8f,
            projectileSpeed = 12f,
            lifetime = 1.5f,
            homing = false,
            targetingRange = 12f,
            fireWithoutTarget = true
        };

        def.visuals = new ProjectileVisuals
        {
            color = Color.yellow,
            hasTrail = false
        };

        return def;
    }

    /// <summary>
    /// Orbiting Blades - Melee blades that circle the player
    /// </summary>
    public static WeaponDefinition CreateOrbitingBlades()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "OrbitingBlades";
        def.weaponId = "orbiting_blades";
        def.displayName = "Orbiting Blades";
        def.description = "Spinning blades that orbit around you, damaging nearby enemies.";

        // Stats from OrbiterWeapon.cs
        def.baseDamage = 15f;
        def.baseFireRate = 0f; // Always active
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1.5f;
        def.damageType = DamageType.Physical;

        def.tags = new List<WeaponTag> { WeaponTag.Melee };
        def.behaviorType = WeaponBehaviorType.Orbiter;

        def.orbiterSettings = new OrbiterSettings
        {
            orbitRadius = 2f,
            orbitSpeed = 8f,
            hitCooldown = 1f,
            collisionRadius = 0.5f
        };

        def.visuals = new ProjectileVisuals
        {
            color = Color.white,
            hasTrail = false
        };

        return def;
    }

    /// <summary>
    /// Chain Lightning - Electricity that chains between enemies
    /// </summary>
    public static WeaponDefinition CreateChainLightning()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "ChainLightning";
        def.weaponId = "chain_lightning";
        def.displayName = "Chain Lightning";
        def.description = "Lightning that jumps between multiple enemies.";

        // Stats from LightningWeapon.cs (buffed)
        def.baseDamage = 15f;
        def.baseFireRate = 0.8f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Lightning;

        def.tags = new List<WeaponTag> { WeaponTag.Lightning };
        def.behaviorType = WeaponBehaviorType.Chain;

        def.chainSettings = new ChainSettings
        {
            maxChains = 5,
            chainRange = 4f,
            chainDamageMultiplier = 0.75f,
            targetingRange = 9f,
            effectDuration = 0.2f
        };

        return def;
    }

    /// <summary>
    /// Poison Cloud - AoE poison damage
    /// </summary>
    public static WeaponDefinition CreatePoisonCloud()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "PoisonCloud";
        def.weaponId = "poison_cloud";
        def.displayName = "Poison Cloud";
        def.description = "Creates a toxic cloud that damages enemies over time.";

        // Stats from PoisonWeapon.cs (buffed)
        def.baseDamage = 3f; // Per tick
        def.baseFireRate = 0.3f; // Spawns every ~3 seconds
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Poison;

        def.tags = new List<WeaponTag> { WeaponTag.Poison, WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.AoE;

        def.aoeSettings = new AoESettings
        {
            radius = 3.25f,
            tickRate = 0.4f,
            duration = 6f,
            followPlayer = false,
            expandOverTime = true,
            maxRadius = 4f,
            spawnAtPlayer = false // Spawns at enemy
        };

        return def;
    }

    /// <summary>
    /// Flame Bolt - Fire projectile weapon (Tier 1)
    /// Combines with Heat Aura to create Circle of Fire
    /// </summary>
    public static WeaponDefinition CreateFlameBolt()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "FlameBolt";
        def.weaponId = "flame_bolt";
        def.displayName = "Flame Bolt";
        def.description = "Hurls blazing bolts at enemies. Burns on impact.";

        def.baseDamage = 8f;
        def.baseFireRate = 1.2f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire;

        def.tags = new List<WeaponTag> { WeaponTag.Fire };
        def.behaviorType = WeaponBehaviorType.Projectile;

        def.projectileSettings = new ProjectileSettings
        {
            aimType = AimType.NearestEnemy,
            spreadAngle = 10f,
            projectileSpeed = 9f,
            lifetime = 2f,
            homing = false,
            targetingRange = 12f,
            fireWithoutTarget = false
        };

        def.visuals = new ProjectileVisuals
        {
            color = new Color(1f, 0.5f, 0.1f), // Orange-red fire
            hasTrail = true
        };

        return def;
    }

    /// <summary>
    /// Heat Aura - Small persistent fire aura (Tier 1)
    /// Combines with Flame Bolt to create Circle of Fire
    /// </summary>
    public static WeaponDefinition CreateHeatAura()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "HeatAura";
        def.weaponId = "heat_aura";
        def.displayName = "Heat Aura";
        def.description = "A small aura of heat that damages nearby enemies.";

        def.baseDamage = 2f; // Low damage per tick
        def.baseFireRate = 1f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire;

        def.tags = new List<WeaponTag> { WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.AoE;

        def.aoeSettings = new AoESettings
        {
            radius = 1.5f, // Small radius
            tickRate = 0.5f,
            duration = 0f, // Permanent
            followPlayer = true,
            expandOverTime = false,
            maxRadius = 1.5f,
            spawnAtPlayer = true
        };

        return def;
    }

    /// <summary>
    /// Light Beam - Short physical beam (Tier 1)
    /// Combines with Flame Bolt to create Laser Beam
    /// </summary>
    public static WeaponDefinition CreateLightBeam()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "LightBeam";
        def.weaponId = "light_beam";
        def.displayName = "Light Beam";
        def.description = "A short focused beam of light that pierces through enemies.";

        def.baseDamage = 3f; // Per tick
        def.baseFireRate = 2f;
        def.projectileCount = 1;
        def.basePierce = 999; // Infinite pierce
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Physical;

        def.tags = new List<WeaponTag> { WeaponTag.Gun };
        def.behaviorType = WeaponBehaviorType.Beam;

        def.beamSettings = new BeamSettings
        {
            beamLength = 8f, // Short beam
            beamWidth = 0.3f,
            rotationSpeed = 0f,
            autoRotate = false,
            tickRate = 2f
        };

        return def;
    }

    /// <summary>
    /// Laser Beam - Continuous damage beam (Tier 2 Combined)
    /// Created by combining Light Beam [Gun] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponDefinition CreateLaserBeam()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "LaserBeam";
        def.weaponId = "laser_beam";
        def.displayName = "Laser Beam";
        def.description = "A powerful fire beam that burns all enemies in its path.";

        // Tier 2 combined stats (1.5x multiplier applied)
        def.baseDamage = 7f; // Per tick (boosted from base 5)
        def.baseFireRate = 2.5f;
        def.projectileCount = 1;
        def.basePierce = 999; // Infinite pierce
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire;

        // Combined tags: Gun (Light Beam) + Fire (Flame Bolt)
        def.tags = new List<WeaponTag> { WeaponTag.Gun, WeaponTag.Fire };
        def.behaviorType = WeaponBehaviorType.Beam;

        def.beamSettings = new BeamSettings
        {
            beamLength = 13.5f, // Longer than Light Beam
            beamWidth = 0.4f,
            rotationSpeed = 0f,
            autoRotate = false,
            tickRate = 2f
        };

        return def;
    }

    /// <summary>
    /// Circle of Fire - AoE ring around player (Tier 2 Combined)
    /// Created by combining Flame Bolt [Fire] + Heat Aura [Area]
    /// </summary>
    public static WeaponDefinition CreateCircleOfFire()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "CircleOfFire";
        def.weaponId = "circle_of_fire";
        def.displayName = "Circle of Fire";
        def.description = "A powerful ring of fire surrounds you, burning all nearby enemies.";

        // Tier 2 combined stats (1.5x multiplier from Flame Bolt base 8)
        def.baseDamage = 12f; // Per tick (boosted)
        def.baseFireRate = 1f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire;

        // Combined tags: Fire (Flame Bolt) + Area (Heat Aura)
        def.tags = new List<WeaponTag> { WeaponTag.Fire, WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.AoE;

        def.aoeSettings = new AoESettings
        {
            radius = 2.5f, // Larger than Heat Aura's 1.5
            tickRate = 0.8f, // Faster than Heat Aura
            duration = 0f, // Permanent
            followPlayer = true,
            expandOverTime = false,
            maxRadius = 2.5f,
            spawnAtPlayer = true
        };

        return def;
    }

    /// <summary>
    /// Poisoned Flames - Combined weapon (Fire + Poison)
    /// </summary>
    public static WeaponDefinition CreatePoisonedFlames()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "PoisonedFlames";
        def.weaponId = "poisoned_flames";
        def.displayName = "Poisoned Flames";
        def.description = "Toxic fire that burns and poisons enemies simultaneously.";

        // Combined weapon stats
        def.baseDamage = 12f;
        def.baseFireRate = 1f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire;

        // Combined tags from Fire Ring + Poison
        def.tags = new List<WeaponTag> { WeaponTag.Fire, WeaponTag.Poison, WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.AoE;

        def.aoeSettings = new AoESettings
        {
            radius = 3f,
            tickRate = 1.0f, // Reduced from 0.4 - now 1 tick per second
            duration = 0f, // Permanent ring
            followPlayer = true,
            expandOverTime = false,
            maxRadius = 3f,
            spawnAtPlayer = true
        };

        return def;
    }

    /// <summary>
    /// Storm Shield - Electric orbiting blades (Tier 2 Combined)
    /// Created by combining Chain Lightning [Lightning] + Orbiting Blades [Melee]
    /// </summary>
    public static WeaponDefinition CreateStormShield()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "StormShield";
        def.weaponId = "storm_shield";
        def.displayName = "Storm Shield";
        def.description = "Orbiting electric blades that chain lightning to nearby enemies.";

        def.baseDamage = 18f; // Higher than base orbiters
        def.baseFireRate = 0f;
        def.projectileCount = 2; // More blades
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1.5f;
        def.damageType = DamageType.Lightning;

        def.tags = new List<WeaponTag> { WeaponTag.Lightning, WeaponTag.Melee };
        def.behaviorType = WeaponBehaviorType.Orbiter;

        def.orbiterSettings = new OrbiterSettings
        {
            orbitRadius = 2.5f,
            orbitSpeed = 10f,
            hitCooldown = 0.8f,
            collisionRadius = 0.6f
        };

        return def;
    }

    /// <summary>
    /// Arcane Gatling - Rapid homing projectiles (Tier 2 Combined)
    /// Created by combining Magic Missile [Arcane] + Rapid Fire [Gun]
    /// </summary>
    public static WeaponDefinition CreateArcaneGatling()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "ArcaneGatling";
        def.weaponId = "arcane_gatling";
        def.displayName = "Arcane Gatling";
        def.description = "Rapid-fire homing arcane projectiles.";

        def.baseDamage = 7f;
        def.baseFireRate = 5f; // Very fast
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 0.8f;
        def.damageType = DamageType.Arcane;

        def.tags = new List<WeaponTag> { WeaponTag.Arcane, WeaponTag.Gun };
        def.behaviorType = WeaponBehaviorType.Projectile;

        def.projectileSettings = new ProjectileSettings
        {
            aimType = AimType.NearestEnemy,
            spreadAngle = 5f,
            projectileSpeed = 14f,
            lifetime = 2f,
            homing = true,
            homingStrength = 4f,
            targetingRange = 15f,
            fireWithoutTarget = false
        };

        def.visuals = new ProjectileVisuals
        {
            color = new Color(0.7f, 0.3f, 1f), // Purple arcane
            hasTrail = true
        };

        return def;
    }

    /// <summary>
    /// Toxic Barrage - Rapid poison projectiles (Tier 2 Combined)
    /// Created by combining Poison Cloud [Poison, Area] + Rapid Fire [Gun]
    /// </summary>
    public static WeaponDefinition CreateToxicBarrage()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "ToxicBarrage";
        def.weaponId = "toxic_barrage";
        def.displayName = "Toxic Barrage";
        def.description = "Rapid poison projectiles that leave toxic trails.";

        def.baseDamage = 6f;
        def.baseFireRate = 4.5f;
        def.projectileCount = 1;
        def.basePierce = 1; // Passes through one enemy
        def.baseRange = 0.9f;
        def.projectileSize = 0.8f;
        def.damageType = DamageType.Poison;

        def.tags = new List<WeaponTag> { WeaponTag.Poison, WeaponTag.Area, WeaponTag.Gun };
        def.behaviorType = WeaponBehaviorType.Projectile;

        def.projectileSettings = new ProjectileSettings
        {
            aimType = AimType.NearestEnemy,
            spreadAngle = 12f,
            projectileSpeed = 11f,
            lifetime = 1.8f,
            homing = false,
            targetingRange = 12f,
            fireWithoutTarget = true
        };

        def.visuals = new ProjectileVisuals
        {
            color = new Color(0.4f, 0.9f, 0.2f), // Toxic green
            hasTrail = true
        };

        return def;
    }

    /// <summary>
    /// Flame Chain - Fire chain lightning (Tier 2 Combined)
    /// Created by combining Chain Lightning [Lightning] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponDefinition CreateFlameChain()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "FlameChain";
        def.weaponId = "flame_chain";
        def.displayName = "Flame Chain";
        def.description = "Lightning that ignites enemies, spreading fire between them.";

        def.baseDamage = 18f;
        def.baseFireRate = 0.9f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Fire; // Fire damage type

        def.tags = new List<WeaponTag> { WeaponTag.Lightning, WeaponTag.Fire };
        def.behaviorType = WeaponBehaviorType.Chain;

        def.chainSettings = new ChainSettings
        {
            maxChains = 6,
            chainRange = 4.5f,
            chainDamageMultiplier = 0.8f,
            targetingRange = 10f,
            effectDuration = 0.25f
        };

        return def;
    }

    /// <summary>
    /// Frozen Storm - Poison chain lightning (Tier 2 Combined)
    /// Created by combining Chain Lightning [Lightning] + Poison Cloud [Poison, Area]
    /// </summary>
    public static WeaponDefinition CreateFrozenStorm()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "FrozenStorm";
        def.weaponId = "frozen_storm";
        def.displayName = "Frozen Storm";
        def.description = "Lightning strikes that leave poison pools on enemies.";

        def.baseDamage = 16f;
        def.baseFireRate = 0.7f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Poison;

        def.tags = new List<WeaponTag> { WeaponTag.Lightning, WeaponTag.Poison, WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.Chain;

        def.chainSettings = new ChainSettings
        {
            maxChains = 5,
            chainRange = 5f,
            chainDamageMultiplier = 0.7f,
            targetingRange = 10f,
            effectDuration = 0.3f
        };

        return def;
    }

    /// <summary>
    /// Blade Blaze - Flaming orbiting blades (Tier 2 Combined)
    /// Created by combining Orbiting Blades [Melee] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponDefinition CreateBladeBlaze()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "BladeBlaze";
        def.weaponId = "blade_blaze";
        def.displayName = "Blade Blaze";
        def.description = "Flaming blades that orbit around you, burning enemies.";

        def.baseDamage = 20f;
        def.baseFireRate = 0f;
        def.projectileCount = 2;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1.6f;
        def.damageType = DamageType.Fire;

        def.tags = new List<WeaponTag> { WeaponTag.Melee, WeaponTag.Fire };
        def.behaviorType = WeaponBehaviorType.Orbiter;

        def.orbiterSettings = new OrbiterSettings
        {
            orbitRadius = 2.2f,
            orbitSpeed = 9f,
            hitCooldown = 0.7f,
            collisionRadius = 0.55f
        };

        return def;
    }

    /// <summary>
    /// Arcane Aura - Arcane damage aura (Tier 2 Combined)
    /// Created by combining Magic Missile [Arcane] + Heat Aura [Area]
    /// </summary>
    public static WeaponDefinition CreateArcaneAura()
    {
        var def = ScriptableObject.CreateInstance<WeaponDefinition>();
        def.name = "ArcaneAura";
        def.weaponId = "arcane_aura";
        def.displayName = "Arcane Aura";
        def.description = "An arcane field that damages and weakens nearby enemies.";

        def.baseDamage = 5f;
        def.baseFireRate = 1f;
        def.projectileCount = 1;
        def.basePierce = 0;
        def.baseRange = 1f;
        def.projectileSize = 1f;
        def.damageType = DamageType.Arcane;

        def.tags = new List<WeaponTag> { WeaponTag.Arcane, WeaponTag.Area };
        def.behaviorType = WeaponBehaviorType.AoE;

        def.aoeSettings = new AoESettings
        {
            radius = 2.5f,
            tickRate = 0.6f,
            duration = 0f, // Permanent
            followPlayer = true,
            expandOverTime = false,
            maxRadius = 2.5f,
            spawnAtPlayer = true
        };

        return def;
    }
}
