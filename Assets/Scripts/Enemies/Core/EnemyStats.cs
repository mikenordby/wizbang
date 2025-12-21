using UnityEngine;

/// <summary>
/// ScriptableObject defining stats for an enemy type.
/// Create instances in Unity: Assets > Create > Enemy Stats
/// </summary>
[CreateAssetMenu(fileName = "EnemyStats", menuName = "Wizbang/Enemy Stats", order = 1)]
public class EnemyStats : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Blob";
    
    [Header("Combat Stats")]
    [Tooltip("Max health points")]
    public float maxHealth = 20f;
    
    [Tooltip("Damage dealt to player on contact per second")]
    public float contactDamage = 10f;
    
    [Header("Movement")]
    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 2f;
    
    [Header("Rewards")]
    [Tooltip("XP dropped on death")]
    public int xpDrop = 5;
    
    [Header("Visuals")]
    [Tooltip("Enemy color (sprite will be tinted)")]
    public Color color = Color.green;
    
    [Tooltip("Scale multiplier (1.0 = normal size)")]
    public float scale = 1f;
}