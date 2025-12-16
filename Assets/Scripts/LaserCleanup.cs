using UnityEngine;

/// <summary>
/// Helper component to cleanup laser shot tracking when beam is destroyed.
/// Prevents memory leaks from abandoned hit tracking data.
/// </summary>
public class LaserCleanup : MonoBehaviour
{
    private LaserWeapon weapon;
    private int shotID;
    
    public void Initialize(LaserWeapon weapon, int shotID, float lifetime)
    {
        this.weapon = weapon;
        this.shotID = shotID;
        Destroy(gameObject, lifetime);
    }
    
    private void OnDestroy()
    {
        if (weapon != null)
        {
            weapon.CleanupShot(shotID);
        }
    }
}
