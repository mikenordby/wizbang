using UnityEngine;
using System;

/// <summary>
/// Health component for entities. Handles HP, damage, death, and invincibility frames.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float iFrameDuration = 0f; // 0 = no i-frames (for enemies)
    
    private float currentHealth;
    private float iFrameTimer = 0f;
    private SpriteRenderer spriteRenderer;
    
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;
    public Transform Transform => transform;
    
    // Events
    public event Action<float> OnHealthChanged; // Passes current health
    public event Action OnDeath;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Initialize or reset health (for pooled objects)
    /// </summary>
    public void Initialize(float health)
    {
        maxHealth = health;
        currentHealth = health;
        iFrameTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    /// <summary>
    /// Apply damage with i-frame protection
    /// </summary>
    public bool TakeDamage(float damage)
    {
        // Check i-frames
        if (iFrameTimer > 0f)
            return false;
        
        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        Debug.Log($"Health.TakeDamage: {gameObject.name} took {damage} damage, HP: {currentHealth}/{maxHealth}");
        
        OnHealthChanged?.Invoke(currentHealth);
        
        // Start i-frame timer
        if (iFrameDuration > 0f)
        {
            iFrameTimer = iFrameDuration;
        }
        
        // Check death
        if (currentHealth <= 0f)
        {
            Debug.Log($"Health.TakeDamage: {gameObject.name} DIED! Invoking OnDeath event");
            OnDeath?.Invoke();
            return true; // Entity died
        }
        
        return false; // Still alive
    }
    
    /// <summary>
    /// Heal entity (clamp to max)
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    private void Update()
    {
        // Countdown i-frames
        if (iFrameTimer > 0f)
        {
            iFrameTimer -= Time.deltaTime;
            
            // Visual flash during i-frames
            if (spriteRenderer != null)
            {
                // Flash every 0.1 seconds
                float flashSpeed = 10f;
                float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0.3f, 1f, alpha);
                spriteRenderer.color = color;
            }
        }
        else
        {
            // Reset alpha when i-frames end (preserve RGB)
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                if (color.a != 1f)
                {
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }
}