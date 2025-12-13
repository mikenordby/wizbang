using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to setup health and damage system with 2 enemy types
/// </summary>
public class HealthSystemSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Health & Damage System")]
    private static void SetupHealthSystem()
    {
        // Create EnemyStats folder if it doesn't exist
        string statsPath = "Assets/EnemyStats";
        if (!AssetDatabase.IsValidFolder(statsPath))
        {
            AssetDatabase.CreateFolder("Assets", "EnemyStats");
            Debug.Log("Created Assets/EnemyStats folder");
        }
        
        // Create Blob enemy stats
        EnemyStats blobStats = CreateEnemyStats(
            enemyName: "Blob",
            maxHealth: 10f,
            contactDamage: 10f,
            moveSpeed: 2f,
            xpDrop: 5,
            color: Color.green,
            scale: 0.4f
        );
        string blobPath = $"{statsPath}/BlobStats.asset";
        AssetDatabase.CreateAsset(blobStats, blobPath);
        Debug.Log($"Created {blobPath}");
        
        // Create Skeleton enemy stats
        EnemyStats skeletonStats = CreateEnemyStats(
            enemyName: "Skeleton",
            maxHealth: 25f,
            contactDamage: 15f,
            moveSpeed: 2.5f,
            xpDrop: 12,
            color: new Color(0.9f, 0.9f, 0.9f), // Light gray
            scale: 0.5f
        );
        string skeletonPath = $"{statsPath}/SkeletonStats.asset";
        AssetDatabase.CreateAsset(skeletonStats, skeletonPath);
        Debug.Log($"Created {skeletonPath}");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Add Health component to Player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<Health>();
                Debug.Log("Added Health component to Player");
            }
            
            SerializedObject soHealth = new SerializedObject(playerHealth);
            soHealth.FindProperty("maxHealth").floatValue = 100f;
            soHealth.FindProperty("iFrameDuration").floatValue = 0.5f;
            soHealth.ApplyModifiedProperties();
            Debug.Log("Configured Player Health: 100 HP, 0.5s i-frames");
        }
        
        // Configure EnemyPool with enemy types
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            EnemyPool enemyPool = gameManager.GetComponent<EnemyPool>();
            if (enemyPool != null)
            {
                SerializedObject soPool = new SerializedObject(enemyPool);
                SerializedProperty enemyTypesArray = soPool.FindProperty("enemyTypes");
                
                enemyTypesArray.ClearArray();
                enemyTypesArray.arraySize = 2;
                enemyTypesArray.GetArrayElementAtIndex(0).objectReferenceValue = blobStats;
                enemyTypesArray.GetArrayElementAtIndex(1).objectReferenceValue = skeletonStats;
                
                soPool.ApplyModifiedProperties();
                Debug.Log("Configured EnemyPool with Blob and Skeleton types");
            }
        }
        
        Debug.Log("=== Health & Damage System Setup Complete! ===");
        Debug.Log("- Player: 100 HP, 0.5s i-frames");
        Debug.Log("- Blob: 10 HP, 2 speed, 10 damage, 5 XP");
        Debug.Log("- Skeleton: 25 HP, 2.5 speed, 15 damage, 12 XP");
        Debug.Log("- Projectiles deal 10 damage");
        Debug.Log("- Enemies deal continuous contact damage");
    }
    
    private static EnemyStats CreateEnemyStats(string enemyName, float maxHealth, 
        float contactDamage, float moveSpeed, int xpDrop, Color color, float scale)
    {
        EnemyStats stats = ScriptableObject.CreateInstance<EnemyStats>();
        stats.enemyName = enemyName;
        stats.maxHealth = maxHealth;
        stats.contactDamage = contactDamage;
        stats.moveSpeed = moveSpeed;
        stats.xpDrop = xpDrop;
        stats.color = color;
        stats.scale = scale;
        return stats;
    }
}