using System;

/// <summary>
/// Factory for creating weapon behavior instances based on behavior type.
/// </summary>
public static class WeaponBehaviorFactory
{
    /// <summary>
    /// Create a weapon behavior instance for the specified type.
    /// </summary>
    public static IWeaponBehavior Create(WeaponBehaviorType type)
    {
        return type switch
        {
            WeaponBehaviorType.Projectile => new ProjectileBehavior(),
            WeaponBehaviorType.AoE => new AoEBehavior(),
            WeaponBehaviorType.Chain => new ChainBehavior(),
            WeaponBehaviorType.Beam => new BeamBehavior(),
            WeaponBehaviorType.Orbiter => new OrbiterBehavior(),
            _ => throw new ArgumentException($"Unknown weapon behavior type: {type}")
        };
    }
}
