using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to wire up the enemy spawning system
/// </summary>
public class EnemySystemSetup : MonoBehaviour
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Setup Enemy System")]
    public static void SetupEnemySystem()
    {
        // Find the GameObjects
        GameObject gameManager = GameObject.Find("GameManager");
        GameObject enemyPrefab = GameObject.Find("EnemyPrefab");
        GameObject player = GameObject.Find("Player");
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab not found!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        
        // Get components
        EnemyPool pool = gameManager.GetComponent<EnemyPool>();
        EnemySpawner spawner = gameManager.GetComponent<EnemySpawner>();
        
        if (pool == null || spawner == null)
        {
            Debug.LogError("EnemyPool or EnemySpawner component not found on GameManager!");
            return;
        }
        
        // Set up the pool
        SerializedObject poolObj = new SerializedObject(pool);
        poolObj.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        poolObj.ApplyModifiedProperties();
        
        // Set up the spawner
        SerializedObject spawnerObj = new SerializedObject(spawner);
        spawnerObj.FindProperty("enemyPool").objectReferenceValue = pool;
        spawnerObj.FindProperty("player").objectReferenceValue = player.transform;
        spawnerObj.FindProperty("mainCamera").objectReferenceValue = Camera.main;
        spawnerObj.ApplyModifiedProperties();
        
        // Deactivate the enemy prefab so it's not visible in scene
        enemyPrefab.SetActive(false);
        
        Debug.Log("Enemy system setup complete!");
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );
    }
    #endif
}