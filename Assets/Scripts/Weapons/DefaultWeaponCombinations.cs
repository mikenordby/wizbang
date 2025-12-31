using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory class that creates default WeaponCombination instances.
/// Used when no ScriptableObject assets exist in Resources/Combinations/.
/// Defines all Tier 2 weapon combination recipes.
/// </summary>
public static class DefaultWeaponCombinations
{
    /// <summary>
    /// Create all default weapon combinations.
    /// </summary>
    public static List<WeaponCombination> CreateAll()
    {
        return new List<WeaponCombination>
        {
            // Fire combinations
            CreateCircleOfFire(),
            CreateLaserBeam(),
            CreatePoisonedFlames(),

            // Additional combinations from design document
            CreateStormShield(),
            CreateArcaneGatling(),
            CreateToxicBarrage(),
            CreateFlameChain(),
            CreateFrozenStorm(),
            CreateBladeBlaze(),
            CreateArcaneAura()
        };
    }

    /// <summary>
    /// Circle of Fire = Flame Bolt [Fire] + Heat Aura [Area]
    /// </summary>
    public static WeaponCombination CreateCircleOfFire()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "CircleOfFire";
        combo.combinationName = "Circle of Fire";
        combo.description = "A powerful ring of fire surrounds you, burning all nearby enemies.";
        combo.weapon1Type = "flame_bolt";
        combo.weapon2Type = "heat_aura";
        combo.resultWeaponType = "circle_of_fire";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Laser Beam = Light Beam [Gun] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponCombination CreateLaserBeam()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "LaserBeam";
        combo.combinationName = "Laser Beam";
        combo.description = "A powerful fire beam that burns all enemies in its path.";
        combo.weapon1Type = "light_beam";
        combo.weapon2Type = "flame_bolt";
        combo.resultWeaponType = "laser_beam";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Poisoned Flames = Poison Cloud [Poison, Area] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponCombination CreatePoisonedFlames()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "PoisonedFlames";
        combo.combinationName = "Poisoned Flames";
        combo.description = "Toxic fire that burns and poisons enemies simultaneously.";
        combo.weapon1Type = "poison_cloud";
        combo.weapon2Type = "flame_bolt";
        combo.resultWeaponType = "poisoned_flames";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Storm Shield = Chain Lightning [Lightning] + Orbiting Blades [Melee]
    /// </summary>
    public static WeaponCombination CreateStormShield()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "StormShield";
        combo.combinationName = "Storm Shield";
        combo.description = "Orbiting electric blades that chain lightning to nearby enemies.";
        combo.weapon1Type = "chain_lightning";
        combo.weapon2Type = "orbiting_blades";
        combo.resultWeaponType = "storm_shield";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Arcane Gatling = Magic Missile [Arcane] + Rapid Fire [Gun]
    /// </summary>
    public static WeaponCombination CreateArcaneGatling()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "ArcaneGatling";
        combo.combinationName = "Arcane Gatling";
        combo.description = "Rapid-fire homing arcane projectiles.";
        combo.weapon1Type = "magic_missile";
        combo.weapon2Type = "rapid_fire";
        combo.resultWeaponType = "arcane_gatling";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Toxic Barrage = Poison Cloud [Poison, Area] + Rapid Fire [Gun]
    /// </summary>
    public static WeaponCombination CreateToxicBarrage()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "ToxicBarrage";
        combo.combinationName = "Toxic Barrage";
        combo.description = "Rapid poison projectiles that leave toxic trails.";
        combo.weapon1Type = "poison_cloud";
        combo.weapon2Type = "rapid_fire";
        combo.resultWeaponType = "toxic_barrage";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Flame Chain = Chain Lightning [Lightning] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponCombination CreateFlameChain()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "FlameChain";
        combo.combinationName = "Flame Chain";
        combo.description = "Lightning that ignites enemies on hit.";
        combo.weapon1Type = "chain_lightning";
        combo.weapon2Type = "flame_bolt";
        combo.resultWeaponType = "flame_chain";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Frozen Storm = Chain Lightning [Lightning] + Poison Cloud [Poison, Area]
    /// </summary>
    public static WeaponCombination CreateFrozenStorm()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "FrozenStorm";
        combo.combinationName = "Frozen Storm";
        combo.description = "Lightning strikes that leave poison pools.";
        combo.weapon1Type = "chain_lightning";
        combo.weapon2Type = "poison_cloud";
        combo.resultWeaponType = "frozen_storm";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Blade Blaze = Orbiting Blades [Melee] + Flame Bolt [Fire]
    /// </summary>
    public static WeaponCombination CreateBladeBlaze()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "BladeBlaze";
        combo.combinationName = "Blade Blaze";
        combo.description = "Flaming blades that orbit around you.";
        combo.weapon1Type = "orbiting_blades";
        combo.weapon2Type = "flame_bolt";
        combo.resultWeaponType = "blade_blaze";
        combo.tierMultiplier = 1.5f;
        return combo;
    }

    /// <summary>
    /// Arcane Aura = Magic Missile [Arcane] + Heat Aura [Area]
    /// </summary>
    public static WeaponCombination CreateArcaneAura()
    {
        var combo = ScriptableObject.CreateInstance<WeaponCombination>();
        combo.name = "ArcaneAura";
        combo.combinationName = "Arcane Aura";
        combo.description = "An arcane field that damages and tracks enemies.";
        combo.weapon1Type = "magic_missile";
        combo.weapon2Type = "heat_aura";
        combo.resultWeaponType = "arcane_aura";
        combo.tierMultiplier = 1.5f;
        return combo;
    }
}
