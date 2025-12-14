using UnityEngine;

/// <summary>
/// Debug utility to test sprite loading and display cache statistics.
/// Attach to any GameObject and press F4 in Play mode to see sprite cache info.
/// </summary>
public class SpriteLoaderDebug : MonoBehaviour
{
    private void Update()
    {
        #if UNITY_EDITOR
        // F4: Show sprite cache statistics
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SpriteLoader.LogCacheStats();
        }
        
        // F5: Toggle procedural fallback
        if (Input.GetKeyDown(KeyCode.F5))
        {
            // This would need to be implemented in SpriteLoader
            Debug.Log("[SpriteLoaderDebug] F5: Procedural fallback toggle - implement in SpriteLoader if needed");
        }
        
        // F6: Clear sprite cache (for testing)
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SpriteLoader.ClearCache();
            Debug.Log("[SpriteLoaderDebug] Sprite cache cleared - sprites will reload on next use");
        }
        #endif
    }
    
    private void Start()
    {
        Debug.Log("[SpriteLoaderDebug] Press F4 to show sprite cache stats, F6 to clear cache");
    }
}
