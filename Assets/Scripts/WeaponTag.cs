/// <summary>
/// Tags that define weapon characteristics for synergy bonuses.
/// Weapons with matching tags create multiplicative damage bonuses.
/// Simplified to 8 tags total: 5 elements + 3 types.
/// </summary>
public enum WeaponTag
{
    // Elements (5)
    Fire,
    Ice,
    Lightning,
    Poison,
    Arcane,

    // Types (3)
    Gun,      // Ranged projectile weapons
    Melee,    // Close-range attacks
    Area      // AoE effects
}
