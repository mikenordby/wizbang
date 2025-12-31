using UnityEngine;
using TMPro;

/// <summary>
/// Floating damage number that appears when an enemy takes damage
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float scaleSpeed = 8f; // How fast the pop-in happens
    
    private TextMeshPro textMesh;
    private float timer;
    private Color startColor;
    private float currentScale;
    private float targetScale;
    private Vector3 moveDirection;
    
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
        textMesh.fontSize = 6f; // Good readable size
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.sortingOrder = 100; // Render on top of everything
        textMesh.enableAutoSizing = false;

        // Create a material instance with outline
        // TMP outlines require material property modification
        if (textMesh.fontMaterial != null)
        {
            // Create instance to avoid modifying shared material
            Material mat = new Material(textMesh.fontMaterial);

            // Enable outline via shader keywords
            mat.EnableKeyword("OUTLINE_ON");
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.3f);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            // Make text rendering crisper (less anti-aliasing for blockier look)
            mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.1f);

            textMesh.fontMaterial = mat;
        }

        // Wider spacing for chunkier appearance
        textMesh.characterSpacing = 5f;
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
        
        // Set target font size (1.5x larger if crit)
        float baseFontSize = isCrit ? 9f : 6f;
        targetScale = baseFontSize;

        // Start small for pop-in animation
        currentScale = 0f;
        textMesh.fontSize = 0f;

        // Add slight random horizontal spread
        float randomAngle = Random.Range(-30f, 30f);
        moveDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.up;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Show damage number with custom color (for player damage)
    /// </summary>
    public void ShowCustom(Vector3 position, float damage, Color color, bool isCrit = false)
    {
        transform.position = position;
        timer = lifetime;

        // Set text
        textMesh.text = damage.ToString("F0");

        // Set custom color
        startColor = color;
        textMesh.color = startColor;

        // Set target font size (1.5x larger if crit)
        float baseFontSize = isCrit ? 9f : 6f;
        targetScale = baseFontSize;
        
        // Start small for pop-in animation
        currentScale = 0f;
        textMesh.fontSize = 0f;
        
        // Add slight random horizontal spread
        float randomAngle = Random.Range(-30f, 30f);
        moveDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.up;
        
        gameObject.SetActive(true);
    }
    
    private void Update()
    {
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;
        
        // Pop-in scale animation (elastic overshoot)
        if (currentScale < targetScale)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale * 1.1f, scaleSpeed * Time.deltaTime);
            if (currentScale >= targetScale * 0.99f)
            {
                currentScale = targetScale;
            }
            textMesh.fontSize = currentScale;
        }
        
        // Move in random direction (mostly up)
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
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
