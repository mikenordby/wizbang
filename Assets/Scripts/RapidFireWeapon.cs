using UnityEngine;

/// <summary>
/// Rapid-fire pistol weapon that shoots straight ahead at high speed.
/// Placeholder for future implementation.
/// </summary>
public class RapidFireWeapon : Weapon
{
    [Header("Rapid Fire Settings")]
    #pragma warning disable 0414 // Suppress unused field warning for placeholder implementation
    [SerializeField] private float bulletSpeed = 15f; // Faster than magic missile
    [SerializeField] private float spreadAngle = 5f; // Tight spread
    #pragma warning restore 0414
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set weapon identity
        weaponName = "Rapid Fire Pistol";
        baseDamage = 5f; // Lower damage per shot
        baseFireRate = 5f; // MUCH faster fire rate (5 shots per second)
        projectileCount = 1;
        basePierce = 0; // No pierce by default
        baseRange = 0.8f; // Shorter range
        
        DebugLog.Info("[RapidFireWeapon] Initialized (placeholder - full implementation pending)");
    }
    
    protected override void Fire()
    {
        // TODO: Implement rapid fire logic
        // 1. Shoot straight ahead (no auto-aim, shoots in player's facing direction)
        // 2. Spawn fast-moving bullet projectile
        // 3. Apply tight spread if projectileCount > 1
        // 4. Visual: muzzle flash effect
        // 5. Audio: rapid "pew pew" sound
        
        DebugLog.Verbose("[RapidFireWeapon] Fire() called (not yet implemented)");
    }
    
    // TODO: Create BulletProjectile class (faster, smaller sprite)
    // TODO: Implement player facing direction tracking
    // TODO: Add muzzle flash particle effect
    // TODO: Consider recoil or screen shake on rapid fire
}
