using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Poison weapon that creates damaging clouds at enemy positions.
/// Clouds deal damage over time to all enemies within radius.
/// </summary>
public class PoisonWeapon : Weapon
{
    [Header("Poison Properties")]
    [SerializeField] private float cloudRadius = 0.5f;
    [SerializeField] private float maxCloudRadius = 1.2f; // Cap radius to prevent huge clouds at high levels
    [SerializeField] private float cloudDuration = 5f;
    [SerializeField] private float tickRate = 0.5f; // Damage per tick
    
    private List<PoisonCloud> activeClouds = new List<PoisonCloud>();
    private EnemyPool enemyPool;
    
    protected override void Start()
    {
        base.Start();
        weaponName = "Poison Cloud";
        baseDamage = 4f; // Lower base damage since it's DoT
        baseFireRate = 1.5f; // Slower fire rate
        damageType = DamageType.Poison;
        
        enemyPool = FindFirstObjectByType<EnemyPool>();
    }
    
    protected override void Fire()
    {
        if (enemyPool == null) return;
        
        // Find random enemy
        var enemies = enemyPool.GetActiveEnemies();
        if (enemies.Count == 0) return;
        
        Enemy target = enemies[Random.Range(0, enemies.Count)];
        if (target == null || !target.IsActive) return;
        
        // Create poison cloud at enemy position
        CreatePoisonCloud(target.transform.position);
    }
    
    private void CreatePoisonCloud(Vector3 position)
    {
        GameObject cloudObj = new GameObject("PoisonCloud");
        cloudObj.transform.position = position;
        
        // Cap radius to prevent huge clouds at high levels
        float cappedRadius = Mathf.Min(cloudRadius, maxCloudRadius);
        
        PoisonCloud cloud = cloudObj.AddComponent<PoisonCloud>();
        cloud.Initialize(cappedRadius, cloudDuration, currentDamage, tickRate, player, enemyPool);
        
        activeClouds.Add(cloud);
        
        // Clean up destroyed clouds
        activeClouds.RemoveAll(c => c == null);
    }
    
    protected override void ApplyLevelUpgrades()
    {
        base.ApplyLevelUpgrades();
        
        // Level scaling
        switch (weaponLevel)
        {
            case 2: cloudRadius = 3f; break;
            case 3: tickRate = 0.4f; break;
            case 4: cloudDuration = 6f; break;
            case 5: cloudRadius = 3.5f; break;
            case 6: tickRate = 0.3f; break;
            case 7: cloudDuration = 7f; break;
            case 8: cloudRadius = 4f; projectileCount = 2; break; // Fire 2 clouds
        }
    }
}

/// <summary>
/// Individual poison cloud that damages enemies over time
/// </summary>
public class PoisonCloud : MonoBehaviour
{
    private float radius;
    private float duration;
    private float damagePerTick;
    private float tickRate;
    private Player player;
    private EnemyPool enemyPool;
    
    private float lifetime;
    private float tickTimer;
    private SpriteRenderer spriteRenderer;
    private HashSet<int> damagedEnemies = new HashSet<int>(); // Track enemies hit this tick
    
    public void Initialize(float radius, float duration, float damage, float tickRate, Player player, EnemyPool pool)
    {
        this.radius = radius;
        this.duration = duration;
        this.damagePerTick = damage;
        this.tickRate = tickRate;
        this.player = player;
        this.enemyPool = pool;
        
        CreateVisual();
    }
    
    private void CreateVisual()
    {
        // Create poison cloud sprite
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCloudSprite();
        spriteRenderer.color = Color.white; // Use texture colors (dark green with outline)
        spriteRenderer.sortingOrder = 2; // Above ground (-100 to 0), below players/projectiles (5+)
        
        float visualScale = radius * 2f;
        transform.localScale = new Vector3(visualScale, visualScale, 1f);
    }
    
    private Sprite CreateCloudSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        int cx = size / 2;
        int cy = size / 2;
        float maxRadius = size / 2f;
        
        // Define colors
        Color poisonGreen = new Color(0.2f, 0.6f, 0.2f); // Dark toxic green
        Color outlineColor = new Color(0.1f, 0.1f, 0.1f); // Very dark outline
        
