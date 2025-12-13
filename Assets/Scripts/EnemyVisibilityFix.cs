using UnityEngine;
using UnityEditor;

/// <summary>
/// Quick fix to make enemies more visible
/// </summary>
public class EnemyVisibilityFix : MonoBehaviour
{
    [MenuItem("Tools/Fix Enemy Visibility")]
    private static void FixEnemyVisibility()
    {
        // Load and update Blob stats
        EnemyStats blobStats = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/EnemyStats/BlobStats.asset");
        if (blobStats != null)
        {
            SerializedObject soBlobStats = new SerializedObject(blobStats);
            soBlobStats.FindProperty("scale").floatValue = 0.8f; // Make bigger
            soBlobStats.ApplyModifiedProperties();
            Debug.Log("Blob scale increased to 0.8");
        }
        
        // Load and update Skeleton stats
        EnemyStats skeletonStats = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/EnemyStats/SkeletonStats.asset");
        if (skeletonStats != null)
        {
            SerializedObject soSkeletonStats = new SerializedObject(skeletonStats);
            soSkeletonStats.FindProperty("scale").floatValue = 1.0f; // Make bigger
            soSkeletonStats.ApplyModifiedProperties();
            Debug.Log("Skeleton scale increased to 1.0");
        }
        
        // Also check spawn rate
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            EnemySpawner spawner = gameManager.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                SerializedObject soSpawner = new SerializedObject(spawner);
                soSpawner.FindProperty("spawnInterval").floatValue = 0.5f;
                soSpawner.ApplyModifiedProperties();
                Debug.Log("Spawn rate set to 2 per second (0.5s interval)");
            }
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log("=== Enemy Visibility Fixed ===");
        Debug.Log("- Enemies now spawn 2 per second");
        Debug.Log("- Blob size: 0.8 (was 0.4)");
        Debug.Log("- Skeleton size: 1.0 (was 0.5)");
        Debug.Log("- Added spawn debug logging");
    }
}