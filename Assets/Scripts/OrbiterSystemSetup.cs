using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor tool to setup orbiter projectile system in the scene.
/// </summary>
public class OrbiterSystemSetup : MonoBehaviour
{
    [MenuItem("GameObject/Setup/Create Orbiter System")]
    private static void CreateOrbiterSystem()
    {
        // Find or create GameManager
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found! Create it first.");
            return;
        }
        
        // Find Player
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        
        // Create orbiter prefab
        GameObject orbiterPrefab = new GameObject("OrbiterPrefab");
        orbiterPrefab.transform.position = Vector3.zero;
        orbiterPrefab.transform.localScale = Vector3.one * 0.3f;
        
        // Add sprite renderer
        SpriteRenderer spriteRenderer = orbiterPrefab.AddComponent<SpriteRenderer>();
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        spriteRenderer.color = Color.white;
        
        // Add OrbiterProjectile component
        OrbiterProjectile orbiter = orbiterPrefab.AddComponent<OrbiterProjectile>();
        
        // Make it inactive initially
        orbiterPrefab.SetActive(false);
        
        // Add OrbiterManager to GameManager
        OrbiterManager orbiterManager = gameManager.GetComponent<OrbiterManager>();
        if (orbiterManager == null)
        {
            orbiterManager = gameManager.AddComponent<OrbiterManager>();
        }
        
        // Set references using SerializedObject for proper Unity serialization
        SerializedObject serializedManager = new SerializedObject(orbiterManager);
        serializedManager.FindProperty("orbiterPrefab").objectReferenceValue = orbiterPrefab;
        serializedManager.FindProperty("playerTransform").objectReferenceValue = player.transform;
        serializedManager.FindProperty("maxOrbiters").intValue = 2;
        serializedManager.ApplyModifiedProperties();
        
        // Update CollisionManager reference
        CollisionManager collisionManager = gameManager.GetComponent<CollisionManager>();
        if (collisionManager != null)
        {
            SerializedObject serializedCollision = new SerializedObject(collisionManager);
            serializedCollision.FindProperty("orbiterManager").objectReferenceValue = orbiterManager;
            serializedCollision.ApplyModifiedProperties();
        }
        
        EditorUtility.SetDirty(gameManager);
        EditorUtility.SetDirty(orbiterPrefab);
        
        Debug.Log("Orbiter system created successfully!");
        Selection.activeGameObject = gameManager;
    }
}
#endif