        // Create poison cloud with outline and bubbles
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float normalizedDist = dist / maxRadius;
                
                // Dark outline at edge
                if (normalizedDist >= 0.85f && normalizedDist < 1.0f)
                {
                    float outlineAlpha = 1.0f - ((normalizedDist - 0.85f) / 0.15f);
                    pixels[y * size + x] = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineAlpha * 0.8f);
                }
                // Main poison cloud body
                else if (normalizedDist < 0.85f)
                {
                    // Soft falloff for main cloud
                    float alpha = (1f - normalizedDist) * 0.7f;
                    
                    // Add poison bubbles (small circular variations)
                    float bubbleNoise = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.3f;
                    alpha = Mathf.Clamp01(alpha + bubbleNoise);
                    
                    // Darker center, slightly lighter edges for depth
                    float brightness = 0.8f + (normalizedDist * 0.4f);
                    Color pixelColor = poisonGreen * brightness;
                    pixels[y * size + x] = new Color(pixelColor.r, pixelColor.g, pixelColor.b, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        // Add a few distinct poison bubble spots
        System.Random random = new System.Random(42); // Consistent seed for repeatable bubbles
        for (int i = 0; i < 5; i++)
        {
            float angle = i * (360f / 5f) * Mathf.Deg2Rad;
            float bubbleDist = random.Next(5, 15);
            int bx = cx + (int)(Mathf.Cos(angle) * bubbleDist);
            int by = cy + (int)(Mathf.Sin(angle) * bubbleDist);
            int bubbleSize = random.Next(3, 6);
            
            for (int dy = -bubbleSize; dy <= bubbleSize; dy++)
            {
                for (int dx = -bubbleSize; dx <= bubbleSize; dx++)
                {
                    int px = bx + dx;
                    int py = by + dy;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        float bubbleDist2 = Mathf.Sqrt(dx * dx + dy * dy);
                        if (bubbleDist2 < bubbleSize)
                        {
                            float bubbleAlpha = (1f - bubbleDist2 / bubbleSize) * 0.4f;
                            Color existingColor = pixels[py * size + px];
                            // Blend bubble into existing cloud
                            float newAlpha = Mathf.Min(existingColor.a + bubbleAlpha, 1f);
                            pixels[py * size + px] = new Color(existingColor.r, existingColor.g, existingColor.b, newAlpha);
                        }
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear; // Smooth edges
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    private void Update()
    {
        if (GameState.IsPaused) return;
        
        lifetime += Time.deltaTime;
        tickTimer += Time.deltaTime;
        
        // Fade out over time
        if (spriteRenderer != null)
        {
            float fadeAlpha = Mathf.Lerp(0.5f, 0.1f, lifetime / duration);
            Color color = spriteRenderer.color;
            color.a = fadeAlpha;
            spriteRenderer.color = color;
        }
        
        // Apply damage on tick
        if (tickTimer >= tickRate)
        {
            DamageEnemiesInRadius();
            tickTimer = 0f;
            damagedEnemies.Clear();
        }
        
        // Destroy when expired
        if (lifetime >= duration)
        {
            Destroy(gameObject);
        }
    }
    
    private void DamageEnemiesInRadius()
    {
        if (enemyPool == null) return;
        
        // Create snapshot to avoid collection modification during iteration
        var enemies = new System.Collections.Generic.List<Enemy>(enemyPool.GetActiveEnemies());
        
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsActive) continue;
            
            // Check if already damaged this tick
            int enemyID = enemy.GetInstanceID();
            if (damagedEnemies.Contains(enemyID)) continue;
            
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= radius)
            {
                // Apply poison damage
                var context = new DamageContext
                {
                    baseDamage = damagePerTick,
                    player = player,
                    enemy = enemy,
                    damageType = DamageType.Poison
                };
                var result = DamageCalculator.Instance.CalculateDamage(context);
                
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(result.finalDamage);
                    
                    // Show smaller damage numbers for DoT
                    DamageNumberPool.Instance?.ShowDamage(enemy.transform.position, result.finalDamage);
                }
                
                damagedEnemies.Add(enemyID);
            }
        }
    }
}
