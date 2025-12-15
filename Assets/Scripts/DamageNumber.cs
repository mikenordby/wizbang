using UnityEngine;
using TMPro;

/// <summary>
/// Floating damage number that appears when an enemy takes damage
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 2f;
    
    private TextMeshPro textMesh;
    private float timer;
    private Color startColor;
    
    private void Awake()
    {
        // Get or create TextMeshPro component
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }
        
        // Configure text mesh
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 3;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.sortingOrder = 100; // Render on top of everything
    }
    
    /// <summary>
    /// Spawn a damage number at a position
    /// </summary>
    public void Show(Vector3 position, float damage, bool isCrit = false)
    {
        transform.position = position;
        timer = lifetime;
        
        // Set text
        textMesh.text = damage.ToString("F0");
        
        // Set color based on crit (gold #FFD700 for crits)
        startColor = isCrit ? new Color(1f, 0.843f, 0f) : Color.white; // Gold: #FFD700
        textMesh.color = startColor;
        
        // Make 1.5x larger if crit
        textMesh.fontSize = isCrit ? 4.5f : 3f;
        
        gameObject.SetActive(true);
    }
    
    private void Update()
    {
        if (GameState.IsPaused) return;
        
        // Move upward
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        // Fade out
        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / lifetime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        
        // Deactivate when done
        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}
