using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Player player;
    
    // Position logging
    private float logTimer = 0f;
    private float logInterval = 2f;
    
private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        
        // Create a simple white square sprite if none exists
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }
    }
    
private void Update()
    {
        if (GameState.IsPaused)
        {
            // Log position during pause to verify we're not moving
            logTimer += Time.unscaledDeltaTime;
            if (logTimer >= logInterval)
            {
                Debug.Log($"[PAUSED] Player position: ({transform.position.x:F2}, {transform.position.y:F2})");
                logTimer = 0f;
            }
            return;
        }
        
        // Position logging during gameplay
        logTimer += Time.deltaTime;
        if (logTimer >= logInterval)
        {
            Debug.Log($"Player position: ({transform.position.x:F2}, {transform.position.y:F2})");
            logTimer = 0f;
        }
        
        // Get WASD input using Keyboard from new Input System
        moveInput = Vector2.zero;
        
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                moveInput.y = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                moveInput.y = -1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                moveInput.x = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                moveInput.x = 1f;
        }
        
        moveInput.Normalize(); // Prevent faster diagonal movement
    }
    
    private void FixedUpdate()
    {
        // Stop movement during pause
        if (GameState.IsPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Apply movement to Rigidbody2D with speed multiplier
        float effectiveSpeed = moveSpeed * (player != null ? player.MoveSpeedMultiplier : 1f);
        rb.linearVelocity = moveInput * effectiveSpeed;
    }
}