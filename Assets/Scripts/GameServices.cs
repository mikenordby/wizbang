using UnityEngine;

/// <summary>
/// Centralized service locator for game systems.
/// Provides singleton access to pools and managers.
/// Prevents static reference issues across scene loads.
/// </summary>
public class GameServices : MonoBehaviour
{
    private static GameServices instance;
    
    public static GameServices Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameServices>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameServices");
                    instance = go.AddComponent<GameServices>();
                    DebugLog.Warning("[GameServices] Created new instance (should exist in scene)");
                }
            }
            return instance;
        }
    }
    
    // Service references (auto-found on Awake)
    private XPOrbPool xpOrbPool;
    private EnemyPool enemyPool;
    private ProjectilePool projectilePool;
    private CollisionManager collisionManager;
    private OrbiterManager orbiterManager;
    private LevelUpUI levelUpUI;
    private DamageNumberPool damageNumberPool;
    
    // Public accessors with null safety
    public static XPOrbPool XPOrbPool => Instance?.xpOrbPool;
    public static EnemyPool EnemyPool => Instance?.enemyPool;
    public static ProjectilePool ProjectilePool => Instance?.projectilePool;
    public static CollisionManager CollisionManager => Instance?.collisionManager;
    public static OrbiterManager OrbiterManager => Instance?.orbiterManager;
    public static LevelUpUI LevelUpUI => Instance?.levelUpUI;
    public static DamageNumberPool DamageNumberPool => Instance?.damageNumberPool;
    
    private void Awake()
    {
        // Enforce singleton
        if (instance != null && instance != this)
        {
            DebugLog.Warning("[GameServices] Duplicate instance detected, destroying");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Auto-find services
        AutoFindServices();
        
        DebugLog.Info("[GameServices] Initialized successfully");
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    private void AutoFindServices()
    {
        xpOrbPool = FindFirstObjectByType<XPOrbPool>();
        enemyPool = FindFirstObjectByType<EnemyPool>();
        projectilePool = FindFirstObjectByType<ProjectilePool>();
        collisionManager = FindFirstObjectByType<CollisionManager>();
        orbiterManager = FindFirstObjectByType<OrbiterManager>();
        levelUpUI = FindFirstObjectByType<LevelUpUI>();
        damageNumberPool = FindFirstObjectByType<DamageNumberPool>();
        
        // Log warnings for missing services
        if (xpOrbPool == null) DebugLog.Warning("[GameServices] XPOrbPool not found");
        if (enemyPool == null) DebugLog.Warning("[GameServices] EnemyPool not found");
        if (projectilePool == null) DebugLog.Warning("[GameServices] ProjectilePool not found");
        if (collisionManager == null) DebugLog.Warning("[GameServices] CollisionManager not found");
        if (orbiterManager == null) DebugLog.Warning("[GameServices] OrbiterManager not found");
        if (levelUpUI == null) DebugLog.Warning("[GameServices] LevelUpUI not found");
    }
}
