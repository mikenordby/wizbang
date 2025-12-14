using UnityEngine;

/// <summary>
/// Boomerang weapon that throws projectiles in an arc that return to player.
/// Placeholder for future implementation.
/// </summary>
public class BoomerangWeapon : Weapon
{
    [Header("Boomerang Settings")]
    #pragma warning disable 0414 // Suppress unused field warnings for placeholder implementation
    [SerializeField] private float throwDistance = 8f;
    [SerializeField] private float arcHeight = 2f;
    #pragma warning restore 0414
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set weapon identity
        weaponName = "Boomerang";
        baseDamage = 12f;
        baseFireRate = 0.8f; // Slightly slower than Magic Missile
        projectileCount = 1;
        basePierce = 2; // Boomerang can hit multiple enemies on the way out AND back
        
        DebugLog.Info("[BoomerangWeapon] Initialized (placeholder - full implementation pending)");
    }
    
    protected override void Fire()
    {
        // TODO: Implement boomerang throw logic
        // 1. Find nearest enemy or random direction if no enemies
        // 2. Spawn boomerang projectile
        // 3. Set arc trajectory (out -> apex -> return)
        // 4. Boomerang damages on both outward and return trips
        
        DebugLog.Verbose("[BoomerangWeapon] Fire() called (not yet implemented)");
    }
    
    // TODO: Create BoomerangProjectile class with arc movement
    // TODO: Implement return-to-player logic
    // TODO: Handle multiple boomerangs with spread pattern
}
