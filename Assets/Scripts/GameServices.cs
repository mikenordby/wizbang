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
    private TreasureUI treasureUI;
    private DamageNumberPool damageNumberPool;
    private Player player;
    private DamageCalculator damageCalculator;
    private ItemDatabase itemDatabase;
    private SynergyManager synergyManager;
    private CombinationManager combinationManager;
    private ItemEffectsManager itemEffectsManager;
    private WeaponDefinitionDatabase weaponDefinitionDatabase;

    // Public accessors with null safety
    public static XPOrbPool XPOrbPool => Instance?.xpOrbPool;
    public static EnemyPool EnemyPool => Instance?.enemyPool;
    public static ProjectilePool ProjectilePool => Instance?.projectilePool;
    public static CollisionManager CollisionManager => Instance?.collisionManager;
    public static OrbiterManager OrbiterManager => Instance?.orbiterManager;
    public static LevelUpUI LevelUpUI => Instance?.levelUpUI;
    public static TreasureUI TreasureUI => Instance?.treasureUI;
    public static DamageNumberPool DamageNumberPool => Instance?.damageNumberPool;
    public static Player Player => Instance?.player;
    public static DamageCalculator DamageCalculator => Instance?.damageCalculator;
    public static ItemDatabase ItemDatabase => Instance?.itemDatabase;
    public static SynergyManager SynergyManager => Instance?.synergyManager;
    public static CombinationManager CombinationManager => Instance?.combinationManager;
    public static ItemEffectsManager ItemEffectsManager => Instance?.itemEffectsManager;
    public static WeaponDefinitionDatabase WeaponDefinitionDatabase => Instance?.weaponDefinitionDatabase;

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
        treasureUI = FindFirstObjectByType<TreasureUI>();
        damageNumberPool = FindFirstObjectByType<DamageNumberPool>();
        player = FindFirstObjectByType<Player>();
        damageCalculator = FindFirstObjectByType<DamageCalculator>();
        itemDatabase = FindFirstObjectByType<ItemDatabase>();
        synergyManager = FindFirstObjectByType<SynergyManager>();
        combinationManager = FindFirstObjectByType<CombinationManager>();
        itemEffectsManager = FindFirstObjectByType<ItemEffectsManager>();
        weaponDefinitionDatabase = FindFirstObjectByType<WeaponDefinitionDatabase>();

        // Create WeaponDefinitionDatabase if not found (essential for data-driven weapons)
        if (weaponDefinitionDatabase == null)
        {
            GameObject weaponDbObj = new GameObject("WeaponDefinitionDatabase");
            weaponDefinitionDatabase = weaponDbObj.AddComponent<WeaponDefinitionDatabase>();
            DebugLog.Info("[GameServices] Created WeaponDefinitionDatabase");
        }

        // Create ItemDatabase if not found (essential for item system)
        if (itemDatabase == null)
        {
            GameObject itemDbObj = new GameObject("ItemDatabase");
            itemDatabase = itemDbObj.AddComponent<ItemDatabase>();
            DebugLog.Info("[GameServices] Created ItemDatabase");
        }
        
        // Create TreasureUI if not found (essential for chest system)
        if (treasureUI == null)
        {
            GameObject treasureUIObj = new GameObject("TreasureUI");
            treasureUI = treasureUIObj.AddComponent<TreasureUI>();
            DebugLog.Info("[GameServices] Created TreasureUI");
        }
        
        // Log warnings for missing services
        if (xpOrbPool == null) DebugLog.Warning("[GameServices] XPOrbPool not found");
        if (enemyPool == null) DebugLog.Warning("[GameServices] EnemyPool not found");
        if (projectilePool == null) DebugLog.Warning("[GameServices] ProjectilePool not found");
        if (collisionManager == null) DebugLog.Warning("[GameServices] CollisionManager not found");
        if (orbiterManager == null) DebugLog.Warning("[GameServices] OrbiterManager not found");
        if (levelUpUI == null) DebugLog.Warning("[GameServices] LevelUpUI not found");
        if (player == null) DebugLog.Warning("[GameServices] Player not found");
        if (damageCalculator == null) DebugLog.Warning("[GameServices] DamageCalculator not found");
        if (synergyManager == null) DebugLog.Warning("[GameServices] SynergyManager not found");
        if (combinationManager == null) DebugLog.Warning("[GameServices] CombinationManager not found");
        if (itemEffectsManager == null) DebugLog.Warning("[GameServices] ItemEffectsManager not found - item effects will not work!");
    }

    // Manual registration methods (for services that register themselves)
    public static void RegisterSynergyManager(SynergyManager manager)
    {
        if (Instance != null)
        {
            Instance.synergyManager = manager;
            DebugLog.Info("[GameServices] SynergyManager registered");
        }
    }

    public static void RegisterCombinationManager(CombinationManager manager)
    {
        if (Instance != null)
        {
            Instance.combinationManager = manager;
            DebugLog.Info("[GameServices] CombinationManager registered");
        }
    }

    public static void RegisterItemEffectsManager(ItemEffectsManager manager)
    {
        if (Instance != null)
        {
            Instance.itemEffectsManager = manager;
            DebugLog.Info("[GameServices] ItemEffectsManager registered");
        }
    }

    public static void RegisterWeaponDefinitionDatabase(WeaponDefinitionDatabase database)
    {
        if (Instance != null)
        {
            Instance.weaponDefinitionDatabase = database;
            DebugLog.Info("[GameServices] WeaponDefinitionDatabase registered");
        }
    }

    /// <summary>
    /// Re-find services that might have been created after initial Awake().
    /// Call this after character selection or scene changes.
    /// </summary>
    public static void RefreshServices()
    {
        if (Instance != null)
        {
            Instance.AutoFindServices();
            DebugLog.Info("[GameServices] Services refreshed");
        }
    }

    /// <summary>
    /// Manually register the player (called after character selection)
    /// </summary>
    public static void RegisterPlayer(Player newPlayer)
    {
        if (Instance != null && newPlayer != null)
        {
            Instance.player = newPlayer;
            DebugLog.Info($"[GameServices] Player registered: {newPlayer.name}");
        }
    }
}
